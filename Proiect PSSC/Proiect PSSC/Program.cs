using Proiect_PSSC.Domain.Models.Domain_Objects;

namespace Proiect_PSSC
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ProductPrice price = new(2);
            ProductQuantity quantity = new(2);
            Console.WriteLine(quantity * price);
        }
    }
}