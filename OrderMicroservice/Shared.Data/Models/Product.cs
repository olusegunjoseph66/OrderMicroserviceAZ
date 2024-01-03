using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class Product
    {
        public Product()
        {
            DmsOrderItems = new HashSet<DmsOrderItem>();
            ShoppingCartItems = new HashSet<ShoppingCartItem>();
        }

        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string ProductSapNumber { get; set; } = null!;
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public string UnitOfMeasureCode { get; set; } = null!;
        public DateTime DateRefreshed { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<DmsOrderItem> DmsOrderItems { get; set; }
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}
