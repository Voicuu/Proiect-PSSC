using CSharp.Choices;
using Proiect_PSSC.Domain.Models.Domain_Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models.Events
{
    [AsChoice]
    public static partial class OrderProcessingEvent
    {
        public interface IOrderProcessingEvent { }

        public record OrderProcessingFailedEvent : IOrderProcessingEvent
        {
            public string Reason { get; }

            public OrderProcessingFailedEvent(string reason)
            {
                Reason = reason;
            }
        }

        public record OrderProcessingSuccessEvent : IOrderProcessingEvent
        {
            public IReadOnlyCollection<AvailableProduct> ProductList { get; }

            public OrderProcessingSuccessEvent(IReadOnlyCollection<AvailableProduct> productList)
            {
                ProductList = productList;
            }
        }
    }
}
