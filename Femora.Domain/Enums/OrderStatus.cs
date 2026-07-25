using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Domain.Enums;
public enum OrderStatus
{
    [Description("Pending Payment")]
    Pending = 0,

    [Description("Payment Received")]
    Processing = 1,

    [Description("Order Shipped")]
    Shipped = 2,

    [Description("Delivered Successfully")]
    Delivered = 3,

    [Description("Order Cancelled")]
    Cancelled = 4,
}
