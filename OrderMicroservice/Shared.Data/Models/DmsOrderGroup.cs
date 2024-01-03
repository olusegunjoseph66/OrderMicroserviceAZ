using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class DmsOrderGroup
    {
        public DmsOrderGroup()
        {
            DmsOrders = new HashSet<DmsOrder>();
            Otps = new HashSet<Otp>();
        }

        public int Id { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? ShoppingCartId { get; set; }

        public virtual ShoppingCart? ShoppingCart { get; set; }
        public virtual ICollection<DmsOrder> DmsOrders { get; set; }
        public virtual ICollection<Otp> Otps { get; set; }
    }
}
