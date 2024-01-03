using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class ShoppingCart
    {
        public ShoppingCart()
        {
            DmsOrderGroups = new HashSet<DmsOrderGroup>();
            DmsOrders = new HashSet<DmsOrder>();
            ShoppingCartItems = new HashSet<ShoppingCartItem>();
        }

        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreatedByUserId { get; set; }
        public int UserId { get; set; }
        public byte ShoppingCartStatusId { get; set; }
        public DateTime? DateModified { get; set; }
        public int? ModifiedByUserId { get; set; }
        public string? PlantCode { get; set; }
        public int? DistributorSapAccountId { get; set; }
        public string? DeliveryMethod { get; set; }

        public virtual DistributorSapAccount? DistributorSapAccount { get; set; }
        public virtual ShoppingCartStatus ShoppingCartStatus { get; set; } = null!;
        public virtual ICollection<DmsOrderGroup> DmsOrderGroups { get; set; }
        public virtual ICollection<DmsOrder> DmsOrders { get; set; }
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}
