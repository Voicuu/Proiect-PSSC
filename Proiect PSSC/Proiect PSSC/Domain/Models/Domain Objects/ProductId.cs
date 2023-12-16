using LanguageExt;
using Proiect_PSSC.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Proiect_PSSC.Domain.Models.Domain_Objects
{
    public class ProductId
    {
        public string Value { get; set; }

        private static bool IsValid(string value)
        {
            Regex regex = new Regex(@"^P\d{3}$");
            return regex.IsMatch(value);
        }

        public override string ToString()
        {
            return Value;
        }

        public ProductId(string value)
        {
            if (IsValid(value))
            {
                Value = value;
            }
            else
            {
                throw new ProductIdException($"The id {Value} doesn't match the pattern!");
            }
        }

        public static Option<ProductId> TryParse(string value)
        {
            if (IsValid(value))
            {
                return Some<ProductId>(new(value));
            }
            return None;
        }
    }
}
