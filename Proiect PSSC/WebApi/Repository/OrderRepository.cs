using Firebase.Database;
using Proiect_PSSC.Firebase;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Dto;
using System.Collections.Generic;
using Firebase.Database.Query;

namespace WebApi.Repository
{
    public class OrderRepository
    {
        private FirebaseClient client;

        public OrderRepository()
        {
            this.client = FirebaseConfig.GetFirebaseClient();
        }

        public async Task<OrderItem> GetOrderItem(string clientId, string orderId)
        {
            // Fetch the order data for the given clientId and orderId
            var orderData = await client
                .Child("Orders")
                .Child(clientId) // Assuming orders are grouped under client IDs
                .Child(orderId)
                .OnceSingleAsync<FirebaseOrder>();

            // If the order doesn't exist, return null
            if (orderData == null)
            {
                return null;
            }

            // Convert the fetched order data to OrderItem
            var orderItem = new OrderItem
            {
                Date = orderData.Date,
                Items = orderData.Items.Select(kv => new Product
                {
                    Id = kv.Key, // Assuming the key is the product ID
                    Name = kv.Value.ProductName,
                    Quantity = kv.Value.Quantity,
                    Price = kv.Value.Price
                }).ToList(),
                Total = orderData.Total
            };

            return orderItem;
        }
    }

    // Assuming the structure of your order data in Firebase is represented by this class
    public class FirebaseOrder
    {
        public string Date { get; set; }
        public Dictionary<string, FirebaseProductItem> Items { get; set; }
        public decimal Total { get; set; }
    }

    public class FirebaseProductItem
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}