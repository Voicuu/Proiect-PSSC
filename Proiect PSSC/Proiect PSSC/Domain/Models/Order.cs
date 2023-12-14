using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models
{
    public record Order
    {
        public string?  orderID { get; private init; }
        public Cart     cart { get; private set; }
        public Client   client { get; private set; }
        public double   total { get; private set; }

        public Order(Cart cart, Client client)
        {
            this.cart = cart;
            this.client = client;
        }
        public void calcTotal()
        {
            double total_ = 0;
            foreach (var item in cart.productList)
            {
                total_ += item.quantity * item.price;
            }
            this.total = total_;
        }
    }
}
