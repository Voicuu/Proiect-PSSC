using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Firebase
{
    public class ProductDto
    {
        public ValueDto ProductId { get; set; }
        public ValueDto ProductName { get; set; }
        public ValueDto ProductPrice { get; set; }
        public ValueDto ProductQuantity { get; set; }
    }

    public class ValueDto
    {
        public string Value { get; set; }
    }



}
