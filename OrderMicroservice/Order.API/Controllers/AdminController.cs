using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Filters;
using Order.Application.DTOs.Request;
using Order.Application.DTOs.Response;
using Order.Application.Interfaces.Services;
using Shared.Utilities.DTO.Pagination;

namespace Order.API.Controllers
{
    [Authorize(Roles = "Super Administrator, Administrator")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : BaseController
    {
        private readonly IOrderService _orderService;
        public AdminController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("order/dms")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginatedList<GetAdminDMSOrderDto>>))]
        public async Task<IActionResult> GetMyDMSOrders([FromQuery] AdminDmsOrderQueryRequestDTO filter) => Response(await _orderService.AdminGetDmsOrder(filter).ConfigureAwait(false));

        [HttpGet("dms/{OrderId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ViewDmsOrderDetailsDto>))]
        public async Task<IActionResult> GetDMSOrder(int OrderId, CancellationToken cancellationToken = default) => Response(await _orderService.ViewDmsOrderDetails(OrderId, cancellationToken).ConfigureAwait(false));

        [HttpGet("dms/{DmsOrderId}/changeLog")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ViewDmsOrderHistoryVm>))]
        public async Task<IActionResult> GetDMSOrderChangeLog(int DmsOrderId, CancellationToken cancellationToken = default)
        {
            return Response(await _orderService.GetOrderHistory(DmsOrderId, cancellationToken));
        }

        [HttpGet("order/dms/export")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ExportDto>))]
        public async Task<IActionResult> ExportDMSOrders([FromQuery] ExportDmsOrderFilterDto filter) => Response(await _orderService.ExportDmsOrder(filter).ConfigureAwait(false));
    }
}
