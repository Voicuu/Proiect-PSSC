using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Proiect_PSSC.Domain.Models.States.ShippingState;

namespace Proiect_PSSC.Domain.Operations
{
    public class ShippingOperation
    {
        public static IShippingState SendOrderToShippingService(UnshippedOrder unshippedOrder)
        {
            return new ShippedOrder("The order has been successfully sent.\nThe package will arrive in approximately 2-3 days.\nCourier: Sameday", unshippedOrder.ClientId);
        }
    }
}
