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
    public class ProductQuantity
    {
        public int Value { get; set; }

        private static bool IsValid(int value)
        {
            return value >= 0 && value <= 10000;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public ProductQuantity(int value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new ProductQuantityException($"The quantity {Value} is not between 0 and 10000!");
            }
        }

        public static Option<ProductQuantity> TryParse(string stringValue)
        {
            if (int.TryParse(stringValue, out int integerValue))
            {
                if (IsValid(integerValue))
                {
                    return Some<ProductQuantity>(new(integerValue));
                }
            }
            
            return None;
        }
    }
}
