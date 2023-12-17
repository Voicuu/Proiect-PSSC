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


namespace Proiect_PSSC.Domain.Operations
{
    public static class OrderProcessingOperation
    {
        public static Task<IOrderState> ValidateProducts(Func<ProductId, Task<TryAsync<bool>>> checkProductExist,
                                                         UnvalidatedOrder unvalidatedOrder) =>
            unvalidatedOrder.ProductList
                            .Select(ValidateProduct(checkProductExist))
                            .Aggregate(CreateEmptyValidatedProductsList().ToAsync(), ReduceValidProducts)
                            .MatchAsync(
                                  Right: validatedOrder => new ValidatedOrder(validatedOrder, unvalidatedOrder.ClientId),
                                  LeftAsync: errorMessage => Task.FromResult((IOrderState)new InvalidatedOrder(unvalidatedOrder.ProductList, errorMessage,unvalidatedOrder.ClientId))
                            );

        private static Func<UnvalidatedProduct, EitherAsync<string, ValidatedProduct>> ValidateProduct(Func<ProductId, Task<TryAsync<bool>>> checkProductExist) =>
            unvalidatedProduct => ValidateProduct(checkProductExist, unvalidatedProduct);

        private static EitherAsync<string, ValidatedProduct> ValidateProduct(Func<ProductId, Task<TryAsync<bool>>> checkProductExist, 
                                                                             UnvalidatedProduct unvalidatedProduct) =>
            from productName in ProductName.TryParse(unvalidatedProduct.ProductName)
                                   .ToEitherAsync(() => $"Invalid name ({unvalidatedProduct.ProductName})")
            from productQuantity in ProductQuantity.TryParse(unvalidatedProduct.ProductQuantity)
                                   .ToEitherAsync(() => $"Invalid quantity ({unvalidatedProduct.ProductQuantity})")
            from productPrice in ProductPrice.TryParse(unvalidatedProduct.ProductPrice)
                                   .ToEitherAsync(() => $"Invalid price ({unvalidatedProduct.ProductPrice})")
            from productId in ProductId.TryParse(unvalidatedProduct.ProductId)
                               .ToEitherAsync(() => $"Invalid id ({unvalidatedProduct.ProductId})")
            from productExist in checkProductExist(productId).Result
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
                                                          Func<ProductId, TryAsync<ValidatedProduct>> getProductById) =>
            orderState.MatchAsync(
                whenUnvalidatedOrder: unvalidatedOrder => Task.FromResult<IOrderState>(unvalidatedOrder),
                whenInvalidatedOrder: invalidatedOrder => Task.FromResult<IOrderState>(invalidatedOrder),
                whenValidatedOrder: validatedOrder => CalculateTotalProductPriceAndCheckAvailability(validatedOrder, getProductById),
                whenAvailableOrder: availableOrder => Task.FromResult<IOrderState>(availableOrder)
                );

        private static async Task<IOrderState> CalculateTotalProductPriceAndCheckAvailability(ValidatedOrder validatedOrder,
                                                                                              Func<ProductId, TryAsync<ValidatedProduct>> getProductById)
        {
            bool success = await CheckAvailability(validatedOrder.ProductList, getProductById);

            if (success)
            {
                return new AvailableOrder(validatedOrder.ProductList
                                          .Select(CalculateTotalProductPrice)
                                          .ToList()
                                          .AsReadOnly(),
                                          validatedOrder.ClientId);
            }

            return validatedOrder;
        }

        private static async Task<bool> CheckAvailability(IReadOnlyCollection<ValidatedProduct> productList,
                                                          Func<ProductId, TryAsync<ValidatedProduct>> getProductById)
        {
            foreach (ValidatedProduct product in productList)
            {
                var result = await getProductById(product.ProductId);
                ValidatedProduct requestedProduct = result.Match(
                    Succ: product => product,
                    Fail: ex => {
                        return new(null, null, new(0), null);
                    }
                );

                if (requestedProduct.ProductQuantity.Value < product.ProductQuantity.Value)
                {
                    return false;
                }
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
