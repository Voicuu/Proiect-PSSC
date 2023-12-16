using CSharp.Choices;
using Proiect_PSSC.Domain.Models.Domain_Objects;
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

        public record UnvalidatedOrder(IReadOnlyCollection<UnvalidatedProduct> ProductList, string ClientId) : IOrderState;
        
        public record InvalidatedOrder(IReadOnlyCollection<UnvalidatedProduct> ProductList, string Reason, string ClientId) : IOrderState;
        
        public record ValidatedOrder(IReadOnlyCollection<ValidatedProduct> ProductList, string ClientId) : IOrderState;
        
        public record AvailableOrder(IReadOnlyCollection<AvailableProduct> ProductList, string ClientId) : IOrderState;
    }
}
