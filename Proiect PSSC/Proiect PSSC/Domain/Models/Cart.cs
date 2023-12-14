using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Proiect_PSSC.Domain.Models.Status;

namespace Proiect_PSSC.Domain.Models
{
    public record Cart
    {
        public IStatus?         status { get; private init; }
        public List<Product>    productList { get; private init; }

        public Cart(List<Product> productList)
        {
            this.productList = productList;
        }
        public Cart(List<Product> productList, IStatus status)
        {
            this.status = status;
            this.productList = productList;
        }
    }
}
