using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models
{
    public record Product
    {
        public string productID { get; private init; }
        public string name { get; private set; }
        public string productCode { get; private set; }
        public double price { get; private set; }
        public double quantity { get; private set; }

        public Product(string productID, string name, string productCode, double price, double quantity)
        {
            this.productID = productID;
            this.name = name;
            this.productCode = productCode;
            this.price = price;
            this.quantity = quantity;
        }
    }
}
