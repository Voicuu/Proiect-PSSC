using CSharp.Choices;
using Proiect_PSSC.Domain.Models.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models
{
    [AsChoice]
    public static partial class OrderStatus
    {
        public interface IOrderStatus { }

        public record Unvalidated(IReadOnlyCollection<UnvalidatedOrder> ordersList, string reason) : IOrderStatus;
        public record Unavailable(IReadOnlyCollection<UnvalidatedOrder> ordersList) : IOrderStatus;
        public record Failed(IReadOnlyCollection<UnvalidatedOrder> ordersList, Exception ex) : IOrderStatus;
        public record Validated(IReadOnlyCollection<ValidatedOrder> ordersList) : IOrderStatus;
        public record Issued(IReadOnlyCollection<ValidatedOrder> ordersList) : IOrderStatus;
        public record Cancelled(IReadOnlyCollection<ValidatedOrder> ordersList) : IOrderStatus;
    }
}
