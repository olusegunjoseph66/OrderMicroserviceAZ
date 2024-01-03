using Order.Application.ViewModels.Responses;
using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class OrdersShoppingCartAbandonedMessage: IntegrationBaseMessage
    {

        public int UserId { get; set; }
        public int ShoppingCartId { get; set; }
        public int ModifiedByUserId { get; set; }
        public string ShoppingCartStatus { get; set; }
        public DateTime? DateModified { get; set; }
        public List<ShoppingCartItemsResponse> ShoppingCartItems { get; set; }

       
    }
}
