using Proiect_PSSC.Domain.Commands;
using Proiect_PSSC.Domain.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Proiect_PSSC.Domain.Models.Events.BillingEvent;
using static Proiect_PSSC.Domain.Models.Events.ShippingEvent;
using static Proiect_PSSC.Domain.Models.States.BillingState;
using static Proiect_PSSC.Domain.Models.States.ShippingState;

namespace Proiect_PSSC.Domain.Workflows
{
    public class ShippingWorkflow
    {
        public IShippingEvent Execute(ShippingCommand command)
        {
            UnshippedOrder unshippedOrder = new UnshippedOrder(command.ProductList, command.Total, command.ClientId);
            IShippingState products = ShippingOperation.SendOrderToShippingService(unshippedOrder);

            return products.Match(
                    whenUnshippedOrder: unshippedOrder => new ShippingSuccessEvent("The order has not been payed.", unshippedOrder.ClientId) as IShippingEvent,
                    whenShippedOrder: shippedOrder => new ShippingSuccessEvent(shippedOrder.SuccessMessage, shippedOrder.ClientId)
                );
        }
    }
}
