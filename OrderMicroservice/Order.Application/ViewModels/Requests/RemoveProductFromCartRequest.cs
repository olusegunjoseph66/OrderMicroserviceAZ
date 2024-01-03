using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Requests
{
    public class RemoveProductFromCartRequest
    {
        public int ShoppingCartItemId { get; set; }

    }
}
