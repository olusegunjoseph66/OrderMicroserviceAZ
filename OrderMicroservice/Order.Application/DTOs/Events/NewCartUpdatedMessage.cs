using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class NewCartUpdatedMessage : IntegrationBaseMessage
    {
        public int UserId { get; set; }
        public int ShoppingCartId { get; set; }
        public int ModifiedByUserId { get; set; }
        public StatusDto OldShoppingCartStatus { get; set; }
        public DateTime? DateModified { get; set; }
        public StatusDto newShoppingCartStatus { get; set; }
        public List<ShoppingCartItem11> ShoppingCartItems { get; set; }
    }

    public class ShoppingCartItem11
    {
        public int ShoppingCartItemId { get; set; }
        public DateTime dateCreated { get; set; }
        public Product11 Product { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasureCode { get; set; }
    }

    public class Product11
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }
}
