using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class DeliveryStatus
    {
        public DeliveryStatus()
        {
            DmsOrders = new HashSet<DmsOrder>();
        }

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        public virtual ICollection<DmsOrder> DmsOrders { get; set; }
    }
}
