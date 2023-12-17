using LanguageExt;
using Proiect_PSSC.Domain.Commands;
using Proiect_PSSC.Domain.Models.Domain_Objects;
using Proiect_PSSC.Domain.Workflows;
using Proiect_PSSC.Domain.Models.Events;
using static LanguageExt.Prelude;
using Firebase.Database;
using Proiect_PSSC.Firebase;
using Firebase.Database.Query;
using Newtonsoft.Json;

namespace Proiect_PSSC
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var listOfProducts = ReadListOfProducts();
            OrderProcessingCommand command = new(listOfProducts, "10");
            OrderProcessingWorkflow workflow = new();
            var result = await workflow.ExecuteAsync(command, CheckProductExistsAsync, GetProductById);

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

        private static async Task<TryAsync<bool>> CheckProductExistsAsync(ProductId productId)
        {
            var productOption = await GetProductAsync(productId);

            return productOption.Match(
                Some: product => TryAsync(() => Task.FromResult(true)),
                None: () => TryAsync(() => Task.FromException<bool>(new Exception("Product not found")))
            );
        }

        public static async Task<Option<ValidatedProduct>> GetProductAsync(ProductId productId)
        {
            var firebaseClient = FirebaseConfig.GetFirebaseClient();
            var productDataSnapshot = await firebaseClient.Child("Products").Child(productId.Value).OnceSingleAsync<ProductDto>();

            if (productDataSnapshot != null)
            {
                var productData = productDataSnapshot;
                if (!string.IsNullOrWhiteSpace(productData.ProductName?.Value) &&
                    !string.IsNullOrWhiteSpace(productData.ProductPrice?.Value) &&
                    !string.IsNullOrWhiteSpace(productData.ProductQuantity?.Value))
                {
                    if (int.TryParse(productData.ProductQuantity.Value, out var quantity) &&
                        decimal.TryParse(productData.ProductPrice.Value, out var price))
                    {
                        var validatedProduct = new ValidatedProduct(
                            new ProductId(productData.ProductId.Value),
                            new ProductName(productData.ProductName.Value),
                            new ProductQuantity(quantity),
                            new ProductPrice(price)
                        );

                        return Some(validatedProduct);
                    }
                }
            }

            return None;
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