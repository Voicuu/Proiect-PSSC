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


namespace Proiect_PSSC.Domain.Operations
{
    public static class OrderProcessingOperation
    {
        public static Task<IOrderState> ValidateProducts(Func<ProductId, TryAsync<bool>> checkProductExist,
                                                         UnvalidatedOrder unvalidatedOrder) =>
            unvalidatedOrder.ProductList
                            .Select(ValidateProduct(checkProductExist))
                            .Aggregate(CreateEmptyValidatedProductsList().ToAsync(), ReduceValidProducts)
                            .MatchAsync(
                                  Right: validatedOrder => new ValidatedOrder(validatedOrder, unvalidatedOrder.ClientId),
                                  LeftAsync: errorMessage => Task.FromResult((IOrderState)new InvalidatedOrder(unvalidatedOrder.ProductList, errorMessage,unvalidatedOrder.ClientId))
                            );

        private static Func<UnvalidatedProduct, EitherAsync<string, ValidatedProduct>> ValidateProduct(Func<ProductId, TryAsync<bool>> checkProductExist) =>
            unvalidatedProduct => ValidateProduct(checkProductExist, unvalidatedProduct);

        private static EitherAsync<string, ValidatedProduct> ValidateProduct(Func<ProductId, TryAsync<bool>> checkProductExist, 
                                                                             UnvalidatedProduct unvalidatedProduct) =>
            from productName in ProductName.TryParse(unvalidatedProduct.ProductName)
                                   .ToEitherAsync(() => $"Invalid name ({unvalidatedProduct.ProductName})")
            from productQuantity in ProductQuantity.TryParse(unvalidatedProduct.ProductQuantity)
                                   .ToEitherAsync(() => $"Invalid quantity ({unvalidatedProduct.ProductQuantity})")
            from productPrice in ProductPrice.TryParse(unvalidatedProduct.ProductPrice)
                                   .ToEitherAsync(() => $"Invalid price ({unvalidatedProduct.ProductPrice})")
            from productId in ProductId.TryParse(unvalidatedProduct.ProductId)
                               .ToEitherAsync(() => $"Invalid id ({unvalidatedProduct.ProductId})")
            from productExist in checkProductExist(productId)
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
                                                          Func<List<ProductId>, TryAsync<List<ValidatedProduct>>> getAvailableProducts) =>
            orderState.MatchAsync(
                whenUnvalidatedOrder: unvalidatedOrder => Task.FromResult<IOrderState>(unvalidatedOrder),
                whenInvalidatedOrder: invalidatedOrder => Task.FromResult<IOrderState>(invalidatedOrder),
                whenValidatedOrder: validatedOrder => CalculateTotalProductPriceAndCheckAvailability(validatedOrder, getAvailableProducts),
                whenAvailableOrder: availableOrder => Task.FromResult<IOrderState>(availableOrder)
                );

        private static async Task<IOrderState> CalculateTotalProductPriceAndCheckAvailability(ValidatedOrder validatedOrder,
                                                       Func<List<ProductId>, TryAsync<List<ValidatedProduct>>> getAvailableProducts)
        {
            List<ProductId> idsToCheck = BuildIdsToCheck(validatedOrder.ProductList);
            bool fail = false;
            var result = await getAvailableProducts(idsToCheck);
            List<ValidatedProduct> availableProducts = result.Match(
                Succ: products => products,
                Fail: ex => {
                    fail = true;
                    return new List<ValidatedProduct>(); 
                }
            );


            if (availableProducts.Count == idsToCheck.Count && fail == false)
            {
                return new AvailableOrder(availableProducts
                                          .Select(CalculateTotalProductPrice)
                                          .ToList()
                                          .AsReadOnly(),
                                          validatedOrder.ClientId);
            }

            return validatedOrder;
        }

        private static List<ProductId> BuildIdsToCheck(IReadOnlyCollection<ValidatedProduct> productList)
        {
            List<ProductId> productIds = new();

            foreach (ValidatedProduct product in productList)
            {
                productIds.Add(product.ProductId);
            }

            return productIds;
        }

        private static AvailableProduct CalculateTotalProductPrice(ValidatedProduct validProduct) =>
            new AvailableProduct(validProduct.ProductId,
                                 validProduct.ProductName,
                                 validProduct.ProductQuantity,
                                 validProduct.ProductPrice,
                                 validProduct.ProductPrice * validProduct.ProductQuantity);
    }
}
