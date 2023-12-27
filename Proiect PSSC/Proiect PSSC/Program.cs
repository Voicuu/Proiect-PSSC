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

            var userExists = await CheckUserExistsAsync(clientId, password, firebaseClient);
            if (userExists)
            {
                await Execute(clientId, firebaseClient);
            }
            else
            {
                Console.WriteLine("User not found or invalid credentials");
            }

            Console.ReadLine();
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

                                    SaveOrderToFirebase(@event.ProductList, @event.Total, @event.ClientId, firebaseClient);

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
            try
            {
                var projectRoot = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\"));
                var billsDirectory = Path.Combine(projectRoot, "Bills");
                Directory.CreateDirectory(billsDirectory);

                var fileName = $"Invoice_{clientId}_{DateTime.Now:ddMMyyyyHHmmss}.txt";
                var filePath = Path.Combine(billsDirectory, fileName);

                using (var writer = new StreamWriter(filePath))
                {
                    writer.WriteLine(new string('-', 40));
                    writer.WriteLine("INVOICE".PadLeft(20));
                    writer.WriteLine($"Client ID: {clientId}".PadLeft(20));
                    writer.WriteLine($"Date: {DateTime.Now:dd-MM-yyyy HH:mm:ss}".PadLeft(28));
                    writer.WriteLine(new string('-', 40));
                    writer.WriteLine("{0,-20} {1,5} {2,8}", "Item", "Qty", "Price");
                    writer.WriteLine(new string('-', 40));

                    foreach (var product in products)
                    {
                        writer.WriteLine("{0,-20} {1,5} {2,8:C}", product.ProductName.Value, product.ProductQuantity.Value, product.ProductPrice.Value);
                    }

                    writer.WriteLine(new string('-', 40));
                    writer.WriteLine("{0,-26} {1,8:C}", "Total paid:", total);
                    writer.WriteLine(new string('-', 40));
                }

                //Console.WriteLine($"Invoice written successfully to {filePath}");
                //Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing invoice to file: {ex.Message}");
            }
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
                //Console.WriteLine($"Entered Payment Method: {paymentMethod}");

            } while (paymentMethod != "cash" && paymentMethod != "card");

            return paymentMethod;
        }

        private static void ShowList(IReadOnlyCollection<AvailableProduct> products)
        {
            foreach (AvailableProduct product in products)
            {
                Console.WriteLine(product.ToString());
            }
        }

        private static List<UnvalidatedProduct> ReadListOfProducts(string prompt = "Product id (or type 'done' to finish): ")
        {
            List<UnvalidatedProduct> listOfProducts = new();

            while (true)
            {
                var productId = ReadValue(prompt);

                if (string.IsNullOrWhiteSpace(productId) || productId.ToLower() == "done")
                {
                    break;
                }

                var productName = ReadValue("Product name: ");
                var productQuantity = ReadValue("Product quantity: ");
                var productPrice = ReadValue("Product price: ");

                listOfProducts.Add(new(productId, productName, productQuantity, productPrice));
            }

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

        private static async Task<bool> CheckUserExistsAsync(string clientId, string password, FirebaseClient firebaseClient)
        {
            var userExistsOption = await GetUserAsync(clientId, password, firebaseClient);

            return userExistsOption.IfNone(false);
        }

        private static async Task<Option<bool>> GetUserAsync(string clientId, string password, FirebaseClient firebaseClient)
        {
            var userDataSnapshot = await firebaseClient.Child("Users").Child(clientId).Child("password").OnceSingleAsync<string>();
            if (userDataSnapshot != null && password == userDataSnapshot)
            {
                return Some(true);
            }
            return None;
        }

        private static async Task SaveOrderToFirebase(IReadOnlyCollection<AvailableProduct> products, decimal total, string clientId, FirebaseClient firebaseClient)
        {
            var order = new
            {
                ClientId = clientId,
                Date = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"),
                Items = products.ToDictionary(
                    product => product.ProductId.Value,
                    product => new
                    {
                        ProductName = product.ProductName.Value,
                        Quantity = product.ProductQuantity.Value,
                        Price = product.ProductPrice.Value
                    }),
                Total = total
            };

            var orderKey = await firebaseClient.Child("Orders").PostAsync(order);
            Console.WriteLine($"Order saved to Firebase with key: {orderKey.Key}");
        }

    }
}