using LanguageExt;
using Proiect_PSSC.Domain.Commands;
using Proiect_PSSC.Domain.Models.Domain_Objects;
using Proiect_PSSC.Domain.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Proiect_PSSC.Domain.Models.Events.OrderProcessingEvent;
using static Proiect_PSSC.Domain.Models.OrderState;

namespace Proiect_PSSC.Domain.Workflows
{
    public class OrderProcessingWorkflow
    {
        public async Task<IOrderProcessingEvent> ExecuteAsync(OrderProcessingCommand command, Func<ProductId, TryAsync<bool>> checkProductExists)
        {
            UnvalidatedOrder unvalidatedOrder = new UnvalidatedOrder(command.ProductList, command.ClientId);
            IOrderState products = await OrderProcessingOperation.ValidateProducts(checkProductExists, unvalidatedOrder);

            return products.Match(
                    whenUnvalidatedOrder: unvalidatedOrder => new OrderProcessingFailedEvent("Unexpected unvalidated state") as IOrderProcessingEvent,
                    whenInvalidatedOrder: invalidOrder => new OrderProcessingFailedEvent(invalidOrder.Reason),
                    whenValidatedOrder: validatedOrder => new OrderProcessingFailedEvent("Unexpected validated state"),
                    whenAvailableOrder: availableOrder => new OrderProcessingSuccessEvent(availableOrder.ProductList)
                );
        }
    }
}
