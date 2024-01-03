using Proiect_PSSC.Domain.Models.Domain_Objects;

namespace WebApi.Dto
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
