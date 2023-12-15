using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models
{
    public record Contact
    {
        public string phoneNr { get; private init; }
        public string email { get; private init; }
        public Contact(string phoneNr, string email)
        {
            this.phoneNr = phoneNr;
            this.email = email;
        }
    }
}
