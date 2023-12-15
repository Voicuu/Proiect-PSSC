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
    public static partial class ShippingState
    {
        public interface IShippingState { }

        public record UnshippedOrder(IReadOnlyCollection<AvailableProduct> productList, decimal total) : IShippingState;
        public record ShippedOrder(string successMessage) : IShippingState;
    }
}