using LanguageExt;
using Proiect_PSSC.Domain.Models;
using Proiect_PSSC.Domain.Models.Domain_Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Proiect_PSSC.Domain.Models.OrderState;
using static LanguageExt.Prelude;
using LanguageExt.Common;
using LanguageExt.Pretty;
using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Proiect_PSSC.Domain.Operations
{
    public static class OrderProcessingOperation
    {
        public static Task<IOrderState> ValidateProducts(Func<ProductId, FirebaseClient, Task<TryAsync<bool>>> checkProductExist,
                                                         UnvalidatedOrder unvalidatedOrder,
                                                         FirebaseClient firebaseClient) =>
            unvalidatedOrder.ProductList
                            .Select(ValidateProduct(checkProductExist, firebaseClient))
                            .Aggregate(CreateEmptyValidatedProductsList().ToAsync(), ReduceValidProducts)
                            .MatchAsync(
                                  Right: validatedOrder => new ValidatedOrder(validatedOrder, unvalidatedOrder.ClientId),
                                  LeftAsync: errorMessage => Task.FromResult((IOrderState)new InvalidatedOrder(unvalidatedOrder.ProductList, errorMessage,unvalidatedOrder.ClientId))
                            );

        private static Func<UnvalidatedProduct, EitherAsync<string, ValidatedProduct>> ValidateProduct(Func<ProductId, FirebaseClient, Task<TryAsync<bool>>> checkProductExist, 
                                                                                                       FirebaseClient firebaseClient) =>
            unvalidatedProduct => ValidateProduct(checkProductExist, unvalidatedProduct, firebaseClient);

        private static EitherAsync<string, ValidatedProduct> ValidateProduct(Func<ProductId, FirebaseClient, Task<TryAsync<bool>>> checkProductExist, 
                                                                             UnvalidatedProduct unvalidatedProduct,
                                                                             FirebaseClient firebaseClient) =>
            from productName in ProductName.TryParse(unvalidatedProduct.ProductName)
                                   .ToEitherAsync(() => $"Invalid name ({unvalidatedProduct.ProductName})")
            from productQuantity in ProductQuantity.TryParse(unvalidatedProduct.ProductQuantity)
                                   .ToEitherAsync(() => $"Invalid quantity ({unvalidatedProduct.ProductQuantity})")
            from productPrice in ProductPrice.TryParse(unvalidatedProduct.ProductPrice)
                                   .ToEitherAsync(() => $"Invalid price ({unvalidatedProduct.ProductPrice})")
            from productId in ProductId.TryParse(unvalidatedProduct.ProductId)
                               .ToEitherAsync(() => $"Invalid id ({unvalidatedProduct.ProductId})")
            from productExist in checkProductExist(productId, firebaseClient).Result
                                   .ToEither(error => error.ToString())
            select new ValidatedProduct(productId, productName, productQuantity, productPrice);

        private static Either<string, List<ValidatedProduct>> CreateEmptyValidatedProductsList() =>
            Right(new List<ValidatedProduct>());

        private static EitherAsync<string, List<ValidatedProduct>> ReduceValidProducts(EitherAsync<string, List<ValidatedProduct>> acc, 
                                                                                       EitherAsync<string, ValidatedProduct> next) =>
            from list in acc
            from nextProduct in next
            select AppendValidProduct(list, nextProduct);

        private static List<ValidatedProduct> AppendValidProduct(this List<ValidatedProduct> list, 
                                                                 ValidatedProduct validProduct)
        {
            list.Add(validProduct);
            return list;
        }

        public static Task<IOrderState> CheckAvailability(IOrderState orderState,
                                                          Func<ProductId, FirebaseClient, Task<Option<ValidatedProduct>>> getProductById,
                                                          FirebaseClient firebaseClient) =>
            orderState.MatchAsync(
                whenUnvalidatedOrder: unvalidatedOrder => Task.FromResult<IOrderState>(unvalidatedOrder),
                whenInvalidatedOrder: invalidatedOrder => Task.FromResult<IOrderState>(invalidatedOrder),
                whenValidatedOrder: validatedOrder => CalculateTotalProductPriceAndCheckAvailability(validatedOrder, getProductById, firebaseClient),
                whenAvailableOrder: availableOrder => Task.FromResult<IOrderState>(availableOrder)
                );

        private static async Task<IOrderState> CalculateTotalProductPriceAndCheckAvailability(ValidatedOrder validatedOrder,
                                                                                              Func<ProductId, FirebaseClient, Task<Option<ValidatedProduct>>> getProductById,
                                                                                              FirebaseClient firebaseClient)
        {
            bool success = await CheckAvailabilityAndUpdateQuantities(validatedOrder.ProductList, getProductById, firebaseClient);

            if (success)
            {
                List<ValidatedProduct> dbProducts = new();
                foreach(var product in validatedOrder.ProductList)
                {
                    var dbProduct =  await getProductById(product.ProductId, firebaseClient);
                    dbProduct.Match(
                        Some: result => dbProducts.Add(result),
                        None: () => Console.WriteLine($"Product {product.ProductId.Value} not found.")
                        );
                }
                return new AvailableOrder(dbProducts
                                          .Select(CalculateTotalProductPrice)
                                          .ToList()
                                          .AsReadOnly(),
                                          validatedOrder.ClientId);
            }

            return validatedOrder;
        }

        private static async Task<bool> CheckAvailability(IReadOnlyCollection<ValidatedProduct> productList,
                                                          Func<ProductId, FirebaseClient, Task<Option<ValidatedProduct>>> getProductById, 
                                                          FirebaseClient firebaseClient)
        {
            foreach (ValidatedProduct product in productList)
            {
                var resultOption = await getProductById(product.ProductId, firebaseClient);

                if (resultOption.IsNone)
                {
                    Console.WriteLine("Not enough products.");
                    return false;
                }

                var requestedProduct = resultOption.IfNone(() => throw new InvalidOperationException("Product not found"));

                if (requestedProduct.ProductQuantity.Value < product.ProductQuantity.Value)
                {
                    return false;
                }
            }

            return true;
        }

        private static async Task<bool> CheckAvailabilityAndUpdateQuantities(IReadOnlyCollection<ValidatedProduct> productList,
                                                                             Func<ProductId, FirebaseClient, Task<Option<ValidatedProduct>>> getProductById,
                                                                             FirebaseClient firebaseClient)
        {
            // First, check the availability of all products in the order.
            foreach (var product in productList)
            {
                var productOption = await getProductById(product.ProductId, firebaseClient);

                if (productOption.IsNone) return false; // Product doesn't exist.

                var existingProduct = productOption.IfNone(() => throw new InvalidOperationException("Product not found"));

                // Check if enough stock is available.
                if (existingProduct.ProductQuantity.Value < product.ProductQuantity.Value)
                {
                    Console.WriteLine($"Not enough stock for product {product.ProductId.Value}.");
                    return false;
                }
            }

            // All products are available, proceed to update the quantities.
            foreach (var product in productList)
            {
                var productOption = await getProductById(product.ProductId, firebaseClient);

                if (productOption.IsNone) throw new InvalidOperationException("Product not found");

                var existingProduct = productOption.IfNone(() => throw new InvalidOperationException("Product not found"));

                // Calculate the new quantity.
                var newQuantity = existingProduct.ProductQuantity.Value - product.ProductQuantity.Value;

                // Update the product quantity in Firebase.
                await firebaseClient
                    .Child("Products")
                    .Child(product.ProductId.Value)
                    .Child("ProductQuantity")
                    .PutAsync(new ProductQuantity(newQuantity));
            }

            return true;
        }



        private static AvailableProduct CalculateTotalProductPrice(ValidatedProduct validProduct) =>
            new AvailableProduct(validProduct.ProductId,
                                 validProduct.ProductName,
                                 validProduct.ProductQuantity,
                                 validProduct.ProductPrice,
                                 validProduct.ProductPrice * validProduct.ProductQuantity);

    }

}
