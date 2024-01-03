using DMS_API;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Order.Application.DTOs;
using Order.Application.ViewModels.Requests;
using Shared.Data.Models;
using Shared.Data.Repository;
using Shared.ExternalServices.DTOs;
using Shared.ExternalServices.DTOs.APIModels;
using Shared.ExternalServices.Interfaces;
using Shared.ExternalServices.ViewModels.Request;
using Shared.ExternalServices.ViewModels.Response;
using System.Text;
using Vm.ViewModels.Responses;

namespace Shared.ExternalServices.APIServices
{
    public class SapService : BaseService, ISapService
    {
        private readonly IMemoryCache _cache;
        private IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IAsyncRepository<Data.Models.Product> _product;
        private readonly ILogger<SapService> _sapLogger;

        public SapService(IMemoryCache cache, IHttpClientFactory httpClientFactory, ILogger<SapService> _sapLogger, IConfiguration _config, IAsyncRepository<Data.Models.Product> _product) : base(httpClientFactory)
        {
            _cache = cache;
            _httpClientFactory = httpClientFactory;
            this._config = _config;
            this._product = _product;
            this._sapLogger = _sapLogger;
        }
        public async Task<CustomerDto> FindCustomer(string companyCode, string countryCode, string distributorNumber)
        {
            var result = await this.SendAsync<ResponseDto>(new ApiRequest()
            {
                apiType = SD.ApiType.GET,
                url = $"{SD.SapAPIBase}/customer/{countryCode}/{companyCode}/{distributorNumber}",
            });
            if (result != null && result?.Status == "success")
                return JsonConvert.DeserializeObject<CustomerDto>(Convert.ToString(result.Data));
            else
            {
                return new CustomerDto()
                {
                    DistributorName = "Niyi Tayo",
                    EmailAddress = "niyitayo@gmail.com",
                    PhoneNumber = "07031863168",
                };
            }
        }
        public async Task<List<SapOrderDto>> GetOrder(string companyCode, string countryCode, string distributorNumber, DateTime? fromDate, DateTime? toDate)
        {
            if (toDate == null)
                toDate = DateTime.UtcNow;
            var startDate = fromDate.Value.ToString("yyyy-MM-dd");
            var endDate = toDate.Value.ToString("yyyy-MM-dd");
            //var request = new HttpRequestMessage(HttpMethod.Get, $"http://sappidev2.dangote-group.com:50000/RESTAdapter/DMS/order/{countryCode}/{companyCode}/{distributorNumber}/from/{startDate}/to/{endDate}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.SapAPIBase}/order/{countryCode}/{companyCode}/{distributorNumber}/from/{startDate}/to/{endDate}");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            HttpClient client = new HttpClient(handler);
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            client.DefaultRequestHeaders.Clear();
            client.Timeout = TimeSpan.FromMinutes(20);
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            var apiResponseDto = JsonConvert.DeserializeObject<SapOrdersReponseVm>(apiContent);
            _sapLogger.LogInformation($" Distributor Order list retrieved for Distributor's Number : {distributorNumber} | {DateTime.UtcNow} | {apiContent} ");
            if (apiResponseDto.statusCode == "00" && apiResponseDto.data != null)
            {
                var res = new List<SapOrderDto>();
                foreach (var item in apiResponseDto.data.sapOrders)
                {
                    res.Add(new SapOrderDto
                    {
                        DateCreated = DateTime.Parse(item.dateCreated),
                        DistributorNumber = item.distributorNumber,
                        Id = item.Id,
                        NumberOfItems = item.numberOfItems.ToString(),
                        OrderType = new DTOs.APIModels.Status { Code = item.orderType.code, Name = item.orderType.name },
                        NetValue = item.netValue.ToString("F2"),
                        ParentId = item.parentId,
                        Status = new DTOs.APIModels.Status { Name = item.Status.name, Code = item.Status.name },
                        DeliveryBlock = null,
                    });
                }
                return res;
            }
            _sapLogger.LogError($"An error Occurred while retrieving SAP Orders for Distributor : {distributorNumber} | {DateTime.UtcNow}", apiContent);
            return new List<SapOrderDto>();
        }
        public async Task<SapOrder1> GetOrderDetails(string companyCode, string countryCode, string orderSapNo)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.SapAPIBase}/order/{countryCode}/{companyCode}/{orderSapNo}");
            //var request = new HttpRequestMessage(HttpMethod.Get, $"http://sappidev2.dangote-group.com:50000/RESTAdapter/DMS/order/{countryCode}/{companyCode}/{orderSapNo}");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            HttpClient client = new HttpClient(handler);
            //request.Headers.Authorization = new BasicAuthenticationHeaderValue("DMS_D", "z95W!j5V39gQ");
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            var apiResponseDto = JsonConvert.DeserializeObject<APIResponseDto>(apiContent);
            _sapLogger.LogInformation($" Order details fetched for Order Number : {orderSapNo} | {DateTime.UtcNow} | {apiContent}");
            if (apiResponseDto.statusCode == "00")
            {
                var returnResponse = JsonConvert.DeserializeObject<Root>(apiContent);
                return returnResponse.data.sapOrder;

                //return apiResponseDto.data.sapOrder;
            }
            _sapLogger.LogError($"error occured while retrieving order details for Id : {orderSapNo} | {DateTime.UtcNow}", apiContent);
            return null;

        }
        public async Task<WalletResponse> GetWallet(string companyCode, string countryCode, string distributorNumber)
        {
            //var result = await this.SendAsync<ResponseDto>(new ApiRequest()
            //{
            //    apiType = SD.ApiType.GET,
            //    url = $"{SD.SapAPIBase}/wallet/{countryCode}/{companyCode}/{distributorNumber}",
            //});
            //if (result != null && result?.Status == "success")
            //    return JsonConvert.DeserializeObject<WalletResponse>(Convert.ToString(result.Data));
            //else
            //{
            //    return new WalletResponse()
            //    {
            //        Id = 10,
            //        AvailableBalance = 100000000000000000
            //    };
            //}

            //var request = new HttpRequestMessage(HttpMethod.Get, $"http://sappidev2.dangote-group.com:50000/RESTAdapter/DMS/wallet/{countryCode}/{companyCode}/{distributorNumber}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.SapAPIBase}/wallet/{countryCode}/{companyCode}/{distributorNumber}");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            HttpClient client = new HttpClient(handler);
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            var apiResponseDto = JsonConvert.DeserializeObject<SAPWalletResponse1>(apiContent);
            _sapLogger.LogInformation($" Wallet information request for distributor: {distributorNumber} | {DateTime.UtcNow} |Payload: {countryCode}-{companyCode}-{distributorNumber} |Response: {apiContent}");
            if (apiResponseDto.statusCode == "00")
            {
                //if (apiResponseDto.data.sapWallet.AvailableBalance < 0)
                //{
                //    return new WalletResponse { AvailableBalance = 100000000000000000 };
                //}
                return new WalletResponse { AvailableBalance = apiResponseDto.data.sapWallet.AvailableBalance };
            }
            _sapLogger.LogError($"Error Occured while retrieving wallet for distributor : {distributorNumber} | {DateTime.UtcNow}", apiContent);
            return new WalletResponse { AvailableBalance = 0 };

        }
        public async Task<SapOrderCreateResponse> CreateOrder(DmsOrder sapDmsOrder)
        {

            try
            {
                var product = await _product.Table.FirstOrDefaultAsync(x => x.Id == sapDmsOrder.DmsOrderItems.FirstOrDefault().ProductId);
                var data = new CreateDmsOrderDto()
                {
                    deliveryMethodCode = sapDmsOrder.DeliveryMethodCode,
                    companyCode = sapDmsOrder.DistributorSapAccount.CompanyCode,
                    customerPaymentDate = sapDmsOrder.CustomerPaymentDate.Value.ToString("yyyy-MM-dd"),
                    distributorNumber = sapDmsOrder.DistributorSapAccount.DistributorSapNumber,
                    plantCode = sapDmsOrder.PlantCode,
                    customerPaymentReference = sapDmsOrder.CustomerPaymentReference,
                    orderItem = sapDmsOrder.DmsOrderItems.Select(c => new OrderitemDto()
                    {
                        productId = product.ProductSapNumber.ToString(),
                        quantity = c.Quantity.ToString(),
                        unitOfMeasureCode = c.SalesUnitOfMeasureCode
                    }).FirstOrDefault(),
                    deliveryAddress = sapDmsOrder.DeliveryAddress,
                    deliveryDate = sapDmsOrder.DeliveryDate?.ToString("yyyy-MM-dd"),
                    truckSizeCode = sapDmsOrder.TruckSizeCode,
                    deliveryStateCode = sapDmsOrder.DeliveryStateCode,
                };

                var request = new HttpRequestMessage(HttpMethod.Post, $"{SD.SapAPIBase}/order");
                //var request = new HttpRequestMessage(HttpMethod.Post, $"http://sappidev2.dangote-group.com:50000/RESTAdapter/DMS/order");
                var handler = new TemporaryRedirectHandler()
                {
                    InnerHandler = new HttpClientHandler()
                    {
                        AllowAutoRedirect = false
                    }
                };
                request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
                request.Content = new StringContent(JsonConvert.SerializeObject(data),
                            Encoding.UTF8, "application/json");
                HttpClient client = new HttpClient(handler);
                client.DefaultRequestHeaders.Clear();
                var response = await client.SendAsync(request);
                var apiContent = await response.Content.ReadAsStringAsync();
                var apiResponseDto = JsonConvert.DeserializeObject<SAPResponseDtoForAll>(apiContent);
                _sapLogger.LogInformation($"Creating an order for distributor : {sapDmsOrder.DistributorSapAccount.DistributorSapNumber} and Order Id : {sapDmsOrder.Id} | {DateTime.UtcNow}| {apiContent}");
                if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    return new SapOrderCreateResponse
                    {
                        ActionFailed = true,
                    };
                }

                if (apiResponseDto.statusCode == "00")
                {
                    var res = JsonConvert.DeserializeObject<CreateSapOrderResponseObj>(Convert.ToString(apiContent));
                    return new SapOrderCreateResponse
                    {
                        SapOrder = res.data.sapOrder,
                        ActionFailed = false

                    };

                }
                _sapLogger.LogError($"Error occured while creating SAP Order for distributor : {sapDmsOrder.DistributorSapAccount.DistributorSapNumber} and Order Id : {sapDmsOrder.Id} | {DateTime.UtcNow}", apiContent);
                return null;
            }
            catch (Exception)
            {

                return null;
            }

        }
        public async Task<List<SapOrderDto>> GetChildOrder(string companyCode, string countryCode, string orderId)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.SapAPIBase}/order/children/{countryCode}/{companyCode}/{orderId}");
           // var request = new HttpRequestMessage(HttpMethod.Get, $"http://sappidev2.dangote-group.com:50000/RESTAdapter/DMS/order/children/{countryCode}/{companyCode}/{orderId}");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            HttpClient client = new HttpClient(handler);
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            var apiResponseDto = JsonConvert.DeserializeObject<SapChildrenResponseDTO>(apiContent);
            _sapLogger.LogInformation($"Fetch child Order initiated for Order with Id: {orderId} | {DateTime.UtcNow} | {apiContent}");

            if (apiResponseDto == null)
            {
                return new List<SapOrderDto>();
            }
            if (apiResponseDto.StatusCode == "00" && apiResponseDto.Data.SapOrders != null)
                return apiResponseDto.Data.SapOrders;
            else
            {
                _sapLogger.LogError($"error occured while retrieving ATC for Order : {orderId} | {DateTime.UtcNow}", apiContent);
                return new List<SapOrderDto>();
            }
        }
        public async Task<List<PlantResponse>> GetPlant(string companyCode, string countryCode)
        {

            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.SapAPIBase}/plant/{countryCode}/{companyCode}");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            HttpClient client = new HttpClient(handler);
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            client.DefaultRequestHeaders.Clear();
            client.Timeout = TimeSpan.FromMinutes(3);
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            var apiResponseDto = JsonConvert.DeserializeObject<SapPlantResponse>(apiContent);
            _sapLogger.LogInformation($"Fetching plants for company code {companyCode} and countryCode {countryCode} | {DateTime.UtcNow} | {apiContent}");
            if (apiResponseDto.data != null)
            {
                var res = new List<PlantResponse>();
                foreach (var plant in apiResponseDto.data.sapPlants)
                {
                    res.Add(new PlantResponse
                    {
                        DateRefreshed = DateTime.UtcNow,
                        Address = plant.Address,
                        CompanyCode = companyCode,
                        CountryCode = countryCode,
                        Name = plant.Name,
                        Code = plant.Id
                    });
                }
                return res;
            }
            _sapLogger.LogError($"Error occured while retrieving plants | {DateTime.UtcNow}", apiContent);
            return new List<PlantResponse>();

            //var result = await this.SendAsync<ResponseDto>(new ApiRequest()
            //{
            //    apiType = SD.ApiType.GET,
            //    url = $"{SD.SapAPIBase}/plant/{countryCode}/{companyCode}",
            //});
            //if (result != null && result?.Status == "success")
            //    return JsonConvert.DeserializeObject<List<PlantResponse>>(Convert.ToString(result.Data));
            //else
            //{
            //    return new List<PlantResponse>().Select(c => new PlantResponse()
            //    {

            //        CompanyCode = "CC00103",
            //        CountryCode = "NG",
            //        Code = "CC001",
            //        Address = "29 Fake Address",
            //        Name = "Alapere Depot",
            //        PlantTypeCode = "234Ui0"
            //    }).ToList();

            //}
        }
        public async Task<List<CountryResponse>> GetCountries()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.RDATAAPIBase}/api/v1/country");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            var apiResponseDto = JsonConvert.DeserializeObject<CountryResponseDto>(apiContent);
            var countries = new List<CountryResponse>();
            if (apiResponseDto.data != null)
            {
                foreach (var item in apiResponseDto.data.data.countries)
                {
                    countries.Add(new CountryResponse
                    {
                        code = item.code,
                        name = item.name,
                    });
                }
                return countries;
            }
            return new List<CountryResponse>();
        }
        public async Task<List<CountryResponse>> GetState(string countrycode)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.RDATAAPIBase}/api/v1/country/{countrycode}/state");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            var apiResponseDto = JsonConvert.DeserializeObject<ApiResponseState>(apiContent);
            if (apiResponseDto.data != null)
            {
                return apiResponseDto.data.state;
            }
            return null;
        }
        public async Task<List<CompanyResponse>> GetCompanies()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.RDATAAPIBase}/api/v1/company");
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            var apiResponseDto = JsonConvert.DeserializeObject<RDataCompanyResponse>(apiContent);

            //var res = JsonConvert.DeserializeObject<RDataCompanyResponse>(Convert.ToString(apiResponseDto.data.companies));
            if (apiResponseDto != null)
            {
                var resp = new List<CompanyResponse>();
                foreach (var company in apiResponseDto.data.companies)
                {
                    resp.Add(new CompanyResponse
                    {
                        Code = company.code,
                        Name = company.name,

                    });
                }
                return resp;
            }
            return new List<CompanyResponse>();
        }
        public async Task<bool> CreateSAPDelivary(DmsOrder sapDmsOrder)
        {
            var data = new CreateDmsOrderDelivery()
            {
                OrderId = sapDmsOrder.OrderSapNumber,
                companyCode = sapDmsOrder.CompanyCode,
                deliveryAddress = sapDmsOrder.DeliveryAddress,
                deliveryCity = sapDmsOrder.DeliveryCity,
                plantCode = sapDmsOrder.PlantCode,
                deliveryDate = sapDmsOrder.DeliveryDate.Value.ToString("yyyyMMdd"),
            };
            //var request = new HttpRequestMessage(HttpMethod.Post, $"http://sappidev2.dangote-group.com:50000/RESTAdapter/DMS/order/update/{sapDmsOrder.OrderSapNumber}");
            var request = new HttpRequestMessage(HttpMethod.Post, $"{SD.SapAPIBase}/order/update/{sapDmsOrder.OrderSapNumber}");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            request.Content = new StringContent(JsonConvert.SerializeObject(data),
                        Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            _sapLogger.LogInformation($"Create order initiated for Order : {sapDmsOrder.OrderSapNumber ?? ""} | {DateTime.UtcNow} | {apiContent}");
            var apiResponseDto = JsonConvert.DeserializeObject<APIResponseDto>(apiContent);
            if (apiResponseDto.statusCode == "00")
            {
                return true;
            }
            _sapLogger.LogError($"Error occured while retrieving plants | {DateTime.UtcNow}", apiContent);
            return false;
        }
        public async Task<SapTrip> GetTrips(string companyCode, string countryCode, string deliveryId)
        {
            
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.SapAPIBase}/trip/{countryCode}/{companyCode}/{deliveryId}");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            HttpClient client = new HttpClient(handler);
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            _sapLogger.LogInformation($"Get trips for Delivery: {deliveryId} | {DateTime.UtcNow} | {apiContent}");
            var res = JsonConvert.DeserializeObject<SapTripResponse>(apiContent);
            if (res.data != null)
            {
                return new SapTrip()
                {
                    OdometerEnd = res.data.sapTrips[0].OdometerEnd,
                    DateCreated = res.data.sapTrips[0].DateCreated,
                    DeliveryId = res.data.sapTrips[0].DeliveryId.ToString(),
                    DispatchDate = res.data.sapTrips[0].DispatchDate,
                    OdometerStart = res.data.sapTrips[0].OdometerStart,
                    Id = res.data.sapTrips[0].Id.ToString(),
                    TruckLocation = res.data.sapTrips[0].TruckLocation,
                    TripStatus = new DTOs.APIModels.Status { Code = res.data.sapTrips[0].tripStatus.code, Name = res.data.sapTrips[0].tripStatus.name }
                };

            }
            _sapLogger.LogError($"Error occured while retrieving trip details for deliveryId : {deliveryId} | {DateTime.UtcNow}", apiContent);

            return null;
        }
        public List<DmsOrder> GetSapOrders()
        {
            var productKey = "SapOrder";

            if (_cache.TryGetValue(productKey, out List<DmsOrder> cacheProducts))
                return cacheProducts;

            return new List<DmsOrder>();
        }
        public async Task<SapSearchChildOrderDto1> SearchChildOrder(string companyCode, string countryCode, string AtcNumber)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.SapAPIBase}/order/search/{countryCode}/{companyCode}/{AtcNumber}");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            HttpClient client = new HttpClient(handler);
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            _sapLogger.LogInformation($"Search ATC for ATC Number: {AtcNumber} | {DateTime.UtcNow}", apiContent);
            var apiResponseDto = JsonConvert.DeserializeObject<SapSearchChildOrderDto>(apiContent);
            if (apiResponseDto.Data != null && apiResponseDto.StatusCode == "00")
            {
                return apiResponseDto.Data.SapOrders;
            }
            _sapLogger.LogError($"Error occured while Search ATCs for ATC Number: {AtcNumber} | {DateTime.UtcNow} | {apiContent}");

            return null;
        }
        public async Task<bool> RequestReportByProduct(RequestReportByProductVm requestDto)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{SD.SapAPIBase}/statement");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            request.Content = new StringContent(JsonConvert.SerializeObject(requestDto));
            HttpClient client = new HttpClient(handler);
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            _sapLogger.LogInformation($"Report by Product request for distributor: {requestDto.distributorNumber} | {DateTime.UtcNow}| {apiContent}");
            //var apiResponseDto = JsonConvert.DeserializeObject<ExpandoObject>(apiContent);
            if (response.IsSuccessStatusCode)
                return true;
            _sapLogger.LogError($"Error occured while requesting report by products| {DateTime.UtcNow}", apiContent);
            return false;

        }
        public async Task<SapEstimateResponse> GetItemEstimate(EstimateRequest requestDto)
        {
            //var request = new HttpRequestMessage(HttpMethod.Get, $"http://sappidev2.dangote-group.com:50000/RESTAdapter/DMS/order/estimate/{requestDto.countryCode}/{requestDto.companyCode}/{requestDto.distributorNumber}");
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.SapAPIBase}/order/estimate/{requestDto.countryCode}/{requestDto.companyCode}/{requestDto.distributorNumber}");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            //request.Headers.Authorization = new BasicAuthenticationHeaderValue("DMS_D", "z95W!j5V39gQ");
            request.Content = new StringContent(JsonConvert.SerializeObject(requestDto), Encoding.UTF8, "application/json");
            _sapLogger.LogInformation($"Get Estimate Request | {DateTime.Now} | {JsonConvert.SerializeObject(request)} ");
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            var apiResponseDto = JsonConvert.DeserializeObject<APIResponseDto>(apiContent);
            _sapLogger.LogInformation($"Get Estimate for product with ID: {requestDto.productId} | {DateTime.UtcNow} | payload: {JsonConvert.SerializeObject(requestDto)} | response: {apiContent}");
            if (apiResponseDto.statusCode == "00")
            {
                var result = JsonConvert.DeserializeObject<SapEstimateResponse>(apiContent);
                return result;
            }
            _sapLogger.LogError($"Error occured while Getting estimate for product : {requestDto.productId} and plant: {requestDto.plantCode} | {DateTime.UtcNow}", apiContent);
            var result1 = JsonConvert.DeserializeObject<SapEstimateResponse>(apiContent);
            return result1;
        }
        public async Task<APIResponseDto> ProofOfDelivery(ProofOfDelivery proofOfDelivery)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{SD.SapAPIBase}/deliveryProof");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            request.Content = new StringContent(JsonConvert.SerializeObject(proofOfDelivery),
                        Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            _sapLogger.LogInformation($"Proof of Delivery for ATC: {proofOfDelivery.atcNumber} | {DateTime.UtcNow}| {apiContent}");
            var apiResponseDto = JsonConvert.DeserializeObject<APIResponseDto>(apiContent);
            return apiResponseDto;
        }
        public async Task<APIResponseDto> RequestOrderDocument(OrderDocumentVm model)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{SD.SapAPIBase}/order/statement");
            //var request = new HttpRequestMessage(HttpMethod.Post, $"http://sappidev2.dangote-group.com:50000/RESTAdapter/DMS/order/statement");
            var handler = new TemporaryRedirectHandler()
            {
                InnerHandler = new HttpClientHandler()
                {
                    AllowAutoRedirect = false
                }
            };
            request.Headers.Authorization = new BasicAuthenticationHeaderValue(_config.GetValue<string>("SapSetting:Username"), _config.GetValue<string>("SapSetting:Password"));
            request.Content = new StringContent(JsonConvert.SerializeObject(model),
                        Encoding.UTF8, "application/json");
            HttpClient client = new HttpClient(handler);
            client.DefaultRequestHeaders.Clear();
            var response = await client.SendAsync(request);
            var apiContent = await response.Content.ReadAsStringAsync();
            _sapLogger.LogInformation($"Request Order document request for ATC: {model.atcNumber} | {DateTime.UtcNow}| {apiContent}");
            var apiResponseDto = JsonConvert.DeserializeObject<APIResponseDto>(apiContent);
            return apiResponseDto;
        }
    }
}
