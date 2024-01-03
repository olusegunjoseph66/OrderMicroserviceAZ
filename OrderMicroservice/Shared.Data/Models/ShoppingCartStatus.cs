using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class ShoppingCartStatus
    {
        public ShoppingCartStatus()
        {
            ShoppingCarts = new HashSet<ShoppingCart>();
        }

        public byte Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;

        public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; }
    }
}
