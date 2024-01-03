using Order.Application.ViewModels.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Features
{
    public class ShoppingCartCacheDto
    {
        public ShoppingCartCacheDto(ActiveCartResponse activeCart, int userId)
        {
            _activeCart = activeCart;
            UserId = userId;
        }

        public ActiveCartResponse _activeCart { get; set; }
        public int UserId { get; set; }
    }

    public class ShoppingCartCacheDto2
    {
        public ShoppingCartCacheDto2(ActiveCartResponse2 activeCart, int userId)
        {
            _activeCart = activeCart;
            UserId = userId;
        }

        public ActiveCartResponse2 _activeCart { get; set; }
        public int UserId { get; set; }
    }
}
