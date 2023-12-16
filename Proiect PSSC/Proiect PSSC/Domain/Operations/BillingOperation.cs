using Proiect_PSSC.Domain.Models.Domain_Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Proiect_PSSC.Domain.Models.States.BillingState;

namespace Proiect_PSSC.Domain.Operations
{
    public static class BillingOperation
    {
        private static decimal CalculateTotal(List<AvailableProduct> availableProducts)
        {
            decimal total = 0;
            foreach(var availableProduct in availableProducts) 
            {
                total += availableProduct.TotalProductPrice.Value;
            }
            return total;
        }
        public static IBillingState ChoosePaymentMethod(UnpayedOrder unpayedOrder, string paymentMethod)
        {
            if (paymentMethod == "card")
            {
                return new PayByCardOrder(unpayedOrder.ProductList, CalculateTotal(unpayedOrder.ProductList.ToList()), unpayedOrder.ClientId);
            }
            return new PayByCashOrder(unpayedOrder.ProductList, CalculateTotal(unpayedOrder.ProductList.ToList()), unpayedOrder.ClientId);
        }
    }
}
