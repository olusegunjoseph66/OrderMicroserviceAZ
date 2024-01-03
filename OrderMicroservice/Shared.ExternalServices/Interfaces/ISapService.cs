using Order.Application.ViewModels.Requests;
using Shared.Data.Models;
using Shared.ExternalServices.DTOs;
using Shared.ExternalServices.DTOs.APIModels;
using Shared.ExternalServices.ViewModels.Request;
using Shared.ExternalServices.ViewModels.Response;
using Vm.ViewModels.Responses;

namespace Shared.ExternalServices.Interfaces
{
    public interface ISapService
    {
        Task<SapOrderCreateResponse> CreateOrder(DmsOrder sapDmsOrder);
        Task<bool> CreateSAPDelivary(DmsOrder sapDmsOrder);
        Task<CustomerDto> FindCustomer(string companyCode, string countryCode, string distributorNumber);
        Task<List<SapOrderDto>> GetChildOrder(string companyCode, string countryCode, string orderId);
        Task<List<CompanyResponse>> GetCompanies();
        Task<List<CountryResponse>> GetCountries();
        Task<List<SapOrderDto>> GetOrder(string companyCode, string countryCode, string distributorNumber, DateTime? fromDate, DateTime? toDate);
        Task<SapOrder1> GetOrderDetails(string companyCode, string countryCode, string orderSapNo);
        Task<List<PlantResponse>> GetPlant(string companyCode, string countryCode);
        List<DmsOrder> GetSapOrders();
        //List<DmsOrder> GetChildATCOrders(int distributorId, bool isAtc, string parentSapNumber);
        Task<List<CountryResponse>> GetState(string countrycode);
        Task<SapTrip> GetTrips(string companyCode, string countryCode, string deliveryId);
        Task<WalletResponse> GetWallet(string companyCode, string countryCode, string distributorNumber);
        Task<SapSearchChildOrderDto1> SearchChildOrder(string companyCode, string countryCode, string AtcNumber);
        Task<bool> RequestReportByProduct(RequestReportByProductVm request);
        Task<SapEstimateResponse> GetItemEstimate(EstimateRequest request);
        Task<APIResponseDto> ProofOfDelivery(ProofOfDelivery proofOfDelivery);
        Task<APIResponseDto> RequestOrderDocument(OrderDocumentVm model);
    }
}