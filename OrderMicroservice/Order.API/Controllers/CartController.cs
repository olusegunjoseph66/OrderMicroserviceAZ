using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.Interfaces.Services;
using Order.Application.ViewModels.QueryFilters;
using Order.Application.ViewModels.Requests;
using Order.Application.ViewModels.Responses;
using Shared.Utilities.DTO.Pagination;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartController : BaseController
    {
        private readonly ICartService  _cartService;
        public CartController(ICartService cartService)
        {
            _cartService = cartService;    
        }

        
        [HttpPut("cartItem/{ShoppingCartItemId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> UpdateCartItemQuantity(int ShoppingCartItemId, [FromBody] UpdateProductItemToCartRequest request, CancellationToken cancellationToken)
        {
            return Response(await _cartService.UpdateProduct(request, ShoppingCartItemId, cancellationToken));
        }

        [HttpPost("cartItem")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> AddProductToCart([FromBody] AddProductToCartRequest request, CancellationToken cancellationToken)
        {
            return Response(await _cartService.AddProduct(request, cancellationToken));
        }

        [HttpPost("cartItem/v2")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> AddProductToCartv2([FromBody] AddProductToCartRequestV2 request, CancellationToken cancellationToken)
        {
            return Response(await _cartService.AddProductV2(request, cancellationToken));
        }

        [HttpDelete("cartItem/{ShoppingCartItemId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> DeleteCartItem(int ShoppingCartItemId, [FromBody] RemoveProductFromCartRequest request, CancellationToken cancellationToken)
        {
            return Response(await _cartService.DeleteProduct(request, ShoppingCartItemId, cancellationToken));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<ActiveCartResponseVM>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyActiveShoppingCart( CancellationToken cancellationToken)
        {
            return Response(await _cartService.GetActiveCarts( cancellationToken));
        }

        [HttpGet("cart/v2/")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse<ActiveCartResponseVM>))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyActiveShoppingCartV2(CancellationToken cancellationToken)
        {
            return Response(await _cartService.GetActiveCartsV2(cancellationToken));
        }
    }
}
