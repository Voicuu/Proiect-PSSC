using CSharp.Choices;
using Proiect_PSSC.Domain.Models.Validations;
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

        public record UnpayedOrder(IReadOnlyCollection<AvailableProduct> productList) : IBillingState;
        public record PayByCardOrder(IReadOnlyCollection<AvailableProduct> productList, decimal total) : IBillingState;
        public record PayByCashOrder(IReadOnlyCollection<AvailableProduct> productList, decimal total) : IBillingState;
        public record PaymentFailedOrder(IReadOnlyCollection<AvailableProduct> productList) : IBillingState;
        public record PayedOrder(IReadOnlyCollection<AvailableProduct> productList, decimal total) : IBillingState;
    }
}
