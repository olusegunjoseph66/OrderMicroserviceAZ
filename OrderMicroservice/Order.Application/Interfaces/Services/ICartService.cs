using Order.Application.DTOs.APIDataFormatters;
using Order.Application.ViewModels.QueryFilters;
using Order.Application.ViewModels.Requests;
using Order.Application.ViewModels.Responses;
using Shared.Utilities.DTO.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Interfaces.Services
{
    public interface ICartService
    {
        Task<ApiResponse> AddProduct(AddProductToCartRequest request, CancellationToken cancellationToken);
        Task<ApiResponse> AddProductV2(AddProductToCartRequestV2 request, CancellationToken cancellationToken);
        Task<ApiResponse> UpdateProduct(UpdateProductItemToCartRequest request, int ShoppingCartItemId, CancellationToken cancellationToken);
        Task<ApiResponse> DeleteProduct(RemoveProductFromCartRequest request, int ShoppingCartItemId, CancellationToken cancellationToken);
       
        Task<ApiResponse> GetActiveCarts(CancellationToken cancellationToken);
        Task<ApiResponse> GetActiveCartsV2(CancellationToken cancellationToken);
        Task<ApiResponse> AutoAbandonCarts(CancellationToken cancellationToken);


    }
}
