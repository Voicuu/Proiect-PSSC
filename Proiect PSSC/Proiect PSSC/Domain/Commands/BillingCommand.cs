using Proiect_PSSC.Domain.Models.Domain_Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Commands
{
    public record BillingCommand(IReadOnlyCollection<AvailableProduct> ProductList, string ClientId, string paymentMethod)
    {
    }
}
