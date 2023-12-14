using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models
{
    public record Client
    {
        public string clientID { get; private init; }
        public string name { get; private set; }
        public Address address { get; private set; }
        public Contact contact { get; private set; }

        public Client(string clientID, string name, Address address, Contact contact)
        {
            this.clientID = clientID;
            this.name = name;
            this.address = address;
            this.contact = contact;
        }
    }
}
