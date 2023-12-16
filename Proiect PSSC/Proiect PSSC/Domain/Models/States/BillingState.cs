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
    public static partial class BillingState
    {
        public interface IBillingState { }

        public record UnpayedOrder(IReadOnlyCollection<AvailableProduct> ProductList, string ClientId) : IBillingState;

        public record PayByCardOrder(IReadOnlyCollection<AvailableProduct> ProductList, decimal Total, string ClientId) : IBillingState;
        
        public record PayByCashOrder(IReadOnlyCollection<AvailableProduct> ProductList, decimal Total, string ClientId) : IBillingState;
        
        public record PaymentFailedOrder(IReadOnlyCollection<AvailableProduct> ProductList, string ClientId) : IBillingState;
        
        public record PayedOrder(IReadOnlyCollection<AvailableProduct> ProductList, decimal Total, string ClientId) : IBillingState;
    }
}
