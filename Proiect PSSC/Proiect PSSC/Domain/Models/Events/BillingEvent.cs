﻿using CSharp.Choices;
using Proiect_PSSC.Domain.Models.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models.Events
{
    [AsChoice]
    public static partial class BillingEvent
    {
        public interface IBillingEvent { }

        public record BillingFailedEvent : IBillingEvent
        {
            public string Reason { get; }

            public BillingFailedEvent(string reason)
            {
                Reason = reason;
            }
        }

        public record BillingSuccessEvent : IBillingEvent
        {
            IReadOnlyCollection<AvailableProduct> ProductList { get; }
            public decimal Total {  get; }

            public BillingSuccessEvent(IReadOnlyCollection<AvailableProduct> productList, decimal total)
            {
                ProductList = productList;
                Total = total;
            }
        }
    }
}