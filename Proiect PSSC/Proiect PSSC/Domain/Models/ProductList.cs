using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models
{
    public record ProductList
    {
        public List<Product> productList { get; private set; }

        public ProductList(List<Product> productList)
        {
            this.productList = productList;
        }
    }
}
