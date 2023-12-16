using CSharp.Choices;
using Proiect_PSSC.Domain.Models.Domain_Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models.States
{
    [AsChoice]
    public static partial class ShippingState
    {
        public interface IShippingState { }

        public record UnshippedOrder(IReadOnlyCollection<AvailableProduct> ProductList, decimal Total, string ClientId) : IShippingState;
        
        public record ShippedOrder(string SuccessMessage, string ClientId) : IShippingState;
    }
}
