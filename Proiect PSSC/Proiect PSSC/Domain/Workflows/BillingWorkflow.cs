using LanguageExt;
using Proiect_PSSC.Domain.Commands;
using Proiect_PSSC.Domain.Models.Domain_Objects;
using Proiect_PSSC.Domain.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Proiect_PSSC.Domain.Models.Events.BillingEvent;
using static Proiect_PSSC.Domain.Models.Events.OrderProcessingEvent;
using static Proiect_PSSC.Domain.Models.OrderState;
using static Proiect_PSSC.Domain.Models.States.BillingState;

namespace Proiect_PSSC.Domain.Workflows
{
    public class BillingWorkflow
    {
        public IBillingEvent Execute(BillingCommand command)
        {
            UnpayedOrder unpayedOrder = new UnpayedOrder(command.ProductList, command.ClientId);
            IBillingState products = BillingOperation.ChoosePaymentMethod(unpayedOrder, command.paymentMethod);
            products = BillingOperation.PayOrder(products);

            return products.Match(
                    whenUnpayedOrder: unpayedOrder => new BillingFailedEvent("The order has not been payed.") as IBillingEvent,
                    whenPayByCardOrder: payByCardOrder => new BillingFailedEvent("Waiting for payment..."),
                    whenPayByCashOrder: payByCashOrder => new BillingSuccessEvent(payByCashOrder.ProductList,payByCashOrder.Total),
                    whenPaymentFailedOrder: paymentFailedOrder => new BillingFailedEvent("You are broke."),
                    whenPayedOrder: payedOrder => new BillingSuccessEvent(payedOrder.ProductList, payedOrder.Total)
                );
        }
    }
}
