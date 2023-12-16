using LanguageExt;
using Proiect_PSSC.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Proiect_PSSC.Domain.Models.Domain_Objects
{
    public class ProductName
    {
        public string Value { get; set; }

        private static bool IsValid(string value)
        {
            return value.Length <= 50;
        }

        public override string ToString()
        {
            return Value;
        }

        public ProductName(string value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new ProductNameLengthException($"The length of {Value} is more than 50!");
            }
        }

        public static Option<ProductName> TryParse(string value) 
        {
            if (IsValid(value))
            {
                return Some<ProductName>(new(value));
            }
            return None;
        }
    }
}
