using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class OrderStatus
    {
        public OrderStatus()
        {
            DmsOrders = new HashSet<DmsOrder>();
        }

        public byte Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        public virtual ICollection<DmsOrder> DmsOrders { get; set; }
    }
}
