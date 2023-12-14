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
    public static partial class Status
    {

        public interface IStatus { }
        public record Empty(IReadOnlyCollection<UnvalidatedCart> cartList) : IStatus;

        public record Unvalidated(IReadOnlyCollection<UnvalidatedCart> cartList, string reason) : IStatus;

        public record Validated(IReadOnlyCollection<ValidatedCart> cartList) : IStatus;

        public record Payed(IReadOnlyCollection<ValidatedCart> cartList, DateTime payDate) : IStatus;
    }
}
