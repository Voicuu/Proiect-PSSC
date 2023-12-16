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
    public static partial class ShippingEvent
    {
        public interface IShippingEvent { }

        public record ShippingSuccessEvent : IShippingEvent
        {
            public string SuccessMessage { get; }

            public string ClientId { get; }

            public ShippingSuccessEvent(string successMessage, string clientId)
            {
                SuccessMessage = successMessage;
                ClientId = clientId;
            }
        }
    }
}
