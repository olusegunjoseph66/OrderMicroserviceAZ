using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class DistributorSapAccount
    {
        public DistributorSapAccount()
        {
            DmsOrders = new HashSet<DmsOrder>();
            ShoppingCartItems = new HashSet<ShoppingCartItem>();
            ShoppingCarts = new HashSet<ShoppingCart>();
        }

        public int DistributorSapAccountId { get; set; }
        public int UserId { get; set; }
        public string DistributorSapNumber { get; set; } = null!;
        public string DistributorName { get; set; } = null!;
        public DateTime DateRefreshed { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string AccountType { get; set; } = null!;

        public virtual ICollection<DmsOrder> DmsOrders { get; set; }
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}
