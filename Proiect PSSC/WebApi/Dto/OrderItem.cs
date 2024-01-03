namespace WebApi.Dto
{
    public class OrderItem
    {
        public string Date { get; set; }
        public List<Product> Items { get; set; }
        public decimal Total { get; set; }
    }
}
