using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Filters;
using Order.Application.DTOs.Request;
using Order.Application.DTOs.Response;
using Order.Application.ViewModels.Requests;
using Order.Application.ViewModels.Responses;
using Shared.ExternalServices.ViewModels.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.Interfaces.Services
{
    public interface IOrderService
    {
        Task<ApiResponse> SubmitOrder(DmsRequestDTO model);
        Task<ApiResponse> SubmitOrderV2(DmsRequestDTOV2 model);
        Task<ApiResponse> UpdateOrder(DmsRequestDTO model);
        Task<ApiResponse> GetMyDmsOrder(DmsOrderQueryRequestDTO model, int UserId);
        Task<ApiResponse> GetMyDmsOrderBySapAccountId(DmsOrderBySapAccountIdQueryRequestDTO model);
        Task<ApiResponse> GetMyDmsOrderByOrderId(int orderId);
        Task<ApiResponse> GetMySAPOrderByDAccountIdAndOrderSapNo(string orderSapNo, int distrubutorAccountId);
        Task<ApiResponse> ValidateOtp(string code);
        Task<ApiResponse> CancelOrder(CancelDmsRequestDTO order);
        Task<ApiResponse> AdminGetDmsOrder(AdminDmsOrderQueryRequestDTO query);
        Task<ApiResponse> ExportDmsOrder(ExportDmsOrderFilterDto filter);
        Task<ApiResponse> OrderStatuses(CancellationToken cancellationToken);
        Task<ApiResponse> GetMySapChildOrder(DmsOrderSapChildByQueryRequestDTO query);
        Task<ApiResponse> GetMyDMSOrderChangeLog(int dmsOrderId, int UserId);
        Task<ApiResponse> GetMyDMSOrderChangeLog(string orderSapNumber, int UserId);
        Task<ApiResponse> ExportMySapOrder(int distributorSapAccountId, string orderSapNumber);
        Task<ApiResponse> ExportMySapOrder2(int distributorSapAccountId, string orderSapNumber);
       // Task<ApiResponse> ExportMySapOrder3(int distributorSapAccountId, string orderSapNumber);

        Task<ApiResponse> SaveOrder(SaveOrderLaterRequest request, CancellationToken cancellationToken);
        Task<ApiResponse> SaveOrderV2(SaveOrderLaterRequestV2 request, CancellationToken cancellationToken);
        Task<ApiResponse> StartCheck(StartCheckRequest request, CancellationToken cancellationToken);
        Task<ApiResponse> StartCheckV2(StartCheckRequest request, CancellationToken cancellationToken);
        Task<ApiResponse> GetOrderHistory(int OrderId, CancellationToken cancellationToken);
        Task<ApiResponse> GetOrderHistory(string OrderSapNumber, CancellationToken cancellationToken);
        Task<ApiResponse> GetMyRecentDmsOrder(CancellationToken cancellationToken);
        Task<ApiResponse> ViewDmsOrderDetails(int orderId, CancellationToken cancellationToken);
        Task<ApiResponse> ScheduleATC(ScheduleATCDeliveryRequest request, CancellationToken cancellationToken);

        Task<ApiResponse> AutoSubmitOrder(CancellationToken cancellationToken);
        Task<ApiResponse> AutoRefreshOrder(CancellationToken cancellationToken);
        Task<ApiResponse> SearchAtc(int distributorSapAccountId, string atcNumbers, CancellationToken cancellation);
        Task<ApiResponse> TrackMyOrder(int distributorSapAccountId, string orderSapNumber, CancellationToken cancellation);
        Task<ApiResponse> Dashboard(int distributorSapAccountId);
        Task<ApiResponse> Dashboard();
        Task<ApiResponse> RequestReportByProduct(RequestReportByProductViewModel vm);
        Task<ApiResponse> ProofOfDelivery(ProofRequestDto model);
        Task<ApiResponse> RequestOrderDocument(ProofRequestDto model);
    }
}
