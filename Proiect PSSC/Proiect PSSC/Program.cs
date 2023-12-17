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
            OrderProcessingCommand command = new(listOfProducts, "10");
            OrderProcessingWorkflow workflow = new();
            var result = await workflow.ExecuteAsync(command, CheckProductExists, GetProductById);

            result.Match(
                    whenOrderProcessingFailedEvent: @event =>
                    {
                        Console.WriteLine($"Processing failed: {@event.Reason}");
                        return @event;
                    },
                    whenOrderProcessingSuccessEvent: @event =>
                    {
                        string paymentMethod;
                        Console.WriteLine("Processing successful");
                        ShowList(@event.ProductList);

                        Console.WriteLine();

                        paymentMethod = ChoosePaymentMethod();

                        BillingCommand billingCommand = new(@event.ProductList, @event.ClientId, paymentMethod);

                        BillingWorkflow billingWorkflow = new();

                        var billingResult = billingWorkflow.Execute(billingCommand);

                        billingResult.Match(
                                whenBillingFailedEvent: @event =>
                                {
                                    Console.WriteLine($"Payment failed: {@event.Reason}");
                                    return @event;
                                },
                                whenBillingSuccessEvent: @event =>
                                {
                                    Console.WriteLine($"Payment succeeded, amount payed: {@event.Total}");

                                    Console.WriteLine();

                                    ShippingCommand shippingCommand = new(@event.ProductList, @event.Total, @event.ClientId);
                                    ShippingWorkflow shippingWorkflow = new();

                                    var shippingResult = shippingWorkflow.Execute(shippingCommand);

                                    shippingResult.Match(
                                        whenShippingSuccessEvent: @event =>
                                        {
                                            Console.WriteLine($"Successful delivery for client: {@event.ClientId}\n{@event.SuccessMessage}");
                                            return @event;
                                        }
                                        );

                                    return @event;
                                }
                            );


                        return @event;
                    }
                );
        }

        private static string ChoosePaymentMethod()
        {
            string paymentMethod = "";
            do
            {
                paymentMethod = ReadValue("How would you like to pay: (Card/Cash)\nYour choice: ").ToLower();
                
            } while (paymentMethod!="cash" && paymentMethod != "card");

            return paymentMethod;
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

        private static TryAsync<ValidatedProduct> GetProductById(ProductId productId)
        {
            ValidatedProduct product = new(new("P111"), new("a"), new(4), new(3));

            Func<Task<ValidatedProduct>> func = async () =>
            {
                return product;
            };
            return TryAsync(func);
        }
    }
}