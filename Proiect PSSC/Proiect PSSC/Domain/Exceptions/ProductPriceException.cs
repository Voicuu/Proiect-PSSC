using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Exceptions
{
    public class ProductPriceException : Exception
    {
        public ProductPriceException(string message) : base(message) { }
    }
}
