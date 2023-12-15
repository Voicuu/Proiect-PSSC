using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Exceptions
{
    internal class ExceptionProductQuantity : Exception
    {
        public ExceptionProductQuantity(string message) : base(message) { }
    }
}
