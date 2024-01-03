using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Requests
{
    public class UpdateProductItemToCartRequest
    {
        public int ShoppingCartItemId { get; set; }
        public double Quantity { get; set; }
    }
}
