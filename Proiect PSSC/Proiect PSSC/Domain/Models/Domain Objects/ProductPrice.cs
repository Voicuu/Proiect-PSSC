using LanguageExt;
using LanguageExt.ClassInstances.Pred;
using Proiect_PSSC.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Proiect_PSSC.Domain.Models.Domain_Objects
{
    public class ProductPrice
    {
        public decimal Value { get; set; }

        private static bool IsValid(decimal value)
        {
            return value > 0;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public ProductPrice(decimal value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new ProductPriceException($"The price {Value} is less than 0!");
            }
        }

        public static Option<ProductPrice> TryParse(string stringValue)
        {
            if (decimal.TryParse(stringValue, out decimal decimalValue))
            {
                if (IsValid(decimalValue))
                {
                    return Some<ProductPrice>(new(decimalValue));
                }
            }
            
            return None;
        }
    }
}
