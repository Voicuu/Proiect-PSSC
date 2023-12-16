using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Exceptions
{
    public class ProductIdException : Exception
    {
        public ProductIdException(string message) : base(message) { }
    }
}
