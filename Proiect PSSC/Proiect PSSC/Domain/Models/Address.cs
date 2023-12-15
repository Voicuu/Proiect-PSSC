using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models
{
    public record Address
    {
        public string street { get; private init; }
        public string town { get; private init; }
        public Address(string street, string town)
        {
            this.street = street;
            this.town = town;
        }
    }
}
