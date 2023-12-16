using LanguageExt;
using Proiect_PSSC.Domain.Commands;
using Proiect_PSSC.Domain.Models.Domain_Objects;
using Proiect_PSSC.Domain.Workflows;
using Proiect_PSSC.Domain.Models.Events;
using static LanguageExt.Prelude;

namespace Proiect_PSSC
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var listOfProducts = ReadListOfProducts();
            OrderProcessingCommand command = new(listOfProducts, "1");
            OrderProcessingWorkflow workflow = new();
            var result = await workflow.ExecuteAsync(command, CheckProductExists, GetAvailableProducts);

            result.Match(
                    whenOrderProcessingFailedEvent: @event =>
                    {
                        Console.WriteLine($"Processing failed: {@event.Reason}");
                        return @event;
                    },
                    whenOrderProcessingSuccessEvent: @event =>
                    {
                        Console.WriteLine("Processing successful");
                        ShowList(@event.ProductList);
                        return @event;
                    }
                );
        }

        private static void ShowList(IReadOnlyCollection<AvailableProduct> products)
        {
            foreach (AvailableProduct product in products )
            {
                Console.WriteLine(product.ToString());
            }
        }

        private static List<UnvalidatedProduct> ReadListOfProducts()
        {
            List<UnvalidatedProduct> listOfProducts = new();
            do
            {
                var productId = ReadValue("Product id: ");
                if (string.IsNullOrEmpty(productId))
                {
                    break;
                }

                var productName = ReadValue("Product name: ");
                if (string.IsNullOrEmpty(productName))
                {
                    break;
                }

                var productQuantity = ReadValue("Product quantity: ");
                if (string.IsNullOrEmpty(productQuantity))
                {
                    break;
                }

                var productPrice = ReadValue("Product price: ");
                if (string.IsNullOrEmpty(productPrice))
                {
                    break;
                }

                listOfProducts.Add(new(productId, productName, productQuantity, productPrice));
            } while (true);
            return listOfProducts;
        }

        private static string? ReadValue(string prompt)
        {
            Console.Write(prompt);
            return Console.ReadLine();
        }

        private static TryAsync<bool> CheckProductExists(ProductId productId)
        {
            Func<Task<bool>> func = async () =>
            {
                return true;
            };
            return TryAsync(func);
        }

        private static TryAsync<List<ValidatedProduct>> GetAvailableProducts(List<ProductId> productIds)
        {
            List<ValidatedProduct> validatedProducts = new();
            ValidatedProduct product1 = new(new("P111"), new("a"), new(4), new(3));
            ValidatedProduct product2 = new(new("P112"), new("b"), new(4), new(3));
            validatedProducts.Add(product1);
            //validatedProducts.Add(product2);

            Func<Task<List<ValidatedProduct>>> func = async () =>
            {
                return validatedProducts;
            };
            return TryAsync(func);
        }
    }
}