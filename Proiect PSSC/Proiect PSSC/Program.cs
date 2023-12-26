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
using LanguageExt.Pipes;

namespace Proiect_PSSC
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var firebaseClient = FirebaseConfig.GetFirebaseClient();

            string? clientId = ReadValue("Enter client id: ");
            Console.Write("Enter password: ");
            string password = ReadPassword();

            var res = await CheckUserExistsAsync(clientId, password, firebaseClient);

            res.Match(
                Succ: boolVal => Execute(clientId, firebaseClient),
                Fail: ex => Console.WriteLine("User not found")
                );
        }

        private static async Task Execute(string clientId, FirebaseClient firebaseClient)
        {
            var listOfProducts = ReadListOfProducts();
            OrderProcessingCommand command = new(listOfProducts, clientId, firebaseClient);
            OrderProcessingWorkflow workflow = new();
            var result = await workflow.ExecuteAsync(command, CheckProductExistsAsync, GetProductAsync);

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

                                    PrintBillToFile(@event.ProductList, @event.Total, @event.ClientId);

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

        private static void PrintBillToFile(IReadOnlyCollection<AvailableProduct> products, decimal total, string clientId)
        {
            string path = $"Bills/{clientId}{DateTime.Now}.txt";
            File.Create(path);

            File.AppendText(path).WriteLine("Order details");
            File.AppendText(path).WriteLine();
            foreach (var product in products)
            {
                File.AppendText(path).WriteLine($"{product.ProductName.Value}");
                File.AppendText(path).WriteLine($"{product.ProductQuantity.Value} * {product.ProductPrice.Value} = {product.TotalProductPrice.Value}$");
                File.AppendText(path).WriteLine();
            }

            File.AppendText(path).WriteLine($"Total payed: {total}$");
        }

        private static string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, password.Length - 1);
                    Console.Write("\b \b");
                }
                else if (key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }

            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();

            return password;
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

        private static async Task<TryAsync<bool>> CheckProductExistsAsync(ProductId productId, FirebaseClient firebaseClient)
        {
            var productOption = await GetProductAsync(productId, firebaseClient);

            return productOption.Match(
                Some: product => TryAsync(() => Task.FromResult(true)),
                None: () => TryAsync(() => Task.FromException<bool>(new Exception("Product not found")))
            );
        }

        private static async Task<Option<ValidatedProduct>> GetProductAsync(ProductId productId, FirebaseClient firebaseClient)
        {
            //var firebaseClient = FirebaseConfig.GetFirebaseClient();
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

        private static async Task<TryAsync<bool>> CheckUserExistsAsync(string clientId, string password, FirebaseClient firebaseClient)
        {
            var userExists = await GetUserAsync(clientId, password, firebaseClient);

            return userExists.Match(
                Some: exists => TryAsync(() => Task.FromResult(true)),
                None: () => TryAsync(() => Task.FromException<bool>(new Exception("User not found")))
            );
        }

        private static async Task<Option<bool>> GetUserAsync(string clientId, string password, FirebaseClient firebaseClient)
        {
            //var firebaseClient = FirebaseConfig.GetFirebaseClient();
            var userDataSnapshot = await firebaseClient.Child("Users").Child(clientId).Child("password").OnceSingleAsync<string>();

            if (userDataSnapshot != null)
            {
                var clientPassword = userDataSnapshot;
                if (!string.IsNullOrWhiteSpace(clientPassword))
                {
                    if (password == clientPassword)
                    {
                        return Some(true);
                    }
                }
            }

            return None;
        }
    }
}