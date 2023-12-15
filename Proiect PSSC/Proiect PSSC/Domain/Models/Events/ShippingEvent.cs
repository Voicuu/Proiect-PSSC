using CSharp.Choices;
using Proiect_PSSC.Domain.Models.Validations;
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
            public string Reason { get; }

            public ShippingSuccessEvent(string reason)
            {
                Reason = reason;
            }
        }
    }
}
