﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Proiect_PSSC.Domain.Models.Domain_Objects
{
    public record UnvalidatedProduct(string ProductId,
                                     string ProductName,
                                     string ProductQuantity,
                                     string ProductPrice)
    {

    }
}
