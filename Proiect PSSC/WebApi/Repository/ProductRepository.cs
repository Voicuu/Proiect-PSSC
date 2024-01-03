using Firebase.Database;
using Firebase.Database.Query;
using Proiect_PSSC.Firebase;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Dto;

namespace WebApi.Repository
{
    public class ProductRepository
    {
        private FirebaseClient client;

        public ProductRepository()
        {
            this.client = FirebaseConfig.GetFirebaseClient();
        }

        public async Task<List<Product>> GetAllProducts()
        {
            var firebaseObjects = await client
               .Child("Products")
               .OnceAsync<FirebaseProduct>();

            return firebaseObjects.Select(firebaseObject => new Product
            {
                Id = firebaseObject.Object.ProductId.Value,
                Name = firebaseObject.Object.ProductName.Value,
                Quantity = int.Parse(firebaseObject.Object.ProductQuantity.Value),
                Price = decimal.Parse(firebaseObject.Object.ProductPrice.Value)
            }).ToList();
        }

        public async Task<Product> GetProductById(string productId)
        {
            var firebaseObject = await client
               .Child("Products")
               .Child(productId)
               .OnceSingleAsync<FirebaseProduct>();
            
            if (firebaseObject == null)
            {
                return null;
            }

            return new Product
            {
                Id = firebaseObject.ProductId.Value,
                Name = firebaseObject.ProductName.Value,
                Quantity = int.Parse(firebaseObject.ProductQuantity.Value),
                Price = decimal.Parse(firebaseObject.ProductPrice.Value)
            };
        }
    }

    public class FirebaseProduct
    {
        public ValueWrapper ProductId { get; set; }
        public ValueWrapper ProductName { get; set; }
        public ValueWrapper ProductPrice { get; set; }
        public ValueWrapper ProductQuantity { get; set; }
    }

    public class ValueWrapper
    {
        public string Value { get; set; }
    }
}