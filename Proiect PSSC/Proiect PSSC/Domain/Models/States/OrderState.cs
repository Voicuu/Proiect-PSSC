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
    public static partial class OrderState
    {
        public interface IOrderState { }

        public record UnvalidatedOrder(IReadOnlyCollection<UnvalidatedProduct> productList) : IOrderState;
        public record InvalidatedOrder(IReadOnlyCollection<UnvalidatedProduct> productList, string reason) : IOrderState;
        public record ValidatedOrder(IReadOnlyCollection<ValidatedProduct> productList) : IOrderState;
        public record AvailableOrder(IReadOnlyCollection<AvailableProduct> productList) : IOrderState;
    }
}
