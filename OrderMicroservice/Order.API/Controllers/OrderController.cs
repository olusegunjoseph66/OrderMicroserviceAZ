using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.API.Extensions;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Request;
using Order.Application.DTOs.Response;
using Order.Application.Interfaces.Services;
using Order.Application.ViewModels.Requests;
using Order.Application.ViewModels.Responses;
using Shared.Appplition.DTOs;
using Shared.ExternalServices.ViewModels.Request;
using Shared.Utilities.DTO.Pagination;

namespace Order.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : BaseController
    {
        private readonly IOrderService _orderService;
        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet("dms")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<GetMyDMSOrderDto>))]
        public async Task<IActionResult> GetMyDMSOrders([FromQuery] DmsOrderQueryRequestDTO filter) =>
            Response(await _orderService.GetMyDmsOrder(filter, User.GetSessionDetails().UserID).ConfigureAwait(false));

        [HttpGet("sap")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginatedList<GetMySAPOrderDto>>))]
        public async Task<IActionResult> GetMySAPOrders([FromQuery] DmsOrderBySapAccountIdQueryRequestDTO filter) => Response(await _orderService.GetMyDmsOrderBySapAccountId(filter).ConfigureAwait(false));

        [HttpGet("dms/{orderId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ViewDmsOrderDetailsDto>))]
        public async Task<IActionResult> GetMyDMSOrdersDetails(int orderId) => Response(await _orderService.GetMyDmsOrderByOrderId(orderId).ConfigureAwait(false));

        [HttpGet("sap/{distributorSapAccountId}/sapNumber/{orderSapNumber}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ViewSapOrderDetailsDtoVM>))]
        public async Task<IActionResult> GetMySAPOrdersDetails(int distributorSapAccountId, string orderSapNumber) => Response(await _orderService
            .GetMySAPOrderByDAccountIdAndOrderSapNo(orderSapNumber, distributorSapAccountId).ConfigureAwait(false));

        [HttpPost("dms/order/submit")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<OtpResponse>))]
        public async Task<IActionResult> SubmitOrder(DmsRequestDTO model) => Response(await _orderService.SubmitOrder(model).ConfigureAwait(false));

        [HttpPost("dms/order/submit/v2")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<OtpResponse>))]
        public async Task<IActionResult> SubmitOrderV2(DmsRequestDTOV2 model) => Response(await _orderService.SubmitOrderV2(model).ConfigureAwait(false));

        [HttpPost("dms/cancel")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        public async Task<IActionResult> CancelOrder(CancelDmsRequestDTO dmsOrder) => Response(await _orderService.CancelOrder(dmsOrder).ConfigureAwait(false));

        [HttpPut("dms-update")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ViewDmsOrderDetailsDtoVM>))]
        public async Task<IActionResult> UpdateOrder(DmsRequestDTO model) => Response(await _orderService.UpdateOrder(model).ConfigureAwait(false));

        [HttpGet("sap/atc")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<PaginatedList<GetMySAPChildOrderDto>>))]
        public async Task<IActionResult> GetMySapChildOrder([FromQuery] DmsOrderSapChildByQueryRequestDTO filter) => Response(await _orderService.GetMySapChildOrder(filter).ConfigureAwait(false));

        [HttpPost("dms/save")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> SaveOrder([FromBody] SaveOrderLaterRequest request, CancellationToken cancellationToken)
        {
            return Response(await _orderService.SaveOrder(request, cancellationToken));
        }
        [HttpPost("dms/save/v2")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<object>))]
        public async Task<IActionResult> SaveOrderV2([FromBody] SaveOrderLaterRequestV2 request, CancellationToken cancellationToken)
        {
            return Response(await _orderService.SaveOrderV2(request, cancellationToken));
        }
        [HttpPost("cart/checkout")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<StartCheckResponce>))]
        public async Task<IActionResult> StartCheckout([FromBody] StartCheckRequest request, CancellationToken cancellationToken)
        {
            return Response(await _orderService.StartCheck(request, cancellationToken));
        }

        [HttpPost("cart/checkout/v2")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<StartCheckResponce>))]
        public async Task<IActionResult> StartCheckoutV2([FromBody] StartCheckRequest request, CancellationToken cancellationToken)
        {
            return Response(await _orderService.StartCheckV2(request, cancellationToken));
        }

        [HttpGet("dms/recent")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ViewDmsOrderDetailsDto>))]
        public async Task<IActionResult> GetMyRecentDMSOrder(CancellationToken cancellationToken = default) => Response(await _orderService.GetMyRecentDmsOrder(cancellationToken).ConfigureAwait(false));


        [HttpPost("dms/atc/schedule")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ScheduleATCDeliveryResponse>))]
        public async Task<IActionResult> ScheduleATCOrder([FromBody] ScheduleATCDeliveryRequest request, CancellationToken cancellationToken)
        {
            return Response(await _orderService.ScheduleATC(request, cancellationToken));
        }

        [HttpGet("orderstatus")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<OrderStatusResponse>))]
        public async Task<IActionResult> GetOrderStatuses(CancellationToken cancellationToken = default)
        {
            return Response(await _orderService.OrderStatuses(cancellationToken));
        }


        [HttpGet("dms/{dmsOrderId}/changeLog")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<dmsOrderChangeLogResponse>))]
        public async Task<IActionResult> GetMyDMSOrderChangeLog(int dmsOrderId, CancellationToken cancellationToken = default)
        {
            return Response(await _orderService.GetMyDMSOrderChangeLog(dmsOrderId, User.GetSessionDetails().UserID).ConfigureAwait(false));
        }


        [HttpGet("sap/{orderSapNumber}/changeLog")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<dmsOrderChangeLogResponse>))]
        public async Task<IActionResult> GetMyDMSOrderChangeLog(string orderSapNumber, CancellationToken cancellationToken = default)
        {
            return Response(await _orderService.GetMyDMSOrderChangeLog(orderSapNumber, User.GetSessionDetails().UserID).ConfigureAwait(false));
        }

        [HttpGet("sap/export/{distributorSapAccountId}/sapNumber/{orderSapNumber}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ExportDto>))]
        public async Task<IActionResult> ExportDMSOrders(int distributorSapAccountId, string orderSapNumber) => Response(await _orderService.ExportMySapOrder(distributorSapAccountId, orderSapNumber));


        [HttpGet("sap/export2/{distributorSapAccountId}/sapNumber/{orderSapNumber}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ExportDto>))]
        public async Task<IActionResult> ExportDMSOrders2(int distributorSapAccountId, string orderSapNumber) => Response(await _orderService.ExportMySapOrder2(distributorSapAccountId, orderSapNumber));


        //[HttpGet("sap/export3/{distributorSapAccountId}/sapNumber/{orderSapNumber}")]
        //[ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<ExportDto>))]
        //public async Task<IActionResult> ExportDMSOrders3(int distributorSapAccountId, string orderSapNumber) => Response(await _orderService.ExportMySapOrder3(distributorSapAccountId, orderSapNumber));


        [HttpGet("sap/atc/search")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<SAPSearchList>))]
        public async Task<IActionResult> SearchAtc(int distributorSapAccountId, string atcNumbers, CancellationToken cancellation)
        {
            return Response(await _orderService.SearchAtc(distributorSapAccountId, atcNumbers, cancellation));
        }
        [HttpGet("track/{distributorSapAccountId}/sapNumber/{orderSapNumber}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<OrderTrackerDTO>))]
        public async Task<IActionResult> Track(int distributorSapAccountId, string orderSapNumber, CancellationToken cancellation)
             => Response(await _orderService.TrackMyOrder(distributorSapAccountId, orderSapNumber, cancellation));

        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<DashboardReport2>))]
        public async Task<IActionResult> dashboard() => Response(await _orderService.Dashboard());

        [HttpGet("dashboard/distributorSapAccountId")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<DashboardReport>))]
        public async Task<IActionResult> dashboardV2(int distributorSapAccountId) => Response(await _orderService.Dashboard(distributorSapAccountId));


        [HttpGet("RequestReportByProduct")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<ReportByProductViewModel>>))]
        public async Task<IActionResult> RequestReportByProduct([FromQuery] RequestReportByProductViewModel model) => Response(await _orderService.RequestReportByProduct(model));

        [HttpPost("report/deliveryProof")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<EmptyResult>>))]
        public async Task<IActionResult> deliveryProof([FromBody] ProofRequestDto model) => Response(await _orderService.ProofOfDelivery(model));

        [HttpPost("report/atc")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse<List<EmptyResult>>))]
        public async Task<IActionResult> GetOrderDocument([FromBody] ProofRequestDto model) => Response(await _orderService.RequestOrderDocument(model));

    }
}
