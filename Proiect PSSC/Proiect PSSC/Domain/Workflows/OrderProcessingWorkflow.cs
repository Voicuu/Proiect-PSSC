using Firebase.Database;
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
        public async Task<IOrderProcessingEvent> ExecuteAsync(OrderProcessingCommand command, 
                                                              Func<ProductId, FirebaseClient, Task<TryAsync<bool>>> checkProductExists,
                                                              Func<ProductId, FirebaseClient, Task<Option<ValidatedProduct>>> getProductById)
        {
            UnvalidatedOrder unvalidatedOrder = new UnvalidatedOrder(command.ProductList, command.ClientId);
            IOrderState products = await OrderProcessingOperation.ValidateProducts(checkProductExists, unvalidatedOrder, command.FirebaseClient);
            products = await OrderProcessingOperation.CheckAvailability(products, getProductById, command.FirebaseClient);

            return products.Match(
                    whenUnvalidatedOrder: unvalidatedOrder => new OrderProcessingFailedEvent("Unexpected unvalidated state") as IOrderProcessingEvent,
                    whenInvalidatedOrder: invalidOrder => new OrderProcessingFailedEvent(invalidOrder.Reason),
                    whenValidatedOrder: validatedOrder => new OrderProcessingFailedEvent("Unavailable products"),
                    whenAvailableOrder: availableOrder => new OrderProcessingSuccessEvent(availableOrder.ProductList, availableOrder.ClientId)
                );
        }
    }
}
