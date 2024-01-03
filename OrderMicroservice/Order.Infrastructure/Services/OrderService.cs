using Account.Application.Constants;
using Account.Application.Interfaces.Services;
using Aspose.Pdf;
using AutoMapper;
using DinkToPdf.Contracts;
using IronPdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Order.Application.Constants;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Events;
using Order.Application.DTOs.Filters;
using Order.Application.DTOs.Request;
using Order.Application.DTOs.Response;
using Order.Application.DTOs.Sortings;
using Order.Application.Enums;
using Order.Application.Exceptions;
using Order.Application.Interfaces.Services;
using Order.Application.ViewModels.Requests;
using Order.Application.ViewModels.Responses;
using Order.Infrastructure.QueryObjects;
using Shared.Appplition.DTOs;
using Shared.Data.Extensions;
using Shared.Data.Models;
using Shared.Data.Repository;
using Shared.ExternalServices.Interfaces;
using Shared.ExternalServices.ViewModels.Request;
using Shared.ExternalServices.ViewModels.Response;
using Shared.Utilities.DTO.Pagination;
using Shared.Utilities.Helpers;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Order.Infrastructure.Services
{
    public partial class OrderService : BaseService, IOrderService
    {
        public async Task<ApiResponse> GetMyDmsOrder(DmsOrderQueryRequestDTO model, int UserId)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order"}{" | "}{LoggedInUser()}{" | "}{model.DistributorSapAccountId}{" | "}{DateTime.Now}");
            var cacheKey = $"{CacheKeys.DMS_ORDER_USER_ACCOUNT_ID}{LoggedInUser()}{model.DistributorSapAccountId}";

            List<DmsOrder> redisDmsOrderList = new();
            _orderLogger.LogInformation($"{"DMS Order Cache Response:-"}{" | "}{JsonConvert.SerializeObject(redisDmsOrderList)}");

            if (redisDmsOrderList == null || redisDmsOrderList.Count() == 0)
            {
                redisDmsOrderList = await _dmsOrder.Table.
                    Where(c => c.UserId == LoggedInUser()).Include(c => c.OrderStatus).Include(c => c.OrderType).Include(c => c.DeliveryStatus)
                    .Include(x => x.DmsOrderItems).ToListAsync();

                if (!string.IsNullOrEmpty(model.DistributorSapAccountId))
                {
                    redisDmsOrderList = redisDmsOrderList.Where(x => x.DistributorSapAccountId == Convert.ToInt32(model.DistributorSapAccountId)).ToList();
                }
            }

            RequestSortingDto sorting = new();
            ConstructSorting(model.Sort, ref sorting);

            var requestFilter = new RequestFilterDto(model.CountryCode, model.CompanyCode, model.SearchKeyword, model.DeliveryMethodCode,
                Convert.ToInt32(model.DistributorSapAccountId), model.OrderStatusCode, model.IsATC, model.FromDate, model.ToDate);

            var expression = new DmsOrderDistrubutorQueryObject(requestFilter).Expression;
            var orderExpression = ProcessOrderFunc(sorting);

            var orderItems = redisDmsOrderList.AsQueryable().AsNoTrackingWithIdentityResolution()
                .OrderByWhere(expression, orderExpression);

            var totalCount = orderItems.Count();
            var totalPages = NumberManipulator.PageCountConverter(totalCount, model.PageSize);

            var paginateResp = orderItems.Paginate(model.PageIndex, model.PageSize);
            var orderList = paginateResp.ToList();

            var mapRecord = orderList.Select(c => new
            {
                parentSapNumber = (c.ParentOrderSapNumber != null && c.ParentOrderSapNumber != "0") ? c.ParentOrderSapNumber : c.OrderSapNumber,
                dmsorderId = c.Id,
                dateCreated = c.DateCreated.ConvertToLocal(),
                companyCode = c.CompanyCode,
                countryCode = c.CountryCode,
                orderStatus = new StatusResponseDto { Code = c.OrderStatus.Code, Name = c.OrderStatus.Name },
                orderType = new StatusResponseDto { Code = c.OrderType.Code, Name = c.OrderType.Name },
                deliveryStatus = c.DeliveryStatus == null ? null : new StatusResponseDto { Code = c.DeliveryStatus.Code, Name = c.DeliveryStatus.Name },
                isATC = c.IsAtc,
                estimatedNetvalue = c.EstimatedNetValue,
                orderSapNetValue = c.OrderSapNetValue,
                numItems = c.DmsOrderItems.Count,
                dmsOrderGroupId = c.DmsOrderGroupId,

            }).ToList();
            var response = new PaginatedListVM<object>(mapRecord,
                new PaginationMetaData(model.PageIndex, model.PageSize, totalPages, totalCount));
            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL,
                new { pagination = response.Pagination, dmsOrders = response.Items });
        }
        public async Task<ApiResponse> GetMyDmsOrderBySapAccountId(DmsOrderBySapAccountIdQueryRequestDTO model)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            _orderLogger.LogInformation($"{"About to retrieve DMS Order for Distributor Account ID"}{" | "}{LoggedInUser()}{" | "}{model.DistributorSapAccountId}{" | "}{DateTime.UtcNow}");
            var sapDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == Convert.ToInt32(model.DistributorSapAccountId));
            _orderLogger.LogInformation($"{$"Fetching orders from SAP took: {stopwatch.Elapsed.TotalSeconds} || at: {DateTime.UtcNow}"}");
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(sapDetail)}");

            if (sapDetail == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            var response = await _sapService.GetOrder(sapDetail.CompanyCode, sapDetail.CountryCode, sapDetail.DistributorSapNumber, model.FromDate, model.ToDate);
            response = response.Where(x => (string.IsNullOrEmpty(model.OrderStatusCode) || x.Status.Code == model.OrderStatusCode)
            && (string.IsNullOrEmpty(model.OrderTypeCode) || x.OrderType.Code == model.OrderTypeCode)).ToList();

            var totalCount = response.Count;
            if (model.PageSize > totalCount)
            {
                model.PageSize = totalCount;
            }
            var totalPages = NumberManipulator.PageCountConverter(totalCount, model.PageSize);

            if (model.Sort == OrderSortingEnum.DateAscending)
            {
                response = response.OrderBy(x => x.DateCreated).ToList();
            }
            else if (model.Sort == OrderSortingEnum.DateDescending)
            {
                response = response.OrderByDescending(x => x.DateCreated).ToList();
            }
            else if (model.Sort == OrderSortingEnum.ValueAscending)
            {
                response = response.OrderBy(x => x.NetValue).ToList();
            }
            else if (model.Sort == OrderSortingEnum.ValueDescending)
            {
                response = response.OrderByDescending(x => x.NetValue).ToList();
            }
            response = response.Skip((model.PageIndex - 1) * model.PageSize).Take(model.PageSize).ToList();

            var mapRecord = response.Select(c => new
            {
                id = c.Id,
                parentId = c.ParentId,
                dateCreated = c.DateCreated,
                distributorNumber = c.DistributorNumber,
                numberOfItem = c.NumberOfItems,
                netValue = c.NetValue,
                status = new
                {
                    code = c.Status?.Code,
                    name = c.Status?.Name
                },
                orderType = c.OrderType
            }).ToList();

            var response1 = new PaginatedListVM<object>(mapRecord,
                new PaginationMetaData(model.PageIndex, model.PageSize, totalPages, totalCount));
            _orderLogger.LogInformation($"{$"Order response returned from DMS: {stopwatch.Elapsed.TotalSeconds} || at: {DateTime.UtcNow}"}");
            stopwatch.Stop();

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL, new { sapOrders = response1 });
        }
        public async Task<ApiResponse> GetMyDmsOrderByOrderId(int orderId)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order By Order Id"}{" | "}{LoggedInUser()}{" | "}{orderId}{" | "}{DateTime.Now}");

            DmsOrder redisDmsOrderItem = await _dmsOrder.Table.Where(c => c.Id == orderId).Include(c => c.OrderStatus).Include(c => c.DeliveryStatus)
                   .Include(c => c.DistributorSapAccount).Include(c => c.OrderType).Include(c => c.Plant).FirstOrDefaultAsync();
            if (redisDmsOrderItem != null)
            {
                redisDmsOrderItem.DmsOrderItems = await _orderItem.Table.Where(c => c.OrderId == orderId)
                .Include(c => c.Product).ToListAsync();
            }

            if (redisDmsOrderItem == null)
                return ResponseHandler.SuccessResponse("No record found...");

            if (redisDmsOrderItem?.UserId != LoggedInUser())
                throw new NotFoundException(ErrorCodes.INVALID_ROUTE.Key, ErrorCodes.INVALID_ROUTE.Value);
            //Get delivery Country with codeDeliveryMethodDto
            var deliveryMet = await _deliveryMethod.Table.FirstOrDefaultAsync(c => c.Code == redisDmsOrderItem.DeliveryMethodCode);

            //Get delivery Country with code
            var truckSize = await _truckSize.Table.FirstOrDefaultAsync(c => c.Code == redisDmsOrderItem.TruckSizeCode);

            var orderItems = redisDmsOrderItem.DmsOrderItems.Select(c => new DmsOrderItemDto
            {
                DateModified = c.DateModified = c.DateModified == null ? null : c.DateModified.Value.ConvertToLocal(),
                DateCreated = c.DateCreated.ConvertToLocal(),
                SalesUnitOfMeasureCode = c.SalesUnitOfMeasureCode,
                ProductId = c.ProductId,
                SapDeliveryQuality = c.SapDeliveryQuality,
                UserId = c.UserId,
                Id = c.Id,
                OrderId = c.OrderId,
                OrderItemSapNumber = c.OrderItemSapNumber,
                Quantity = c.Quantity,
                SapPricePerUnit = c.SapPricePerUnit,
                SapNetValue = c.SapNetValue,
                salesUnitOfMeasure = new DeliveryStatusDto
                {
                    Name = c.SalesUnitOfMeasureCode,
                    Code = c.SalesUnitOfMeasureCode,
                },
                product = new ProductDto
                {
                    Id = c.Product.Id,
                    Name = c.Product.Name,
                    Price = c.Product.Price,
                    DateRefreshed = c.Product.DateRefreshed,
                    CompanyCode = c.Product.CompanyCode,
                    CountryCode = c.Product.CountryCode,
                    UnitOfMeasureCode = c.Product.UnitOfMeasureCode,
                    ProductSapNumber = c.Product.ProductSapNumber
                }
            });
            var orderItem = new ViewDmsOrderDetailsDto()
            {
                Id = redisDmsOrderItem.Id,
                DmsOrderGroupId = redisDmsOrderItem.DmsOrderGroupId,
                OrderSapNumber = redisDmsOrderItem.OrderSapNumber,
                ParentOrderSapNumber = redisDmsOrderItem.ParentOrderSapNumber == null ? redisDmsOrderItem.OrderSapNumber : redisDmsOrderItem.ParentOrderSapNumber,
                DistributorSapAccountId = redisDmsOrderItem.DistributorSapAccountId,
                OrderStatusId = redisDmsOrderItem.OrderStatusId,
                OrderTypeId = redisDmsOrderItem.OrderTypeId,
                TruckSizeCode = redisDmsOrderItem.TruckSizeCode,
                DateCreated = redisDmsOrderItem.DateCreated.ConvertToLocal(),
                ShoppingCartId = redisDmsOrderItem.ShoppingCartId,
                CompanyCode = redisDmsOrderItem.CompanyCode,
                CountryCode = redisDmsOrderItem.CountryCode,

                PlantCode = redisDmsOrderItem.PlantCode,
                IsAtc = redisDmsOrderItem.IsAtc,
                CustomerPaymentReference = redisDmsOrderItem.CustomerPaymentReference,
                CustomerPaymentDate = redisDmsOrderItem.CustomerPaymentDate,
                OrderStatus = new OrderStatusDto { Code = redisDmsOrderItem.OrderStatus.Code, Name = redisDmsOrderItem.OrderStatus.Name },
                OrderType = new OrderTypeDto { Code = redisDmsOrderItem.OrderType.Code, Name = redisDmsOrderItem.OrderType.Name },
                truckSize = new TruckSizeDto { Code = truckSize?.Code, Name = truckSize?.Name },
                deliveryMethod = new DeliveryMethodDto { Code = deliveryMet?.Code, Name = deliveryMet?.Name },
                DistributorSapAccount = new DistributorSapAccountDto
                {
                    DistributorSapAccountId = redisDmsOrderItem.DistributorSapAccount.DistributorSapAccountId,
                    DistributorSapNumber = redisDmsOrderItem.DistributorSapAccount.DistributorSapNumber,
                    DistributorName = redisDmsOrderItem.DistributorSapAccount.DistributorName,
                    CompanyCode = redisDmsOrderItem.DistributorSapAccount.CompanyCode,
                    CountryCode = redisDmsOrderItem.DistributorSapAccount.CountryCode,
                    AccountType = redisDmsOrderItem.DistributorSapAccount.AccountType,
                    DateRefreshed = redisDmsOrderItem.DistributorSapAccount.DateRefreshed,
                },
                EstimatedNetValue = redisDmsOrderItem.EstimatedNetValue,
                OrderSapNetValue = redisDmsOrderItem.OrderSapNetValue,
                SapVat = redisDmsOrderItem.SapVat,
                SapFreightCharges = redisDmsOrderItem.SapFreightCharges,
                DeliveryDate = redisDmsOrderItem.DeliveryDate,
                DeliveryAddress = redisDmsOrderItem.DeliveryAddress,
                DeliveryCity = redisDmsOrderItem.DeliveryCity,
                OrderItems = orderItems,
                DeliveryCountry = new CountryResponse
                {
                    name = redisDmsOrderItem.DeliveryCountryCode,
                    code = redisDmsOrderItem.DeliveryCountryCode
                },
                DeliveryState = new DeliveryStatusDto
                {
                    Code = redisDmsOrderItem.DeliveryStateCode,
                }
            };

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL, new { dmsOrder = orderItem });
        }
        public async Task<ApiResponse> GetMySAPOrderByDAccountIdAndOrderSapNo(string orderSapNo, int distrubutorAccountId)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order By Order Sap No"}{" | "}{LoggedInUser()}{" | "}{orderSapNo}{" | "}{DateTime.UtcNow}");

            var sapDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == distrubutorAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(sapDetail)}");

            if (sapDetail == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            List<DmsOrder> redisDmsOrderItems = _sapService.GetSapOrders();
            var redisDmsOrderItem = redisDmsOrderItems?.Where(c => c.OrderSapNumber == orderSapNo)?.FirstOrDefault();
            if (redisDmsOrderItem != null)
            {
                redisDmsOrderItem.DmsOrderItems = redisDmsOrderItem.DmsOrderItems;
            }
            if (redisDmsOrderItem == null)
            {
                var sapOrder = await _sapService.GetOrderDetails(sapDetail.CompanyCode, sapDetail.CountryCode, orderSapNo);
                if (sapOrder != null)
                {
                    var SapOrderResponseDto = new ViewSapOrderDetailsDto();

                    SapOrderResponseDto.Id = sapOrder.Id.ToString();
                    SapOrderResponseDto.ParentId = sapOrder.parentId == 0 ? sapOrder.Id.ToString() : sapOrder.parentId.ToString();
                    SapOrderResponseDto.DistributorNumber = sapOrder.distirbutorNumber.ToString();
                    SapOrderResponseDto.DateCreated = DateTime.Parse(sapOrder.dateCreated);
                    SapOrderResponseDto.NetValue = decimal.TryParse(sapOrder.netValue, out decimal netval) ? netval : 0;
                    SapOrderResponseDto.OrderType = new SApStatusResponse { Code = sapOrder.orderType.Code, Name = sapOrder.orderType.Name };
                    SapOrderResponseDto.Status = new SApStatusResponse { Code = sapOrder.Status.Code, Name = sapOrder.Status.Name };
                    SapOrderResponseDto.DeliveryStatus = new SApStatusResponse { Code = sapOrder.deliveryStatus.code, Name = sapOrder.deliveryStatus.name };
                    SapOrderResponseDto.DeliveryMethod = new SApStatusResponse { Code = sapOrder.deliveryMethod.code, Name = sapOrder.deliveryMethod.name };
                    SapOrderResponseDto.DeliveryBlock = new SApStatusResponse { Code = sapOrder.deliveryMethod.code, Name = sapOrder.deliveryMethod.name };
                    SapOrderResponseDto.Reference = sapOrder.reference;
                    SapOrderResponseDto.EstimatedNetValue = decimal.TryParse(sapOrder.netValue, out decimal netvaln) ? netvaln : 0;
                    SapOrderResponseDto.DeliveryDate = DateTime.TryParse(sapOrder.delivery.deliveryDate, out DateTime dateTime) ? dateTime : null;
                    SapOrderResponseDto.DeliveryCity = sapOrder.deliveryCity;
                    SapOrderResponseDto.DeliveryAddress = sapOrder.deliveryAddress;
                    SapOrderResponseDto.Vat = decimal.TryParse(sapOrder.vat, out decimal vat) ? vat : 0;
                    SapOrderResponseDto.SapFreightCharges = decimal.TryParse(sapOrder.freightCharges, out decimal frieght) ? frieght : 0;
                    SapOrderResponseDto.Trucksize = new SApStatusResponse { };
                    SapOrderResponseDto.Delivery = new SapDelivery
                    {
                        deliveryDate = DateTime.TryParse(sapOrder.delivery.deliveryDate, out DateTime dat) ? dat : null,
                        Id = (int?)sapOrder.delivery.Id,
                        loadingDate = DateTime.TryParse(sapOrder.delivery.loadingDate, out DateTime date) ? date : null,
                        pickUpDate = DateTime.TryParse(sapOrder.delivery.pickUpDate, out DateTime dateTime1) ? dateTime1 : null,
                        transportDate = DateTime.TryParse(sapOrder.delivery.transportDate, out DateTime dateTime2) ? dateTime2 : null,
                        plannedGoodsMovementDate = DateTime.TryParse(sapOrder.delivery.plannedGoodsMovementDate, out DateTime Datt) ? Datt : null,
                        WayBillNumber = sapOrder.delivery.WayBillNumber,
                    };
                    var OrderItems = new List<OrderResponseDto>();
                    foreach (var item in sapOrder.orderItems)
                    {
                        OrderItems.Add(new OrderResponseDto
                        {
                            Id = item.Id.ToString(),
                            OrderId = item.orderId.ToString(),
                            Product = new ProductSapDt0
                            {
                                ProductId = item.product.productId,
                                Name = item.product.name,
                                ProductType = item.product.productType
                            },
                            SalesUnitOfMeasure = new SApStatusResponse { Name = item.salesUnitOfMeasure.name, Code = item.salesUnitOfMeasure.code },
                            Plant = new SApStatusResponse { Code = item.plant.Code, Name = item.plant.Name },
                            ShippingPoint = new SApStatusResponse { },
                            PricePerUnit = double.Parse(item.pricePerUnit),
                            OrderQuantity = item.orderQuantity,
                            DeliveryQuantity = item.deliveryQuantity,
                            NetValue = double.Parse(item.netValue),

                        });
                    }
                    SapOrderResponseDto.OrderItems = OrderItems;
                    return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL, new { sapOrder = SapOrderResponseDto });
                }

            }
            return ResponseHandler.SuccessResponse("No record found...");

        }
        public async Task<ApiResponse> SubmitOrder(DmsRequestDTO model)
        {
            var dmsOrdersList = new List<DmsOrder>();
            if (model.DmsOrderId != null)
            {
                var dmsOrder = await _dmsOrder.Table.Include(c => c.DistributorSapAccount).FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.Id == model.DmsOrderId);
                if (dmsOrder == null)
                    throw new NotFoundException(ErrorCodes.DMS_ORDER_NOTFOUND.Key, ErrorCodes.DMS_ORDER_NOTFOUND.Value);

                dmsOrdersList.Add(dmsOrder);
            }
            if (model.DmsOrderGroupId != null)
            {
                //Check the groupId Here
                var dmsOrders = await _dmsOrder.Table.Include(c => c.DistributorSapAccount).Where(c => c.UserId == LoggedInUser() && c.DmsOrderGroupId == model.DmsOrderGroupId).ToListAsync();
                foreach (var ord in dmsOrders)
                {
                    dmsOrdersList.Add(ord);
                }
            }

            foreach (var dmsOrder in dmsOrdersList)
            {
                var oldOrder1 = await _dmsOrder.Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dmsOrder.Id);
                if (dmsOrder.IsAtc)
                    throw new NotFoundException(ErrorCodes.OPERATION_NOT_ALLOW_ON_ATC.Key, ErrorCodes.OPERATION_NOT_ALLOW_ON_ATC.Value);


                var distributorSapAccount = await _distributorSapNo.Table.FirstOrDefaultAsync(x => x.DistributorSapAccountId == dmsOrder.DistributorSapAccountId);
                if (distributorSapAccount == null)
                    throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

                var sapWallet = await _sapService.GetWallet(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, distributorSapAccount.DistributorSapNumber);

                if (sapWallet == null)
                    throw new NotFoundException(ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Key, ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Value);

                //Check this later
                if (Math.Abs(sapWallet.AvailableBalance) < (dmsOrder.EstimatedNetValue ?? 0))
                    throw new NotFoundException(ErrorCodes.INSUFICIENT_FUNDS.Key, ErrorCodes.INSUFICIENT_FUNDS.Value);


                dmsOrder.TruckSizeCode = model.TruckSizeCode;
                dmsOrder.DeliveryMethodCode = model.DeliveryMethodCode;
                dmsOrder.PlantCode = model.PlantCode;
                dmsOrder.DeliveryStateCode = model.DeliveryStateCode;
                dmsOrder.DeliveryCity = model.DeliveryCity;
                dmsOrder.DeliveryDate = model.DeliveryDate;
                dmsOrder.DeliveryAddress = model.DeliveryAddress;
                dmsOrder.DeliveryCountryCode = model.DeliveryCountryCode;
                dmsOrder.OrderStatusId = (int)Application.Enums.OrderStatus.PendingOtp;
                dmsOrder.DateModified = DateTime.Now;
                dmsOrder.ChannelCode = model.ChannelCode;

                await _dmsOrder.CommitAsync(default);


                DmsOrderUpdatedMessage dmsOrderRefreshedMessage1 = new();
                dmsOrderRefreshedMessage1.DmsOrderId = dmsOrder.Id;
                dmsOrderRefreshedMessage1.DateModified = DateTime.Now;
                dmsOrderRefreshedMessage1.ModifiedByUserId = dmsOrder.UserId;
                dmsOrderRefreshedMessage1.UserId = dmsOrder.UserId;
                dmsOrderRefreshedMessage1.CompanyCode = dmsOrder.CompanyCode;
                dmsOrderRefreshedMessage1.CountryCode = dmsOrder.CountryCode;
                dmsOrderRefreshedMessage1.OrderSapNumber = dmsOrder.OrderSapNumber;
                dmsOrderRefreshedMessage1.DistributorSapAccount = new DistributorSapAccountResponse(distributorSapAccount.DistributorSapAccountId, distributorSapAccount.DistributorSapNumber, distributorSapAccount.DistributorName);
                dmsOrderRefreshedMessage1.EstimatedNetValue = dmsOrder.EstimatedNetValue;
                dmsOrderRefreshedMessage1.OldOrderStatus = new OldOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder1.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder1.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage1.NewOrderStatus = new NewOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == dmsOrder.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == dmsOrder.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage1.OrderType = new NewOrderStatusResponse
                {
                    Code = _orderType.Table.FirstOrDefault(x => x.Id == dmsOrder.OrderStatusId)?.Code,
                    Name = _orderType.Table.FirstOrDefault(x => x.Id == dmsOrder.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage1.OldtruckSizeCode = oldOrder1.TruckSizeCode;
                dmsOrderRefreshedMessage1.NewtruckSizeCode = dmsOrder.TruckSizeCode;
                dmsOrderRefreshedMessage1.OldDeliveryMethodCode = dmsOrder.DeliveryMethodCode;
                dmsOrderRefreshedMessage1.NewDeliveryMethodCode = dmsOrder.DeliveryMethodCode;
                dmsOrderRefreshedMessage1.Newplant = new Application.DTOs.Events.PlantResponse
                {
                    PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == model.PlantCode)?.Id),
                    Name = _plant.Table.FirstOrDefault(p => p.Code == model.PlantCode)?.Name,
                    Code = _plant.Table.FirstOrDefault(p => p.Code == model.PlantCode)?.Code,

                };
                dmsOrderRefreshedMessage1.OldDeliveryDate = (DateTime)dmsOrder.DeliveryDate;
                dmsOrderRefreshedMessage1.NewDeliveryDate = (DateTime)dmsOrder.DeliveryDate;
                dmsOrderRefreshedMessage1.OldDeliveryAddress = oldOrder1.DeliveryAddress;
                dmsOrderRefreshedMessage1.NewDeliveryAddress = dmsOrder.DeliveryAddress;
                dmsOrderRefreshedMessage1.OldDeliveryCity = oldOrder1.DeliveryCity;
                dmsOrderRefreshedMessage1.NewDeliveryCity = dmsOrder.DeliveryCity;
                dmsOrderRefreshedMessage1.OldDeliveryStateCode = oldOrder1.DeliveryStateCode;
                dmsOrderRefreshedMessage1.NewDeliveryStateCode = dmsOrder.DeliveryStateCode;
                dmsOrderRefreshedMessage1.OldDeliveryCountryCode = oldOrder1.DeliveryCountryCode;
                dmsOrderRefreshedMessage1.NewDeliveryCountryCode = dmsOrder.DeliveryCountryCode;
                foreach (var item in dmsOrder.DmsOrderItems)
                {
                    dmsOrderRefreshedMessage1.DmsOrderItems.Add(new OrderItemsResponse
                    {
                        DateCreated = item.DateCreated,
                        DmsOrderItemId = item.Id,
                        Product = new ProductResponse(item.ProductId, item.Product.Price, item.Product.Name),
                        Quantity = item.Quantity,
                        SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode
                    });
                }


                await _messageBus.PublishTopicMessage(dmsOrderRefreshedMessage1, EventMessages.ORDER_DMS_UPDATED);
            }


            GetEmail();
            GetPhone();
            var otpItem = await _otpService.GenerateOtp(Email, model.DmsOrderId, model.DmsOrderGroupId, false, PhoneNumber, LoggedInUser());
            OrdersOtpGeneratedMessage otpVM = new()
            {
                DateCreated = DateTime.Now,
                DateExpiry = otpItem.DateExpiry,
                EmailAddress = Email,
                PhoneNumber = PhoneNumber,
                OtpCode = otpItem.Code,
                OtpId = otpItem.Id,
            };
            await _messageBus.PublishTopicMessage(otpVM, EventMessages.ORDER_OTP_GENERATED);

            return ResponseHandler.SuccessResponse(SuccessMessages.OTP_GENERATED_SUCCESSFULLY, new { otp = new { otpid = otpItem.Id } });
        }
        public async Task<ApiResponse> SubmitOrderV2(DmsRequestDTOV2 model)
        {
            var dmsOrdersList = new List<DmsOrder>();
            if (model.DmsOrderGroupId != null)
            {
                //Check the groupId Here
                var dmsOrders = await _dmsOrder.Table.Include(c => c.DistributorSapAccount).Where(c => c.UserId == LoggedInUser() && c.DmsOrderGroupId == model.DmsOrderGroupId).ToListAsync();
                foreach (var ord in dmsOrders)
                {
                    dmsOrdersList.Add(ord);
                }
            }

            foreach (var dmsOrder in dmsOrdersList)
            {
                var oldOrder1 = await _dmsOrder.Table.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dmsOrder.Id);
                if (dmsOrder.IsAtc)
                    throw new NotFoundException(ErrorCodes.OPERATION_NOT_ALLOW_ON_ATC.Key, ErrorCodes.OPERATION_NOT_ALLOW_ON_ATC.Value);


                var distributorSapAccount = await _distributorSapNo.Table.FirstOrDefaultAsync(x => x.DistributorSapAccountId == dmsOrder.DistributorSapAccountId);
                if (distributorSapAccount == null)
                    throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

                var sapWallet = await _sapService.GetWallet(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, distributorSapAccount.DistributorSapNumber);

                if (sapWallet == null)
                    throw new NotFoundException(ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Key, ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Value);

                //Check this later
                if (Math.Abs(sapWallet.AvailableBalance) < (dmsOrder.EstimatedNetValue ?? 0))
                    throw new NotFoundException(ErrorCodes.INSUFICIENT_FUNDS.Key, ErrorCodes.INSUFICIENT_FUNDS.Value);


                dmsOrder.TruckSizeCode = model.TruckSizeCode;
                dmsOrder.DeliveryDate = model.DeliveryDate;
                dmsOrder.DeliveryAddress = model.DeliveryAddress;
                dmsOrder.OrderStatusId = (int)Application.Enums.OrderStatus.PendingOtp;
                dmsOrder.DateModified = DateTime.Now;
                dmsOrder.ChannelCode = model.ChannelCode;
                dmsOrder.CustomerPaymentReference = model.CustomerPaymentReference;
                dmsOrder.CustomerPaymentDate = model.CustomerPaymentDate;

                _dmsOrder.Table.Update(dmsOrder);
                await _dmsOrder.CommitAsync(default);

                DmsOrderUpdatedMessage dmsOrderRefreshedMessage1 = new();
                dmsOrderRefreshedMessage1.DmsOrderId = dmsOrder.Id;
                dmsOrderRefreshedMessage1.DateModified = DateTime.Now;
                dmsOrderRefreshedMessage1.ModifiedByUserId = dmsOrder.UserId;
                dmsOrderRefreshedMessage1.UserId = dmsOrder.UserId;
                dmsOrderRefreshedMessage1.CompanyCode = dmsOrder.CompanyCode;
                dmsOrderRefreshedMessage1.CountryCode = dmsOrder.CountryCode;
                dmsOrderRefreshedMessage1.OrderSapNumber = dmsOrder.OrderSapNumber;
                dmsOrderRefreshedMessage1.DistributorSapAccount = new DistributorSapAccountResponse(distributorSapAccount.DistributorSapAccountId, distributorSapAccount.DistributorSapNumber, distributorSapAccount.DistributorName);
                dmsOrderRefreshedMessage1.EstimatedNetValue = dmsOrder.EstimatedNetValue;
                dmsOrderRefreshedMessage1.OldOrderStatus = new OldOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder1.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder1.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage1.NewOrderStatus = new NewOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == dmsOrder.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == dmsOrder.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage1.OrderType = new NewOrderStatusResponse
                {
                    Code = _orderType.Table.FirstOrDefault(x => x.Id == dmsOrder.OrderStatusId)?.Code,
                    Name = _orderType.Table.FirstOrDefault(x => x.Id == dmsOrder.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage1.OldtruckSizeCode = oldOrder1.TruckSizeCode;
                dmsOrderRefreshedMessage1.NewtruckSizeCode = dmsOrder.TruckSizeCode;
                dmsOrderRefreshedMessage1.OldDeliveryMethodCode = dmsOrder.DeliveryMethodCode;
                dmsOrderRefreshedMessage1.NewDeliveryMethodCode = dmsOrder.DeliveryMethodCode;
                dmsOrderRefreshedMessage1.OldDeliveryDate = (DateTime)dmsOrder.DeliveryDate;
                dmsOrderRefreshedMessage1.NewDeliveryDate = (DateTime)dmsOrder.DeliveryDate;
                dmsOrderRefreshedMessage1.OldDeliveryAddress = oldOrder1.DeliveryAddress;
                dmsOrderRefreshedMessage1.NewDeliveryAddress = dmsOrder.DeliveryAddress;
                dmsOrderRefreshedMessage1.OldDeliveryCity = oldOrder1.DeliveryCity;
                dmsOrderRefreshedMessage1.NewDeliveryCity = dmsOrder.DeliveryCity;
                dmsOrderRefreshedMessage1.OldDeliveryStateCode = oldOrder1.DeliveryStateCode;
                dmsOrderRefreshedMessage1.NewDeliveryStateCode = dmsOrder.DeliveryStateCode;
                dmsOrderRefreshedMessage1.OldDeliveryCountryCode = oldOrder1.DeliveryCountryCode;
                dmsOrderRefreshedMessage1.NewDeliveryCountryCode = dmsOrder.DeliveryCountryCode;
                foreach (var item in dmsOrder.DmsOrderItems)
                {
                    dmsOrderRefreshedMessage1.DmsOrderItems.Add(new OrderItemsResponse
                    {
                        DateCreated = item.DateCreated,
                        DmsOrderItemId = item.Id,
                        Product = new ProductResponse(item.ProductId, item.Product.Price, item.Product.Name),
                        Quantity = item.Quantity,
                        SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode
                    });
                }
                await _messageBus.PublishTopicMessage(dmsOrderRefreshedMessage1, EventMessages.ORDER_DMS_UPDATED);
            }

            GetEmail();
            GetPhone();
            var otpItem = await _otpService.GenerateOtp(Email, null, model.DmsOrderGroupId, false, PhoneNumber, LoggedInUser());
            OrdersOtpGeneratedMessage otpVM = new()
            {
                DateCreated = DateTime.Now,
                DateExpiry = otpItem.DateExpiry,
                EmailAddress = Email,
                PhoneNumber = PhoneNumber,
                OtpCode = otpItem.Code,
                OtpId = otpItem.Id,
            };
            await _messageBus.PublishTopicMessage(otpVM, EventMessages.ORDER_OTP_GENERATED);

            return ResponseHandler.SuccessResponse(SuccessMessages.OTP_GENERATED_SUCCESSFULLY, new { otp = new { otpid = otpItem.Id } });
        }
        public async Task<ApiResponse> ValidateOtp(string code)
        {
            var otpItem = await _otp.Table.FirstOrDefaultAsync(c => c.Code == code);
            _orderLogger.LogInformation($"{"OTP DB Response:-"}{" | "}{JsonConvert.SerializeObject(otpItem)}");

            if (otpItem == null)
                throw new NotFoundException(ErrorCodes.INVALID_OTP.Key, ErrorCodes.INVALID_OTP.Value);

            else if (otpItem.OtpStatusId == (int)Application.Enums.OrderStatus.Validated)
                throw new NotFoundException(ErrorCodes.OTP_HAS_BEEN_USED.Key, ErrorCodes.OTP_HAS_BEEN_USED.Value);

            else if (otpItem.OtpStatusId == (int)Application.Enums.OrderStatus.InValidated)
                throw new NotFoundException(ErrorCodes.MAXIMUM_RETRIES_EXCEEDED.Key, ErrorCodes.MAXIMUM_RETRIES_EXCEEDED.Value);

            else if (otpItem.DateExpiry > DateTime.UtcNow)
                throw new NotFoundException(ErrorCodes.OPT_EXPIRED.Key, ErrorCodes.OPT_EXPIRED.Value);

            else
            {
                otpItem.OtpStatusId = (int)Application.Enums.OrderStatus.Validated;
                _otp.Table.Update(otpItem);

                var oldOrder = await _dmsOrder.Table.AsNoTracking().FirstOrDefaultAsync(c => c.Id == otpItem.DmsOrderId);



                var orderItem = await _dmsOrder.Table.FirstOrDefaultAsync(c => c.Id == otpItem.DmsOrderId);
                orderItem.OrderStatusId = (int)Application.Enums.OrderStatus.ProcessingSubmission;
                orderItem.DateModified = DateTime.UtcNow;
                orderItem.DateSubmittedOnDms = DateTime.UtcNow;
                _dmsOrder.Table.Update(orderItem);

                var sapDetail = orderItem.DistributorSapAccount;

                DmsOrderUpdatedMessage dmsOrderRefreshedMessage = new();

                dmsOrderRefreshedMessage.DmsOrderId = orderItem.Id;
                dmsOrderRefreshedMessage.DateModified = DateTime.UtcNow;
                dmsOrderRefreshedMessage.ModifiedByUserId = orderItem.UserId;
                dmsOrderRefreshedMessage.UserId = orderItem.UserId;
                dmsOrderRefreshedMessage.CompanyCode = orderItem.CompanyCode;
                dmsOrderRefreshedMessage.CountryCode = orderItem.CountryCode;
                dmsOrderRefreshedMessage.OrderSapNumber = orderItem.OrderSapNumber;
                dmsOrderRefreshedMessage.DistributorSapAccount = new DistributorSapAccountResponse(sapDetail.DistributorSapAccountId, sapDetail.DistributorSapNumber, sapDetail.DistributorName);
                dmsOrderRefreshedMessage.EstimatedNetValue = orderItem.EstimatedNetValue;
                dmsOrderRefreshedMessage.OldOrderStatus = new OldOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage.NewOrderStatus = new NewOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage.OrderType = new NewOrderStatusResponse
                {
                    Code = _orderType.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Code,
                    Name = _orderType.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage.OldtruckSizeCode = oldOrder.TruckSizeCode;
                dmsOrderRefreshedMessage.NewtruckSizeCode = orderItem.TruckSizeCode;
                dmsOrderRefreshedMessage.OldDeliveryMethodCode = orderItem.DeliveryMethodCode;
                dmsOrderRefreshedMessage.NewDeliveryMethodCode = orderItem.DeliveryMethodCode;
                dmsOrderRefreshedMessage.Oldplant = new Application.DTOs.Events.PlantResponse
                {
                    PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Id),
                    Name = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Name,
                    Code = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Code,

                };
                dmsOrderRefreshedMessage.Newplant = new Application.DTOs.Events.PlantResponse
                {
                    PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Id),
                    Name = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Name,
                    Code = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Code,

                };
                dmsOrderRefreshedMessage.OldDeliveryDate = (DateTime)orderItem.DeliveryDate;
                dmsOrderRefreshedMessage.NewDeliveryDate = (DateTime)orderItem.DeliveryDate;
                dmsOrderRefreshedMessage.OldDeliveryAddress = oldOrder.DeliveryAddress;
                dmsOrderRefreshedMessage.NewDeliveryAddress = orderItem.DeliveryAddress;
                dmsOrderRefreshedMessage.OldDeliveryCity = oldOrder.DeliveryCity;
                dmsOrderRefreshedMessage.NewDeliveryCity = orderItem.DeliveryCity;
                dmsOrderRefreshedMessage.OldDeliveryStateCode = oldOrder.DeliveryStateCode;
                dmsOrderRefreshedMessage.NewDeliveryStateCode = orderItem.DeliveryStateCode;
                dmsOrderRefreshedMessage.OldDeliveryCountryCode = oldOrder.DeliveryCountryCode;
                dmsOrderRefreshedMessage.NewDeliveryCountryCode = orderItem.DeliveryCountryCode;
                //dmsOrderRefreshedMessage.DmsOrderItems = (List<OrderItemsResponse>)orderItem.DmsOrderItems;
                foreach (var item in orderItem.DmsOrderItems)
                {
                    dmsOrderRefreshedMessage.DmsOrderItems.Add(new OrderItemsResponse
                    {
                        DateCreated = item.DateCreated,
                        DmsOrderItemId = item.Id,
                        Product = new ProductResponse(item.ProductId, item.Product.Price, item.Product.Name),
                        Quantity = item.Quantity,
                        SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode
                    });
                }

                await _messageBus.PublishTopicMessage(orderItem, EventMessages.ORDER_DMS_UPDATED);

                return ResponseHandler.SuccessResponse(SuccessMessages.ORDER_SENT_FOR_SUBMISSION);
            }
        }
        public async Task<ApiResponse> CancelOrder(CancelDmsRequestDTO request)
        {
            _orderLogger.LogInformation($"{"About to Cancel DMS Order By Order Id"}{" | "}{LoggedInUser()}{" | "}{request.DmsOrderId}{" | "}{DateTime.UtcNow}");

            if (request.DmsOrderGroupId != 0)
            {
                var orderList = await _dmsOrder.Table.Include(x => x.OrderStatus).Include(x => x.DistributorSapAccount)
                     .Where(x => x.DmsOrderGroupId == request.DmsOrderGroupId).ToListAsync();
                if (orderList.Count > 0)
                {
                    orderList.ForEach(x =>
                    {
                        x.OrderStatusId = (int)Application.Enums.OrderStatus.Cancelled;
                    });
                }
                _dmsOrder.UpdateRange(orderList);
                await _dmsOrder.CommitAsync(default);
            }
            if (request.DmsOrderId > 0)
            {
                var oldOrder = await _dmsOrder.Table.AsNoTracking().Include(x => x.OrderStatus).Include(x => x.DistributorSapAccount).FirstOrDefaultAsync(c => c.Id == request.DmsOrderId && c.UserId == LoggedInUser());
                var sapDetails = oldOrder.DistributorSapAccount;
                var orderItem = await _dmsOrder.Table.Include(o => o.OrderType).Include(o => o.OrderStatus).FirstOrDefaultAsync(c => c.Id == request.DmsOrderId && c.UserId == LoggedInUser());

                if (orderItem == null)
                    throw new NotFoundException(ErrorCodes.DMS_ORDER_NOTFOUND.Key, ErrorCodes.DMS_ORDER_NOTFOUND.Value);

                if (orderItem.OrderStatusId != (int)Application.Enums.OrderStatus.New
                    && orderItem.OrderStatusId != (int)Application.Enums.OrderStatus.Saved
                    && orderItem.OrderStatusId != (int)Application.Enums.OrderStatus.PendingOtp)
                {
                    DmsOrdersCancellationRequestedMessage requestedMessage = new();
                    requestedMessage.UserId = orderItem.UserId;
                    requestedMessage.OrderSapNumber = orderItem.OrderSapNumber;
                    requestedMessage.DateCreated = DateTime.UtcNow;
                    requestedMessage.CompanyCode = orderItem.CompanyCode;
                    requestedMessage.CountryCode = orderItem.CountryCode;
                    requestedMessage.DmsOrderId = orderItem.Id;
                    requestedMessage.orderStatus = orderItem.OrderStatus?.Name;
                    requestedMessage.orderType = orderItem.OrderType?.Name;

                    await _messageBus.PublishTopicMessage(requestedMessage, EventMessages.DMSORDER_CANCELLATION_REQUEST);
                }
                else
                {
                    orderItem.OrderStatusId = (int)Application.Enums.OrderStatus.Cancelled;
                    orderItem.DateModified = DateTime.UtcNow;

                    await _dmsOrder.UpdateAsync(orderItem);
                    await _dmsOrder.CommitAsync(default);

                    DmsOrderUpdatedMessage dmsOrderRefreshedMessage = new();

                    dmsOrderRefreshedMessage.DmsOrderId = orderItem.Id;
                    dmsOrderRefreshedMessage.DateModified = DateTime.UtcNow;
                    dmsOrderRefreshedMessage.ModifiedByUserId = orderItem.UserId;
                    dmsOrderRefreshedMessage.UserId = orderItem.UserId;
                    dmsOrderRefreshedMessage.CompanyCode = orderItem.CompanyCode;
                    dmsOrderRefreshedMessage.CountryCode = orderItem.CountryCode;
                    dmsOrderRefreshedMessage.OrderSapNumber = orderItem.OrderSapNumber;
                    dmsOrderRefreshedMessage.DistributorSapAccount = new DistributorSapAccountResponse(sapDetails.DistributorSapAccountId, sapDetails.DistributorSapNumber, sapDetails.DistributorName);
                    dmsOrderRefreshedMessage.EstimatedNetValue = orderItem.EstimatedNetValue;
                    dmsOrderRefreshedMessage.OldOrderStatus = new OldOrderStatusResponse
                    {
                        Code = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Code,
                        Name = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Name,
                    };
                    dmsOrderRefreshedMessage.NewOrderStatus = new NewOrderStatusResponse
                    {
                        Code = _orderStatus.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Code,
                        Name = _orderStatus.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Name,
                    };
                    dmsOrderRefreshedMessage.OrderType = new NewOrderStatusResponse
                    {
                        Code = _orderType.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Code,
                        Name = _orderType.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Name,
                    };
                    dmsOrderRefreshedMessage.OldtruckSizeCode = oldOrder.TruckSizeCode;
                    dmsOrderRefreshedMessage.NewtruckSizeCode = orderItem.TruckSizeCode;
                    dmsOrderRefreshedMessage.OldDeliveryMethodCode = orderItem.DeliveryMethodCode;
                    dmsOrderRefreshedMessage.NewDeliveryMethodCode = orderItem.DeliveryMethodCode;
                    dmsOrderRefreshedMessage.OldDeliveryDate = orderItem.DeliveryDate;
                    dmsOrderRefreshedMessage.NewDeliveryDate = orderItem.DeliveryDate;
                    dmsOrderRefreshedMessage.OldDeliveryAddress = oldOrder.DeliveryAddress;
                    dmsOrderRefreshedMessage.NewDeliveryAddress = orderItem.DeliveryAddress;
                    dmsOrderRefreshedMessage.OldDeliveryCity = oldOrder.DeliveryCity;
                    dmsOrderRefreshedMessage.NewDeliveryCity = orderItem.DeliveryCity;
                    dmsOrderRefreshedMessage.OldDeliveryStateCode = oldOrder.DeliveryStateCode;
                    dmsOrderRefreshedMessage.NewDeliveryStateCode = orderItem.DeliveryStateCode;
                    dmsOrderRefreshedMessage.OldDeliveryCountryCode = oldOrder.DeliveryCountryCode;
                    dmsOrderRefreshedMessage.NewDeliveryCountryCode = orderItem.DeliveryCountryCode;
                    foreach (var item in orderItem.DmsOrderItems)
                    {
                        dmsOrderRefreshedMessage.DmsOrderItems.Add(new OrderItemsResponse
                        {
                            DateCreated = item.DateCreated,
                            DmsOrderItemId = item.Id,
                            Product = new ProductResponse(item.ProductId, item.Product.Price, item.Product.Name),
                            Quantity = item.Quantity,
                            SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode
                        });
                    }


                    await _messageBus.PublishTopicMessage(dmsOrderRefreshedMessage, EventMessages.ORDER_DMS_UPDATED);
                }

            }
            else if (!string.IsNullOrEmpty(request.orderSapNumber) && request.distributorSapAccountId != 0)
            {
                var sapDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == request.distributorSapAccountId);
                _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(sapDetail)}");

                if (sapDetail == null)
                    throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

                var cacheKey = $"{CacheKeys.DMS_ORDER}{LoggedInUser}{request.orderSapNumber}";
                var redisDmsOrderList = await _cache.GetAsync<DmsOrder>(cacheKey);
                if (redisDmsOrderList == null)
                {
                    redisDmsOrderList = await _dmsOrder.Table.Include(x => x.OrderType).Include(o => o.OrderStatus).Where(c => c.CompanyCode == sapDetail.CompanyCode
                    && c.CountryCode == sapDetail.CountryCode && c.OrderSapNumber == request.orderSapNumber)
                        .Include(c => c.OrderStatus).Include(c => c.OrderType).FirstOrDefaultAsync();

                    redisDmsOrderList.DmsOrderItems = _orderItem.Table.Where(c => c.OrderId == redisDmsOrderList.Id)
                        .Include(c => c.Product).ToList();
                }
                DmsOrdersCancellationRequestedMessage requestedMessage = new();
                requestedMessage.UserId = redisDmsOrderList.UserId;
                requestedMessage.OrderSapNumber = redisDmsOrderList.OrderSapNumber;
                requestedMessage.DateCreated = DateTime.UtcNow;
                requestedMessage.CompanyCode = redisDmsOrderList.CompanyCode;
                requestedMessage.CountryCode = redisDmsOrderList.CountryCode;
                requestedMessage.DmsOrderId = redisDmsOrderList.Id;
                requestedMessage.orderStatus = redisDmsOrderList.OrderStatus?.Name;
                requestedMessage.orderType = redisDmsOrderList.OrderType?.Name;

                await _messageBus.PublishTopicMessage(requestedMessage, EventMessages.DMSORDER_CANCELLATION_REQUEST);
            }

            return ResponseHandler.SuccessResponse("Order Cancelled");
        }
        public async Task<ApiResponse> UpdateOrder(DmsRequestDTO model)
        {
            var dmsOrdersList = new List<DmsOrder>();
            if (model.DmsOrderId != null)
            {
                var dmsOrder = await _dmsOrder.Table.Where(c => c.UserId == LoggedInUser()
                && c.Id == model.DmsOrderId).Include(c => c.OrderStatus).Include(c => c.DeliveryStatus).Include(c => c.DmsOrderItems)
                    .Include(c => c.DistributorSapAccount).Include(c => c.OrderType).Include(c => c.Plant).FirstOrDefaultAsync();

                if (dmsOrder == null)
                    throw new NotFoundException(ErrorCodes.DMS_ORDER_NOTFOUND.Key, ErrorCodes.DMS_ORDER_NOTFOUND.Value);

                dmsOrdersList.Add(dmsOrder);
            }
            if (model.DmsOrderGroupId != null)
            {
                //Check the groupId Here
                var dmsOrders = await _dmsOrder.Table.Where(c => c.UserId == LoggedInUser()
                    && c.DmsOrderGroupId == model.DmsOrderGroupId).Include(c => c.OrderStatus).Include(c => c.DeliveryStatus).Include(c => c.DmsOrderItems)
                    .Include(c => c.DistributorSapAccount).Include(c => c.OrderType).Include(c => c.Plant).ToListAsync();

                foreach (var ord in dmsOrders)
                {
                    dmsOrdersList.Add(ord);
                }
            }

            foreach (var orderItem in dmsOrdersList)
            {

                orderItem.DmsOrderItems = await _orderItem.Table.Where(x => x.OrderId == orderItem.Id).Include(x => x.Product).ToListAsync();



                var sapDatails = orderItem?.DistributorSapAccount;
                var oldOrder = await _dmsOrder.Table.Where(c => c.UserId == LoggedInUser()
                && c.Id == orderItem.Id).AsNoTracking().Include(c => c.OrderStatus).Include(c => c.DeliveryStatus)
                        .Include(c => c.DistributorSapAccount).Include(c => c.OrderType).Include(c => c.Plant).FirstOrDefaultAsync();
                //_orderLogger.LogInformation($"{"DMS OrderItem DB Response:-"}{" | "}{JsonConvert.SerializeObject(orderItem)}");

                if (orderItem == null)
                    throw new NotFoundException(ErrorCodes.DMS_ORDER_NOTFOUND.Key, ErrorCodes.DMS_ORDER_NOTFOUND.Value);

                orderItem.TruckSizeCode = model.TruckSizeCode;
                orderItem.DeliveryMethodCode = model.DeliveryMethodCode;
                orderItem.PlantCode = model.PlantCode;
                orderItem.DeliveryStateCode = model.DeliveryStateCode;
                orderItem.DeliveryCity = model.DeliveryCity;
                orderItem.DeliveryAddress = model.DeliveryAddress;
                orderItem.DeliveryCountryCode = model.DeliveryCountryCode;
                orderItem.DateModified = DateTime.Now;

                await _dmsOrder.UpdateAsync(orderItem);
                await _dmsOrder.CommitAsync(default);


                DmsOrderUpdatedMessage dmsOrderRefreshedMessage = new();
                dmsOrderRefreshedMessage.DmsOrderId = orderItem.Id;
                dmsOrderRefreshedMessage.DateModified = DateTime.Now;
                dmsOrderRefreshedMessage.ModifiedByUserId = orderItem.UserId;
                dmsOrderRefreshedMessage.UserId = orderItem.UserId;
                dmsOrderRefreshedMessage.CompanyCode = orderItem.CompanyCode;
                dmsOrderRefreshedMessage.CountryCode = orderItem.CountryCode;
                dmsOrderRefreshedMessage.OrderSapNumber = orderItem.OrderSapNumber;
                dmsOrderRefreshedMessage.DistributorSapAccount = new DistributorSapAccountResponse(sapDatails.DistributorSapAccountId, sapDatails.DistributorSapNumber, sapDatails.DistributorName);
                dmsOrderRefreshedMessage.EstimatedNetValue = orderItem.EstimatedNetValue;
                dmsOrderRefreshedMessage.OldOrderStatus = new OldOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage.NewOrderStatus = new NewOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage.OrderType = new NewOrderStatusResponse
                {
                    Code = _orderType.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Code,
                    Name = _orderType.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Name,
                };
                dmsOrderRefreshedMessage.OldtruckSizeCode = oldOrder.TruckSizeCode;
                dmsOrderRefreshedMessage.NewtruckSizeCode = orderItem.TruckSizeCode;
                dmsOrderRefreshedMessage.OldDeliveryMethodCode = orderItem.DeliveryMethodCode;
                dmsOrderRefreshedMessage.NewDeliveryMethodCode = orderItem.DeliveryMethodCode;
                dmsOrderRefreshedMessage.Oldplant = new Application.DTOs.Events.PlantResponse
                {
                    PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == oldOrder.PlantCode)?.Id),
                    Name = _plant.Table.FirstOrDefault(p => p.Code == oldOrder.PlantCode)?.Name,
                    Code = _plant.Table.FirstOrDefault(p => p.Code == oldOrder.PlantCode)?.Code,

                };
                dmsOrderRefreshedMessage.Newplant = new Application.DTOs.Events.PlantResponse
                {
                    PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Id),
                    Name = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Name,
                    Code = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Code,

                };
                dmsOrderRefreshedMessage.OldDeliveryDate = oldOrder.DeliveryDate == null ? new DateTime() : (DateTime)oldOrder.DeliveryDate;
                dmsOrderRefreshedMessage.NewDeliveryDate = orderItem.DeliveryDate == null ? new DateTime() : (DateTime)orderItem.DeliveryDate;
                dmsOrderRefreshedMessage.OldDeliveryAddress = oldOrder.DeliveryAddress;
                dmsOrderRefreshedMessage.NewDeliveryAddress = orderItem.DeliveryAddress;
                dmsOrderRefreshedMessage.OldDeliveryCity = oldOrder.DeliveryCity;
                dmsOrderRefreshedMessage.NewDeliveryCity = orderItem.DeliveryCity;
                dmsOrderRefreshedMessage.OldDeliveryStateCode = oldOrder.DeliveryStateCode;
                dmsOrderRefreshedMessage.NewDeliveryStateCode = orderItem.DeliveryStateCode;
                dmsOrderRefreshedMessage.OldDeliveryCountryCode = oldOrder.DeliveryCountryCode;
                dmsOrderRefreshedMessage.NewDeliveryCountryCode = orderItem.DeliveryCountryCode;


                await _messageBus.PublishTopicMessage(dmsOrderRefreshedMessage, EventMessages.ORDER_DMS_UPDATED);

            }

            var retval = new List<ViewDmsOrderDetailsDto>();

            foreach (var orderItem in dmsOrdersList)
            {
                //Get delivery Country with codeDeliveryMethodDto
                var deliveryMet = await _deliveryMethod.Table.FirstOrDefaultAsync(c => c.Code == orderItem.DeliveryMethodCode);

                //Get delivery Country with code
                var truckSize = await _truckSize.Table.FirstOrDefaultAsync(c => c.Code == orderItem.TruckSizeCode);

                var countryCode = await _sapService.GetCountries();
                var sort = countryCode?.Where(c => c.code == orderItem.CountryCode)?.FirstOrDefault();

                var stateCode = await _sapService.GetState(orderItem.CountryCode);
                var statesort = stateCode?.Where(c => c.code == orderItem.CountryCode)?.FirstOrDefault();

                var orderItems = orderItem.DmsOrderItems.Select(c => new DmsOrderItemDto
                {
                    UserId = c.UserId,
                    ProductId = c.ProductId,
                    OrderId = c.OrderId,
                    Id = c.Id,
                    OrderItemSapNumber = c.OrderItemSapNumber,
                    Quantity = c.Quantity,
                    SapPricePerUnit = c.SapPricePerUnit,
                    SapNetValue = c.SapNetValue,
                    salesUnitOfMeasure = new DeliveryStatusDto() { Name = "BAG", Code = "bag" },
                    product = new ProductDto
                    {
                        Id = c.Product.Id,
                        Name = c.Product.Name,
                        Price = c.Product.Price,
                        ProductSapNumber = c.Product.ProductSapNumber,
                        CompanyCode = c.Product.CompanyCode,
                        CountryCode = c.Product.CountryCode,
                        UnitOfMeasureCode = c.Product.UnitOfMeasureCode,
                        DateRefreshed = c.Product.DateRefreshed,
                    }
                });
                var orderItemVM = new ViewDmsOrderDetailsDto()
                {
                    Id = orderItem.Id,
                    OrderSapNumber = orderItem.OrderSapNumber,
                    ParentOrderSapNumber = orderItem.ParentOrderSapNumber,
                    DateCreated = orderItem.DateCreated,
                    CompanyCode = orderItem.CompanyCode,
                    OrderStatusId = orderItem.OrderStatusId,
                    OrderTypeId = orderItem.OrderTypeId,
                    ShoppingCartId = orderItem.ShoppingCartId,
                    TruckSizeCode = orderItem.TruckSizeCode,
                    CountryCode = orderItem.CountryCode,
                    PlantCode = orderItem.PlantCode,
                    IsAtc = orderItem.IsAtc,
                    OrderStatus = new OrderStatusDto { Code = orderItem.OrderStatus.Code, Name = orderItem.OrderStatus.Name },
                    OrderType = new OrderTypeDto { Code = orderItem.OrderType.Code, Name = orderItem.OrderType.Name },
                    truckSize = new TruckSizeDto { Code = truckSize?.Code, Name = truckSize?.Name },
                    deliveryMethod = new DeliveryMethodDto { Code = deliveryMet?.Code, Name = deliveryMet?.Name },
                    DistributorSapAccount = new DistributorSapAccountDto
                    {
                        AccountType = orderItem.DistributorSapAccount.AccountType,
                        CompanyCode = orderItem.DistributorSapAccount.CompanyCode,
                        CountryCode = orderItem.DistributorSapAccount.CountryCode,
                        UserId = orderItem.DistributorSapAccount.UserId,
                        DistributorSapAccountId = orderItem.DistributorSapAccount.DistributorSapAccountId,
                        DistributorSapNumber = orderItem.DistributorSapAccount.DistributorSapNumber,
                        DistributorName = orderItem.DistributorSapAccount.DistributorName,
                    },
                    DeliveryCountry = new CountryResponse { code = sort?.code, name = sort?.name },
                    DeliveryState = new DeliveryStatusDto { Code = statesort?.code, Name = statesort?.name },
                    EstimatedNetValue = orderItem.EstimatedNetValue,
                    OrderSapNetValue = orderItem.OrderSapNetValue,
                    SapVat = orderItem.SapVat,
                    SapFreightCharges = orderItem.SapFreightCharges,
                    DeliveryDate = orderItem.DeliveryDate,
                    DeliveryAddress = orderItem.DeliveryAddress,
                    DeliveryCity = orderItem.DeliveryCity,
                    OrderItems = orderItems
                };

                retval.Add(orderItemVM);
            }

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL, new { dmsOrders = retval });
        }
        public async Task<ApiResponse> GetMySapChildOrder(DmsOrderSapChildByQueryRequestDTO query)
        {
            var sapDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == query.DistributorSapAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(sapDetail)}");

            if (sapDetail == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            var sapChildOrders = await _sapService.GetChildOrder(sapDetail.CompanyCode, sapDetail.CountryCode, query.OrderSapNumber);
            if (sapChildOrders.Count > 0)
                sapChildOrders = sapChildOrders.Where(x => string.IsNullOrEmpty(x.DeliveryBlock.Code)).ToList();
            if (sapDetail.AccountType == "Bank Guarantee")
                sapChildOrders = sapChildOrders.Where(x => x.Status.Code == "C" || x.Status.Code == "F" || x.Status.Code == "G").ToList();

            if (query.Sort == OrderSortingEnum.DateAscending)
                sapChildOrders = sapChildOrders.OrderBy(x => x.DateCreated).ToList();
            else if (query.Sort == OrderSortingEnum.DateDescending)
                sapChildOrders = sapChildOrders.OrderByDescending(x => x.DateCreated).ToList();
            else if (query.Sort == OrderSortingEnum.ValueAscending)
                sapChildOrders = sapChildOrders.OrderBy(x => x.NetValue).ToList();
            else if (query.Sort == OrderSortingEnum.ValueDescending)
                sapChildOrders = sapChildOrders.OrderByDescending(x => x.NetValue).ToList();

            var totalCount = sapChildOrders.Count;
            var totalPages = NumberManipulator.PageCountConverter(totalCount, query.PageSize);

            _orderLogger.LogInformation($"{"Sap Child Orders Response:-"}{" | "}{JsonConvert.SerializeObject(sapChildOrders)}");


            var response = new PaginatedListVM<object>(sapChildOrders,
                new PaginationMetaData(query.PageIndex, query.PageSize, totalPages, totalCount));

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL, response);
        }
        public async Task<ApiResponse> SaveOrder(SaveOrderLaterRequest request, CancellationToken cancellationToken)
        {
            GetUserId();

            var dmsOrdersList = new List<DmsOrder>();
            if (request.DmsOrderId != null)
            {
                var saveDmsOrders = await _dmsOrder.Table.Where(sci => sci.Id == request.DmsOrderId
                                                   && sci.UserId == LoggedInUserId).FirstOrDefaultAsync();
                if (saveDmsOrders == null)
                    throw new NotFoundException(ErrorCodes.DMS_ORDER_NOTFOUND.Key, ErrorCodes.DMS_ORDER_NOTFOUND.Value);

                dmsOrdersList.Add(saveDmsOrders);
            }
            if (request.DmsOrderGroupId != null)
            {
                var dmsOrders = await _dmsOrder.Table.Where(sci => sci.DmsOrderGroupId == request.DmsOrderGroupId
                                 && sci.UserId == LoggedInUserId).ToListAsync();

                foreach (var ord in dmsOrders)
                {
                    dmsOrdersList.Add(ord);
                }
            }
            foreach (var order in dmsOrdersList)
            {
                var oldOrder = await _dmsOrder.Table.AsNoTracking().Include(x => x.DistributorSapAccount).Where(sci => sci.Id == order.Id
                                                                    && sci.UserId == LoggedInUserId).FirstOrDefaultAsync();
                var sapDetails = oldOrder.DistributorSapAccount;
                order.ModifiedByUserId = LoggedInUserId;
                order.DateModified = DateTime.UtcNow;
                order.TruckSizeCode = request.TruckSizeCode;
                order.DeliveryMethodCode = request.DeliveryMethodCode;
                order.OrderStatusId = (int)Application.Enums.OrderStatus.Saved;
                order.PlantCode = request.PlantCode;
                order.DeliveryAddress = request.DeliveryAddress;
                order.DeliveryCity = request.DeliveryCity;
                order.DeliveryStateCode = request.DeliveryStateCode;
                order.DeliveryCountryCode = request.DeliveryCountryCode;
                order.DeliveryMethodCode = request.DeliveryMethodCode;
                order.TruckSizeCode = request.TruckSizeCode;
                order.DeliveryDate = request.DeliveryDate;
                _dmsOrder.Commit();


                //Azure ServiceBus Orders.DmsOrder.Updated
                DmsOrderUpdatedMessage dmsOrderUpdatedMessage = new();


                dmsOrderUpdatedMessage.DmsOrderId = order.Id;
                dmsOrderUpdatedMessage.DateModified = DateTime.Now;
                dmsOrderUpdatedMessage.ModifiedByUserId = order.UserId;
                dmsOrderUpdatedMessage.UserId = order.UserId;
                dmsOrderUpdatedMessage.CompanyCode = order.CompanyCode;
                dmsOrderUpdatedMessage.CountryCode = order.CountryCode;
                dmsOrderUpdatedMessage.OrderSapNumber = order.OrderSapNumber;
                dmsOrderUpdatedMessage.DistributorSapAccount = new DistributorSapAccountResponse(sapDetails.DistributorSapAccountId, sapDetails.DistributorSapNumber, sapDetails.DistributorName);
                dmsOrderUpdatedMessage.EstimatedNetValue = order.EstimatedNetValue;
                dmsOrderUpdatedMessage.OldOrderStatus = new OldOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Name,
                };
                dmsOrderUpdatedMessage.NewOrderStatus = new NewOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == order.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == order.OrderStatusId)?.Name,
                };
                dmsOrderUpdatedMessage.OrderType = new NewOrderStatusResponse
                {
                    Code = _orderType.Table.FirstOrDefault(x => x.Id == order.OrderStatusId)?.Code,
                    Name = _orderType.Table.FirstOrDefault(x => x.Id == order.OrderStatusId)?.Name,
                };
                dmsOrderUpdatedMessage.OldtruckSizeCode = oldOrder.TruckSizeCode;
                dmsOrderUpdatedMessage.NewtruckSizeCode = order.TruckSizeCode;
                dmsOrderUpdatedMessage.OldDeliveryMethodCode = order.DeliveryMethodCode;
                dmsOrderUpdatedMessage.NewDeliveryMethodCode = order.DeliveryMethodCode;
                dmsOrderUpdatedMessage.Oldplant = new Application.DTOs.Events.PlantResponse
                {
                    PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Id),
                    Name = _plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Name,
                    Code = _plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Code,

                };
                dmsOrderUpdatedMessage.Newplant = new Application.DTOs.Events.PlantResponse
                {
                    PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Id),
                    Name = _plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Name,
                    Code = _plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Code,

                };
                dmsOrderUpdatedMessage.OldDeliveryDate = (DateTime)order.DeliveryDate;
                dmsOrderUpdatedMessage.NewDeliveryDate = (DateTime)order.DeliveryDate;
                dmsOrderUpdatedMessage.OldDeliveryAddress = oldOrder.DeliveryAddress;
                dmsOrderUpdatedMessage.NewDeliveryAddress = order.DeliveryAddress;
                dmsOrderUpdatedMessage.OldDeliveryCity = oldOrder.DeliveryCity;
                dmsOrderUpdatedMessage.NewDeliveryCity = order.DeliveryCity;
                dmsOrderUpdatedMessage.OldDeliveryStateCode = oldOrder.DeliveryStateCode;
                dmsOrderUpdatedMessage.NewDeliveryStateCode = order.DeliveryStateCode;
                dmsOrderUpdatedMessage.OldDeliveryCountryCode = oldOrder.DeliveryCountryCode;
                dmsOrderUpdatedMessage.NewDeliveryCountryCode = order.DeliveryCountryCode;
                foreach (var item in order.DmsOrderItems)
                {
                    dmsOrderUpdatedMessage.DmsOrderItems.Add(new OrderItemsResponse
                    {
                        DateCreated = item.DateCreated,
                        DmsOrderItemId = item.Id,
                        Product = new ProductResponse(item.ProductId, item.Product.Price, item.Product.Name),
                        Quantity = item.Quantity,
                        SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode
                    });
                }
                await _messageBus.PublishTopicMessage(dmsOrderUpdatedMessage, EventMessages.DMSORDER_UPDATED);

            }

            return ResponseHandler.SuccessResponse(SuccessMessages.DMSORDER_SAVED_SUCCESSFULLY);
        }
        public async Task<ApiResponse> SaveOrderV2(SaveOrderLaterRequestV2 request, CancellationToken cancellationToken)
        {
            GetUserId();

            var dmsOrdersList = new List<DmsOrder>();
            if (request.DmsOrderGroupId != null)
            {
                var dmsOrders = await _dmsOrder.Table.Where(sci => sci.DmsOrderGroupId == request.DmsOrderGroupId).ToListAsync();

                foreach (var ord in dmsOrders)
                {
                    dmsOrdersList.Add(ord);
                }
            }
            foreach (var order in dmsOrdersList)
            {
                var oldOrder = await _dmsOrder.Table.AsNoTracking().Include(x => x.DistributorSapAccount).Where(sci => sci.Id == order.Id
                                                                    && sci.UserId == LoggedInUserId).FirstOrDefaultAsync();
                var sapDetails = oldOrder.DistributorSapAccount;
                order.ModifiedByUserId = LoggedInUserId;
                order.DateModified = DateTime.UtcNow;
                order.TruckSizeCode = request.TruckSizeCode;
                order.OrderStatusId = (int)Application.Enums.OrderStatus.Saved;
                order.DeliveryAddress = request.DeliveryAddress;
                order.DeliveryCity = request.DeliveryCity;
                order.TruckSizeCode = request.TruckSizeCode;
                order.DeliveryDate = request.DeliveryDate;
                order.CustomerPaymentReference = request.CustomerPaymentReference;
                order.CustomerPaymentDate = request.CustomerPaymentDate;

                await _dmsOrder.UpdateAsync(order);
                _dmsOrder.Commit();


                //Azure ServiceBus Orders.DmsOrder.Updated
                DmsOrderUpdatedMessage dmsOrderUpdatedMessage = new();


                dmsOrderUpdatedMessage.DmsOrderId = order.Id;
                dmsOrderUpdatedMessage.DateModified = DateTime.Now;
                dmsOrderUpdatedMessage.ModifiedByUserId = order.UserId;
                dmsOrderUpdatedMessage.UserId = order.UserId;
                dmsOrderUpdatedMessage.CompanyCode = order.CompanyCode;
                dmsOrderUpdatedMessage.CountryCode = order.CountryCode;
                dmsOrderUpdatedMessage.OrderSapNumber = order.OrderSapNumber;
                dmsOrderUpdatedMessage.DistributorSapAccount = new DistributorSapAccountResponse(sapDetails.DistributorSapAccountId, sapDetails.DistributorSapNumber, sapDetails.DistributorName);
                dmsOrderUpdatedMessage.EstimatedNetValue = order.EstimatedNetValue;
                dmsOrderUpdatedMessage.OldOrderStatus = new OldOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Name,
                };
                dmsOrderUpdatedMessage.NewOrderStatus = new NewOrderStatusResponse
                {
                    Code = _orderStatus.Table.FirstOrDefault(x => x.Id == order.OrderStatusId)?.Code,
                    Name = _orderStatus.Table.FirstOrDefault(x => x.Id == order.OrderStatusId)?.Name,
                };
                dmsOrderUpdatedMessage.OrderType = new NewOrderStatusResponse
                {
                    Code = _orderType.Table.FirstOrDefault(x => x.Id == order.OrderStatusId)?.Code,
                    Name = _orderType.Table.FirstOrDefault(x => x.Id == order.OrderStatusId)?.Name,
                };
                dmsOrderUpdatedMessage.OldtruckSizeCode = oldOrder.TruckSizeCode;
                dmsOrderUpdatedMessage.NewtruckSizeCode = order.TruckSizeCode;
                dmsOrderUpdatedMessage.OldDeliveryMethodCode = order.DeliveryMethodCode;
                dmsOrderUpdatedMessage.NewDeliveryMethodCode = order.DeliveryMethodCode;
                dmsOrderUpdatedMessage.Oldplant = new Application.DTOs.Events.PlantResponse
                {
                    PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Id),
                    Name = _plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Name,
                    Code = _plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Code,

                };
                dmsOrderUpdatedMessage.Newplant = new Application.DTOs.Events.PlantResponse
                {
                    PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Id),
                    Name = _plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Name,
                    Code = _plant.Table.FirstOrDefault(p => p.Code == order.PlantCode)?.Code,

                };
                dmsOrderUpdatedMessage.OldDeliveryDate = (DateTime)order.DeliveryDate;
                dmsOrderUpdatedMessage.NewDeliveryDate = (DateTime)order.DeliveryDate;
                dmsOrderUpdatedMessage.OldDeliveryAddress = oldOrder.DeliveryAddress;
                dmsOrderUpdatedMessage.NewDeliveryAddress = order.DeliveryAddress;
                dmsOrderUpdatedMessage.OldDeliveryCity = oldOrder.DeliveryCity;
                dmsOrderUpdatedMessage.NewDeliveryCity = order.DeliveryCity;
                dmsOrderUpdatedMessage.OldDeliveryStateCode = oldOrder.DeliveryStateCode;
                dmsOrderUpdatedMessage.NewDeliveryStateCode = order.DeliveryStateCode;
                dmsOrderUpdatedMessage.OldDeliveryCountryCode = oldOrder.DeliveryCountryCode;
                dmsOrderUpdatedMessage.NewDeliveryCountryCode = order.DeliveryCountryCode;
                //dmsOrderUpdatedMessage.DmsOrderItems = (List<OrderItemsResponse>)saveDmsOrders.DmsOrderItems;
                foreach (var item in order.DmsOrderItems)
                {
                    dmsOrderUpdatedMessage.DmsOrderItems.Add(new OrderItemsResponse
                    {
                        DateCreated = item.DateCreated,
                        DmsOrderItemId = item.Id,
                        Product = new ProductResponse(item.ProductId, item.Product.Price, item.Product.Name),
                        Quantity = item.Quantity,
                        SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode
                    });
                }
                await _messageBus.PublishTopicMessage(dmsOrderUpdatedMessage, EventMessages.DMSORDER_UPDATED);

            }

            return ResponseHandler.SuccessResponse(SuccessMessages.DMSORDER_SAVED_SUCCESSFULLY);
        }
        public async Task<ApiResponse> StartCheck(StartCheckRequest request, CancellationToken cancellationToken)
        {
            GetUserId();
            var shoppingCart = await _shoppingCartRepository.Table.Include(x => x.ShoppingCartItems).Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active)
                                                 .FirstOrDefaultAsync();

            if (shoppingCart == null)
                throw new NotFoundException(ErrorCodes.SHOPPING_CART_NOTFOUND_FOR_CHECHOUT.Key, ErrorCodes.SHOPPING_CART_NOTFOUND_FOR_CHECHOUT.Value);
            //Error-O-07

            var dmsOrderGroup = new DmsOrderGroup
            {
                DateCreated = DateTime.Now,
                ShoppingCartId = shoppingCart.Id
            };
            await _orderGroup.AddAsync(dmsOrderGroup);
            await _orderGroup.CommitAsync(default);

            var cartItemsByDistributorAccountGroups = shoppingCart;
            int orderId = 0;
            foreach (var items in cartItemsByDistributorAccountGroups.ShoppingCartItems)
            {
                var productdetails = await _product.Table.Where(c => c.Id == items.ProductId).FirstOrDefaultAsync();
                var distributorSapAccount = await _distributorSapNo.Table.Where(dsa => dsa.DistributorSapAccountId == items.DistributorSapAccountId).FirstOrDefaultAsync();

                var sapWallet = _sapService.GetWallet(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, distributorSapAccount.DistributorSapNumber);

                if (sapWallet.Result == null)
                    throw new NotFoundException(ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Key, ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Value);
                //Error-O-08

                var estimatedNetValue = cartItemsByDistributorAccountGroups.ShoppingCartItems.Sum(e => e.Quantity * (decimal)productdetails.Price);

                if (Math.Abs(sapWallet.Result.AvailableBalance) < estimatedNetValue)
                    throw new NotFoundException(ErrorCodes.INSUFICIENT_FUNDS.Key, ErrorCodes.INSUFICIENT_FUNDS.Value);
                //Error-O-09
                int orderTypeId = 0;


                DateTime? validFrom = null;
                DateTime? validTo = null;
                if (distributorSapAccount.CompanyCode == DistributorSapAccountCompanyCode.DISTRIBUTOR_SAPACCOUNT_COMPANY_CODE)
                {
                    orderTypeId = (int)OrderTypesEnum.ZDOR;
                }
                else
                {
                    orderTypeId = (int)OrderTypesEnum.ZQT;
                    validFrom = DateTime.Now;
                    validTo = DateTime.Now.AddDays(_config.GetValue<int>("Settings:Orders:ExpiryDays"));
                }

                //foreach (var shoppingCartitem in items.ShoppingCart.ShoppingCartItems)
                //{
                //    var estimatedOrderValue = shoppingCartitem.Quantity * shoppingCartitem.Product.Price;

                DmsOrder createDmsOrder = new()
                {

                    ShoppingCartId = shoppingCart.Id,
                    UserId = LoggedInUserId,
                    CompanyCode = distributorSapAccount.CompanyCode,
                    CountryCode = distributorSapAccount.CountryCode,
                    DistributorSapAccountId = distributorSapAccount.DistributorSapAccountId,
                    EstimatedNetValue = estimatedNetValue,
                    IsAtc = false,
                    OrderStatusId = (byte)Application.Enums.OrderStatus.New,
                    OrderTypeId = (byte)orderTypeId,
                    ChannelCode = request.ChannelCode,
                    CreatedByUserId = LoggedInUserId,
                    DateCreated = DateTime.UtcNow,
                    ValidFrom = validFrom,
                    ValidTo = validTo,
                    DmsOrderGroupId = dmsOrderGroup.Id,
                };

                await _dmsOrder.AddAsync(createDmsOrder, cancellationToken);

                await _dmsOrder.CommitAsync(cancellationToken);
                orderId = createDmsOrder.Id;


                //Creating OrderItems
                DmsOrderItem createDmsOrderItems = new();
                createDmsOrderItems.OrderId = createDmsOrder.Id;
                createDmsOrderItems.UserId = LoggedInUserId;
                createDmsOrderItems.Quantity = items.Quantity;
                createDmsOrderItems.SalesUnitOfMeasureCode = items.UnitOfMeasureCode;
                createDmsOrderItems.ProductId = (short)items.ProductId;
                createDmsOrderItems.DateCreated = DateTime.UtcNow;
                await _orderItem.AddAsync(createDmsOrderItems, cancellationToken);
                await _orderItem.CommitAsync(cancellationToken);

            }
            shoppingCart.ShoppingCartStatusId = (int)ShoppingCartStatusEnum.CheckedOut;
            shoppingCart.DateModified = DateTime.Now;
            await _shoppingCartRepository.UpdateAsync(shoppingCart);
            await _shoppingCartRepository.CommitAsync(cancellationToken);

            //Service Bus Send Cart Updated Message

            return ResponseHandler.SuccessResponse(SuccessMessages.DMSORDER_CREATED_SUCCESSFULLY, new { dmsOrderGroupId = dmsOrderGroup.Id });

        }
        public async Task<ApiResponse> StartCheckV2(StartCheckRequest request, CancellationToken cancellationToken)
        {
            GetUserId();
            var shoppingCart = await _shoppingCartRepository.Table.Include(x => x.DistributorSapAccount).Include(x => x.ShoppingCartItems)
                .Where(sc => sc.UserId == LoggedInUserId && sc.ShoppingCartStatusId == (int)ShoppingCartStatusEnum.Active).FirstOrDefaultAsync();

            if (shoppingCart == null)
                throw new NotFoundException(ErrorCodes.SHOPPING_CART_NOTFOUND_FOR_CHECHOUT.Value, ErrorCodes.SHOPPING_CART_NOTFOUND_FOR_CHECHOUT.Key);
            //Error-O-07

            var dictionary = new Dictionary<int, dictionaryObject>();
            foreach (var items in shoppingCart.ShoppingCartItems)
            {
                var product = await _product.Table.FirstOrDefaultAsync(x => x.Id == items.ProductId);
                var distributor = await _distributorSapNo.Table.FirstOrDefaultAsync(x => x.DistributorSapAccountId == items.DistributorSapAccountId);
                // Get  Sap Order Estimate
                var estimateRequestDto = new EstimateRequest
                {
                    distributorNumber = distributor.DistributorSapNumber,
                    companyCode = distributor.CompanyCode,
                    countryCode = distributor.CountryCode,
                    deliveryMethodCode = items.DeliveryMethodCode,
                    plantCode = items.PlantCode,
                    quantity = items.Quantity.ToString(),
                    unitOfMeasureCode = items.UnitOfMeasureCode,
                    productId = product.ProductSapNumber,
                    deliveryStateCode = items.DeliveryStateCode,

                };
                var estimate = await _sapService.GetItemEstimate(estimateRequestDto);
                if (estimate.data != null)
                {
                    items.SapEstimatedOrderValue = (decimal?)estimate.data.sapEstimate.orderValue;
                    items.DateOfOrderEstimate = DateTime.UtcNow;
                    await _shoppingCartItemRepo.UpdateAsync(items);
                    await _shoppingCartItemRepo.CommitAsync(default);

                    //Add to the dictionary
                    dictionary.Add(items.Id, new dictionaryObject
                    {
                        orderValue = estimate.data.sapEstimate.orderValue,
                        freightCharges = estimate.data.sapEstimate.freightCharges,
                        Vat = estimate.data.sapEstimate.vat
                    });
                }
                else
                {
                    items.SapEstimatedOrderValue = 0;
                    items.DateOfOrderEstimate = DateTime.UtcNow;
                    await _shoppingCartItemRepo.UpdateAsync(items);
                    await _shoppingCartItemRepo.CommitAsync(default);
                    dictionary.Add(items.Id, new dictionaryObject
                    {
                        orderValue = 0,
                        freightCharges = 0,
                        Vat = 0
                    });
                }

            }

            foreach (var item in shoppingCart.ShoppingCartItems.GroupBy(x => x.DistributorSapAccountId))
            {
                var distributorSapAccount1 = await _distributorSapNo.Table.FirstOrDefaultAsync(x => x.DistributorSapAccountId == item.First().DistributorSapAccountId);

                var sapWallet1 = _sapService.GetWallet(distributorSapAccount1.CompanyCode, distributorSapAccount1.CountryCode, distributorSapAccount1.DistributorSapNumber);

                if (sapWallet1.Result == null)
                    throw new NotFoundException(ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Value, ErrorCodes.UNABLE_TO_VERIFY_FUNDS.Key);
                //Error-O-08

                var totalOrderEstimate = item.Sum(x => x.SapEstimatedOrderValue);
                if (sapWallet1.Result.AvailableBalance > 0) throw new NotFoundException(ErrorCodes.INSUFICIENT_FUNDS.Value, ErrorCodes.INSUFICIENT_FUNDS.Key);


                if (Math.Abs(sapWallet1.Result.AvailableBalance) < (decimal)(totalOrderEstimate))
                {
                    //Send Limit Exceeded Alert Message
                    var itemList = new List<ShoppingCartItemsDto>();
                    foreach (var it in item)
                    {
                        var prod = await _product.Table.FirstOrDefaultAsync(x => x.Id == it.ProductId);
                        itemList.Add(new ShoppingCartItemsDto
                        {
                            ShoppingCartItemId = it.Id,
                            DateCreated = it.DateCreated,
                            Quantity = it.Quantity,
                            UnitOfMeasureCode = it.UnitOfMeasureCode,
                            Product = new ProductMsgDto
                            {
                                Name = prod.Name,
                                ProductId = prod.Id,
                                Price = prod.Price,
                            },
                            SapEstimatedOrderValue = (decimal)it.SapEstimatedOrderValue

                        });
                    }
                    var shoppingCartExceededMsg = new ShoppingCartLimitExceededMessage
                    {
                        DistributorSapAccountId = distributorSapAccount1.DistributorSapAccountId,
                        CompanyCode = distributorSapAccount1.CompanyCode,
                        AvailableBalance = (sapWallet1.Result.AvailableBalance) * -1,
                        CountryCode = distributorSapAccount1.CountryCode,
                        DistributorName = distributorSapAccount1.DistributorName,
                        DistributorSapNumber = distributorSapAccount1.DistributorSapNumber,
                        ShoppingCartItems = itemList,
                    };
                    await _messageBus.PublishTopicMessage(shoppingCartExceededMsg, EventMessages.ORDERS_SHOPPINGCART_LIMITEXCEEDED);

                    throw new NotFoundException(ErrorCodes.INSUFICIENT_FUNDS.Value, ErrorCodes.INSUFICIENT_FUNDS.Key);
                }
                //Error-O-09
            }


            var dmsOrderGroup = new DmsOrderGroup
            {
                DateCreated = DateTime.Now,
                ShoppingCartId = shoppingCart.Id
            };
            await _orderGroup.AddAsync(dmsOrderGroup);
            await _orderGroup.CommitAsync(default);

            foreach (var items in shoppingCart.ShoppingCartItems)
            {
                var distributorSapAccount = await _distributorSapNo.Table.FirstOrDefaultAsync(x => x.DistributorSapAccountId == items.DistributorSapAccountId);
                var productdetails = await _product.Table.Where(c => c.Id == items.ProductId).FirstOrDefaultAsync();

                DateTime? validFrom = null;
                DateTime? validTo = null;

                validFrom = DateTime.Now;
                validTo = DateTime.Now.AddDays(_config.GetValue<int>("Settings:Orders:ExpiryDays"));

                DmsOrder createDmsOrder = new()
                {
                    ShoppingCartId = shoppingCart.Id,
                    UserId = LoggedInUserId,
                    CompanyCode = distributorSapAccount.CompanyCode,
                    CountryCode = distributorSapAccount.CountryCode,
                    DistributorSapAccountId = distributorSapAccount.DistributorSapAccountId,
                    PlantCode = shoppingCart.PlantCode,
                    DeliveryMethodCode = shoppingCart.DeliveryMethod,
                    EstimatedNetValue = (decimal)dictionary[items.Id].orderValue,
                    SapVat = (decimal)dictionary[items.Id].Vat,
                    OrderSapNetValue = (decimal)dictionary[items.Id].orderValue,
                    SapFreightCharges = (decimal)dictionary[items.Id].freightCharges,
                    IsAtc = false,
                    OrderStatusId = (byte)Application.Enums.OrderStatus.New,
                    OrderTypeId = 3,
                    ChannelCode = request.ChannelCode,
                    CreatedByUserId = LoggedInUserId,
                    DateCreated = DateTime.UtcNow,
                    ValidFrom = validFrom,
                    ValidTo = validTo,
                    DmsOrderGroupId = dmsOrderGroup.Id,
                    DeliveryStateCode = items.DeliveryStateCode,
                    DeliveryCountryCode = items.DeliveryCountryCode,
                };

                await _dmsOrder.AddAsync(createDmsOrder, cancellationToken);
                await _dmsOrder.CommitAsync(cancellationToken);

                shoppingCart.ShoppingCartStatusId = (int)ShoppingCartStatusEnum.CheckedOut;
                shoppingCart.DateModified = DateTime.Now;
                await _shoppingCartRepository.UpdateAsync(shoppingCart);
                await _shoppingCartRepository.CommitAsync(cancellationToken);

                //Creating OrderItems
                DmsOrderItem createDmsOrderItems = new();
                createDmsOrderItems.OrderId = createDmsOrder.Id;
                createDmsOrderItems.UserId = LoggedInUserId;
                createDmsOrderItems.Quantity = items.Quantity;
                createDmsOrderItems.SalesUnitOfMeasureCode = items.UnitOfMeasureCode;
                createDmsOrderItems.ProductId = (short)items.ProductId;
                createDmsOrderItems.DateCreated = items.DateCreated;
                await _orderItem.AddAsync(createDmsOrderItems, cancellationToken);
                await _orderItem.CommitAsync(cancellationToken);

                //Azure ServiceBus Orders.DmsOrder.Created
                DmsOrderCreatedMessage dmsOrderCreatedMessage = new();
                dmsOrderCreatedMessage.UserId = LoggedInUserId;
                dmsOrderCreatedMessage.DmsOrderId = createDmsOrder.Id;
                dmsOrderCreatedMessage.CompanyCode = distributorSapAccount.CompanyCode;
                dmsOrderCreatedMessage.CountryCode = distributorSapAccount.CountryCode;
                dmsOrderCreatedMessage.OrderStatus = new Vm.ViewModels.Responses.Status
                {
                    name = "New",
                    code = "New"
                };
                dmsOrderCreatedMessage.OrderType = new Vm.ViewModels.Responses.Status
                {
                    name = "ZDOR",
                    code = "ZDOR",
                };
                dmsOrderCreatedMessage.DistributorSapAccount = new DistributorSapAccountResponse(distributorSapAccount.DistributorSapAccountId, distributorSapAccount.DistributorSapNumber, distributorSapAccount.DistributorName);
                dmsOrderCreatedMessage.CreatedByUserId = LoggedInUserId;
                dmsOrderCreatedMessage.EstimatedNetValue = (decimal)createDmsOrder.EstimatedNetValue;
                var orderItemsList = new List<OrderItemsResponse>();
                orderItemsList.Add(new OrderItemsResponse
                {
                    DateCreated = DateTime.UtcNow,
                    DmsOrderItemId = 0,
                    Quantity = createDmsOrderItems.Quantity,
                    SalesUnitOfMeasureCode = createDmsOrderItems.SalesUnitOfMeasureCode,
                    Product = new ProductResponse(productdetails.Id, productdetails.Price, productdetails.Name)

                });

                dmsOrderCreatedMessage.DmsOrderItems = orderItemsList;
                await _messageBus.PublishTopicMessage(dmsOrderCreatedMessage, EventMessages.ORDER_DMS_CREATED);
            }
            shoppingCart.ShoppingCartStatusId = (int)ShoppingCartStatusEnum.CheckedOut;
            shoppingCart.DateModified = DateTime.Now;
            await _shoppingCartRepository.CommitAsync(cancellationToken);

            //Service Bus Send Cart Updated Message

            return ResponseHandler.SuccessResponse(SuccessMessages.DMSORDER_INITIALIZED_SUCCESSFULLY, new { dmsOrderGroupId = dmsOrderGroup.Id, deliveryType = shoppingCart.DeliveryMethod });

        }
        public async Task<ApiResponse> GetMyRecentDmsOrder(CancellationToken cancellationToken)
        {
            //New changes
            _orderLogger.LogInformation($"{"About to retrieve Recent DMS Orders"}{" | "}{LoggedInUser}{" | "}{" | "}{DateTime.Now}");

            var redisDmsOrders = new DmsOrder();
            if (redisDmsOrders.Id == 0)
            {

                redisDmsOrders = _dmsOrder.Table.Where(c => c.UserId == LoggedInUser() && c.DeliveryAddress != null && c.IsAtc == false)
                    .Include(c => c.OrderStatus).Include(c => c.OrderType)
                    .Include(c => c.DistributorSapAccount).Include(c => c.Plant)
                    .ToListAsync().Result.OrderByDescending(c => c.DateCreated).FirstOrDefault();

                if (redisDmsOrders == null)
                    return Application.DTOs.APIDataFormatters.ResponseHandler.SuccessResponse("No Record Found...");

                redisDmsOrders.DmsOrderItems = await _orderItem.Table.Where(c => c.OrderId == redisDmsOrders.Id)
                    .Include(c => c.Product).ToListAsync();
            }

            if (redisDmsOrders?.UserId != LoggedInUser())
                throw new NotFoundException(ErrorCodes.INVALID_ROUTE.Key, ErrorCodes.INVALID_ROUTE.Value);



            //Get delivery Country with codeDeliveryMethodDto
            var deliveryMet = await _deliveryMethod.Table.FirstOrDefaultAsync(c => c.Code == redisDmsOrders.DeliveryMethodCode);

            //Get delivery Country with code
            var truckSize = await _truckSize.Table.FirstOrDefaultAsync(c => c.Code == redisDmsOrders.TruckSizeCode);

            var orderItems = redisDmsOrders.DmsOrderItems.Select(c => new DmsOrderItemDto
            {
                Id = c.Id,
                OrderItemSapNumber = c.OrderItemSapNumber,
                Quantity = c.Quantity,
                SapPricePerUnit = c.SapPricePerUnit,
                SapNetValue = c.SapNetValue,
                product = new ProductDto
                {
                    Id = c.Product.Id,
                    Name = c.Product.Name,
                    Price = c.Product.Price,
                    CountryCode = c.Product.CountryCode,
                    ProductSapNumber = c.Product.ProductSapNumber,
                    CompanyCode = c.Product.CompanyCode,
                    UnitOfMeasureCode = c.Product.UnitOfMeasureCode,
                    DateRefreshed = c.Product.DateRefreshed,
                },
                OrderId = c.OrderId,
                ProductId = c.ProductId,
                SalesUnitOfMeasureCode = c.SalesUnitOfMeasureCode
            });
            var orderItem = new ViewDmsOrderDetailsDto()
            {
                Id = redisDmsOrders.Id,
                OrderSapNumber = redisDmsOrders.OrderSapNumber,
                ParentOrderSapNumber = redisDmsOrders.ParentOrderSapNumber,
                DateCreated = redisDmsOrders.DateCreated.ConvertToLocal(),
                CompanyCode = redisDmsOrders.CompanyCode,
                CountryCode = redisDmsOrders.CountryCode,
                DistributorSapAccountId = redisDmsOrders.DistributorSapAccountId,
                OrderStatusId = redisDmsOrders.OrderStatusId,
                OrderTypeId = redisDmsOrders.OrderTypeId,
                PlantCode = redisDmsOrders.PlantCode,
                TruckSizeCode = redisDmsOrders.TruckSizeCode,
                IsAtc = redisDmsOrders.IsAtc,
                OrderStatus = new OrderStatusDto { Code = redisDmsOrders.OrderStatus.Code, Name = redisDmsOrders.OrderStatus.Name },
                OrderType = new OrderTypeDto { Code = redisDmsOrders.OrderType.Code, Name = redisDmsOrders.OrderType.Name },
                truckSize = new TruckSizeDto { Code = truckSize?.Code, Name = truckSize?.Name },
                deliveryMethod = new DeliveryMethodDto { Code = deliveryMet?.Code, Name = deliveryMet?.Name },
                DistributorSapAccount = new DistributorSapAccountDto
                {
                    DistributorSapAccountId = redisDmsOrders.DistributorSapAccount.DistributorSapAccountId,
                    DistributorSapNumber = redisDmsOrders.DistributorSapAccount.DistributorSapNumber,
                    DistributorName = redisDmsOrders.DistributorSapAccount.DistributorName,
                    CompanyCode = redisDmsOrders.DistributorSapAccount.CompanyCode,
                    CountryCode = redisDmsOrders.DistributorSapAccount.CountryCode,
                    AccountType = redisDmsOrders.DistributorSapAccount.AccountType
                },
                EstimatedNetValue = redisDmsOrders.EstimatedNetValue,
                OrderSapNetValue = redisDmsOrders.OrderSapNetValue,
                SapVat = redisDmsOrders.SapVat,
                SapFreightCharges = redisDmsOrders.SapFreightCharges,
                DeliveryDate = redisDmsOrders.DeliveryDate,
                DeliveryAddress = redisDmsOrders.DeliveryAddress,
                DeliveryCity = redisDmsOrders.DeliveryCity,
                OrderItems = orderItems
            };
            return Application.DTOs.APIDataFormatters.ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL, new { dmsOrder = orderItem });
        }
        public async Task<ApiResponse> ViewDmsOrderDetails(int orderId, CancellationToken cancellationToken)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order By The Id"}{" | "}{LoggedInUser()}{" | "}{orderId}{" | "}{DateTime.Now}");

            var redisDmsOrderDetail = new DmsOrder();
            if (redisDmsOrderDetail.Id == 0)
            {

                redisDmsOrderDetail = await _dmsOrder.Table.Include(c => c.OrderStatus)
                    .Include(c => c.Plant).Include(c => c.DistributorSapAccount).Include(c => c.OrderType)
                    .FirstOrDefaultAsync(c => c.Id == orderId);

                if (redisDmsOrderDetail != null)
                {
                    redisDmsOrderDetail.DmsOrderItems = await _orderItem.Table.Where(c => c.OrderId == orderId)
                    .Include(c => c.Product).ToListAsync();


                }

            }


            //Get delivery Country with codeDeliveryMethodDto
            var deliveryMet = await _deliveryMethod.Table.FirstOrDefaultAsync(c => c.Code == redisDmsOrderDetail.DeliveryMethodCode);

            //Get delivery Country with code
            var truckSize = await _truckSize.Table.FirstOrDefaultAsync(c => c.Code == redisDmsOrderDetail.TruckSizeCode);

            var countryCode = await _sapService.GetCountries();
            var sort = countryCode?.Where(c => c.code == redisDmsOrderDetail.CountryCode)?.FirstOrDefault();

            var stateCode = await _sapService.GetState(redisDmsOrderDetail.CountryCode);
            var statesort = stateCode?.Where(c => c.code == redisDmsOrderDetail.DeliveryStateCode)?.FirstOrDefault();

            var orderItems = redisDmsOrderDetail.DmsOrderItems.Select(c => new DmsOrderItemDto
            {
                Id = c.Id,
                OrderId = c.OrderId,
                UserId = c.UserId,
                OrderItemSapNumber = c.OrderItemSapNumber,
                Quantity = c.Quantity,
                SapPricePerUnit = c.SapPricePerUnit,
                SapNetValue = c.SapNetValue,
                product = new ProductDto
                {
                    Id = c.Product.Id,
                    Name = c.Product.Name,
                    Price = c.Product.Price,
                    ProductSapNumber = c.Product.ProductSapNumber,
                    CompanyCode = c.Product.CompanyCode,
                    CountryCode = c.Product.CountryCode,
                    UnitOfMeasureCode = c.Product.UnitOfMeasureCode,
                    DateRefreshed = c.Product.DateRefreshed,
                },
                SalesUnitOfMeasureCode = c.SalesUnitOfMeasureCode,
                salesUnitOfMeasure = new DeliveryStatusDto { Code = c.SalesUnitOfMeasureCode, Name = c.SalesUnitOfMeasureCode },
            });
            var orderItemVM = new ViewDmsOrderDetailsDto();
            orderItemVM.DmsOrderGroupId = redisDmsOrderDetail.DmsOrderGroupId;
            orderItemVM.Id = redisDmsOrderDetail.Id;
            orderItemVM.OrderSapNumber = redisDmsOrderDetail.OrderSapNumber;
            orderItemVM.ParentOrderSapNumber = redisDmsOrderDetail.ParentOrderSapNumber;
            orderItemVM.DateCreated = redisDmsOrderDetail.DateCreated.ConvertToLocal();
            orderItemVM.CompanyCode = redisDmsOrderDetail.CompanyCode;
            orderItemVM.CountryCode = redisDmsOrderDetail.CountryCode;
            orderItemVM.DistributorSapAccountId = redisDmsOrderDetail.DistributorSapAccountId;
            orderItemVM.OrderStatusId = redisDmsOrderDetail.OrderStatusId;
            orderItemVM.ShoppingCartId = redisDmsOrderDetail.ShoppingCartId;
            orderItemVM.TruckSizeCode = redisDmsOrderDetail.TruckSizeCode;
            orderItemVM.PlantCode = redisDmsOrderDetail.PlantCode;
            orderItemVM.OrderTypeId = redisDmsOrderDetail.OrderTypeId;
            orderItemVM.IsAtc = redisDmsOrderDetail.IsAtc;
            orderItemVM.OrderStatus = new OrderStatusDto { Code = redisDmsOrderDetail.OrderStatus.Code, Name = redisDmsOrderDetail.OrderStatus.Name };
            orderItemVM.OrderType = new OrderTypeDto { Code = redisDmsOrderDetail.OrderType.Code, Name = redisDmsOrderDetail.OrderType.Name };
            orderItemVM.truckSize = new TruckSizeDto { Code = truckSize?.Code, Name = truckSize?.Name };
            orderItemVM.deliveryMethod = new DeliveryMethodDto { Code = deliveryMet?.Code, Name = deliveryMet?.Name };
            orderItemVM.DistributorSapAccount = new DistributorSapAccountDto
            {
                UserId = redisDmsOrderDetail.DistributorSapAccount.UserId,
                DistributorSapAccountId = redisDmsOrderDetail.DistributorSapAccount.DistributorSapAccountId,
                DistributorSapNumber = redisDmsOrderDetail.DistributorSapAccount.DistributorSapNumber,
                DistributorName = redisDmsOrderDetail.DistributorSapAccount.DistributorName,
                CompanyCode = redisDmsOrderDetail.DistributorSapAccount.CompanyCode,
                CountryCode = redisDmsOrderDetail.DistributorSapAccount.CountryCode,
                DateRefreshed = redisDmsOrderDetail.DistributorSapAccount.DateRefreshed,
                AccountType = redisDmsOrderDetail.DistributorSapAccount.AccountType
            };
            orderItemVM.DeliveryCountry = new CountryResponse { code = sort?.code, name = sort?.name };
            orderItemVM.DeliveryState = new DeliveryStatusDto { Code = statesort?.code, Name = statesort?.name };
            orderItemVM.EstimatedNetValue = redisDmsOrderDetail.EstimatedNetValue;
            orderItemVM.OrderSapNetValue = redisDmsOrderDetail.OrderSapNetValue;
            orderItemVM.SapVat = redisDmsOrderDetail.SapVat;
            orderItemVM.SapFreightCharges = redisDmsOrderDetail.SapFreightCharges;
            orderItemVM.DeliveryDate = redisDmsOrderDetail.DeliveryDate.Value.ConvertToLocal();
            orderItemVM.DeliveryAddress = redisDmsOrderDetail.DeliveryAddress;
            orderItemVM.DeliveryCity = redisDmsOrderDetail.DeliveryCity;
            orderItemVM.OrderItems = orderItems;
            orderItemVM.Plant = new DeliveryStatusDto { Name = _plant.Table.FirstOrDefault(x => x.Code == redisDmsOrderDetail.PlantCode && x.CompanyCode == redisDmsOrderDetail.CompanyCode)?.Name, Code = redisDmsOrderDetail.PlantCode };


            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL, orderItemVM);
        }
        public async Task<ApiResponse> GetOrderHistory(int OrderId, CancellationToken cancellationToken)
        {
            GetUserId();
            var dmsOrdersChangeLog = await _dmsChangeLogOrder.Table.Where(x => x.OrderId == OrderId && x.OldOrderStatusId != x.NewOrderStatusId).ToListAsync();
            if (dmsOrdersChangeLog == null)
                return ResponseHandler.SuccessResponse("No Record Found");

            var logResponse = new List<ChangeLogResponse>();
            foreach (var log in dmsOrdersChangeLog)
            {
                var oldStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == log.OldOrderStatusId);
                var newStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == log.NewOrderStatusId);
                logResponse.Add(new ChangeLogResponse
                {
                    dmsOrderId = OrderId,
                    ChangeType = log.ChangeType,
                    NewDateModified = log.NewDateModified.Value.ConvertToLocal(),
                    OldOrderStatus = new Application.ViewModels.Responses.OrderStatus { Code = oldStatus.Code, Name = oldStatus.Name },
                    NewOrderStatus = new Application.ViewModels.Responses.OrderStatus { Code = newStatus.Code, Name = newStatus.Name },
                });
            }
            _orderLogger.LogInformation($"{"DMS Order Change Log DB Response:-"}{" | "}{JsonConvert.SerializeObject(logResponse)}");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_DMS_CHANGE_LOG, new { dmsOrderChangeLog = logResponse });
        }
        public async Task<ApiResponse> GetOrderHistory(string OrderSapNumber, CancellationToken cancellationToken)
        {
            GetUserId();

            var dmsOrder = await _dmsOrder.Table.FirstOrDefaultAsync(x => x.OrderSapNumber == OrderSapNumber);
            if (dmsOrder == null)
                throw new NotFoundException(ErrorCodes.NOT_DMS_ORIGIN.Value, ErrorCodes.NOT_DMS_ORIGIN.Key);
            //return Error


            var dmsOrdersChangeLog = await _dmsChangeLogOrder.Table.Where(x => x.OrderId == dmsOrder.Id && x.OldOrderStatusId != x.NewOrderStatusId).ToListAsync();
            if (dmsOrdersChangeLog == null)
                return ResponseHandler.SuccessResponse("No Record Found");

            var logResponse = new List<ChangeLogResponse>();
            foreach (var log in dmsOrdersChangeLog)
            {
                var oldStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == log.OldOrderStatusId);
                var newStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == log.NewOrderStatusId);
                logResponse.Add(new ChangeLogResponse
                {
                    dmsOrderId = (int)log.OrderId,
                    ChangeType = log.ChangeType,
                    NewDateModified = log.NewDateModified.Value.ConvertToLocal(),
                    OldOrderStatus = new Application.ViewModels.Responses.OrderStatus { Code = oldStatus.Code, Name = oldStatus.Name },
                    NewOrderStatus = new Application.ViewModels.Responses.OrderStatus { Code = newStatus.Code, Name = newStatus.Name },


                });
            }
            _orderLogger.LogInformation($"{"DMS Order Change Log DB Response:-"}{" | "}{JsonConvert.SerializeObject(logResponse)}");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_DMS_CHANGE_LOG, new { dmsOrderChangeLog = logResponse });
        }
        public async Task<ApiResponse> ScheduleATC(ScheduleATCDeliveryRequest request, CancellationToken cancellationToken)
        {
            GetUserId();

            var distributorSapAccount = await _distributorSapNo.Table.Where(dsa => dsa.DistributorSapAccountId == request.DistributorSapAccountId
                                                                         && dsa.UserId == LoggedInUserId).FirstOrDefaultAsync();

            if (distributorSapAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);
            //Error-O-01

            Shared.ExternalServices.DTOs.SapOrder1 sapOrder = null;
            if (sapOrder == null)
            {
                sapOrder = await _sapService.GetOrderDetails(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, request.OrderSapNumber);
            }

            if (sapOrder.distirbutorNumber.ToString() != distributorSapAccount.DistributorSapNumber)
                throw new NotFoundException(ErrorCodes.DMS_ORDER_NOTFOUND.Value, ErrorCodes.DMS_ORDER_NOTFOUND.Key);
            //Error - O - 02

            if (!sapOrder.orderType.Code.StartsWith('Y') && sapOrder.orderType.Code.ToUpper() != "ZDBO" && sapOrder.orderType.Code.ToUpper() != "ZDCI" && sapOrder.orderType.Code.ToUpper() != "ZSBO" && sapOrder.orderType.Code.ToUpper() != "ZSCI"
                && sapOrder.orderType.Code.ToUpper() != "ZXDJ" && sapOrder.orderType.Code.ToUpper() != "ZPBO" && sapOrder.orderType.Code.ToUpper() != "ZXSJ")
                throw new NotFoundException(ErrorCodes.DELIVERY_ONLY_ALLOW_ON_ATC.Value, ErrorCodes.DELIVERY_ONLY_ALLOW_ON_ATC.Key);
            //Error-O-20

            if (!string.IsNullOrEmpty(sapOrder.deliveryBlock.code))
                throw new NotFoundException(ErrorCodes.OPERATION_NOT_ALLOW_ON_ATC.Value, ErrorCodes.OPERATION_NOT_ALLOW_ON_ATC.Key);

            if (sapOrder.creditBlock.code == "B")
                throw new NotFoundException(ErrorCodes.OPERATION_NOT_ALLOW_ON_ATC.Value, ErrorCodes.OPERATION_NOT_ALLOW_ON_ATC.Key);

            //Error-O-21

            if (sapOrder.Status.Code != "B" && sapOrder.Status.Code != "A")
                throw new NotFoundException(ErrorCodes.CANNOT_SCHEDULE_COMPLETED_ORDERS.Value, ErrorCodes.CANNOT_SCHEDULE_COMPLETED_ORDERS.Key);


            var dmsOrder = await _dmsOrder.Table.Where(x => x.OrderSapNumber == request.OrderSapNumber).FirstOrDefaultAsync();
            if (dmsOrder == null)
            {
                var shoppingCart = await _shoppingCartRepository.Table.FirstOrDefaultAsync();
                dmsOrder = new()
                {
                    //ShoppingCartId = 443,
                    ShoppingCartId = shoppingCart.Id,
                    OrderSapNumber = sapOrder.Id.ToString(),
                    ParentOrderSapNumber = sapOrder.parentId.ToString(),
                    IsAtc = true,
                    OrderStatusId = (byte)Application.Enums.OrderStatus.PendingOtp,
                    DeliveryDate = request.DeliveryDate,
                    TruckSizeCode = request.TruckSizeCode,
                    DeliveryStateCode = request.DeliveryStateCode,
                    PlantCode = request.PlantCode,
                    DeliveryAddress = request.DeliveryAddress,
                    DeliveryCity = request.DeliveryCity,
                    DeliveryCountryCode = request.DeliveryCountryCode,
                    ChannelCode = request.ChannelCode,
                    DateSubmittedOnDms = DateTime.UtcNow,
                    DeliveryMethodCode = sapOrder.deliveryMethod.code,
                    UserId = LoggedInUserId,
                    CompanyCode = distributorSapAccount.CompanyCode,
                    CountryCode = distributorSapAccount.CountryCode,
                    DistributorSapAccountId = distributorSapAccount.DistributorSapAccountId,
                    EstimatedNetValue = decimal.Parse(sapOrder.netValue),
                    OrderTypeId = _orderType.Table.Where(c => c.Code == sapOrder.orderType.Code).FirstOrDefault().Id,
                    CreatedByUserId = LoggedInUserId,
                    DateCreated = DateTime.UtcNow,
                    ModifiedByUserId = LoggedInUserId,
                    SapFreightCharges = decimal.TryParse(sapOrder.freightCharges, out decimal result) ? decimal.Parse(sapOrder.freightCharges) : null,
                    SapVat = decimal.TryParse(sapOrder.vat, out decimal res) ? decimal.Parse(sapOrder.vat) : null,
                    OrderSapNetValue = decimal.Parse(sapOrder.netValue),


                };

                await _dmsOrder.AddAsync(dmsOrder, cancellationToken);

                await _dmsOrder.CommitAsync(cancellationToken);

                foreach (var dmOrderitem in sapOrder.orderItems)
                {

                    var product = await _product.Table.FirstOrDefaultAsync(x => x.ProductSapNumber == dmOrderitem.product.productId.ToString());

                    DmsOrderItem createDmsOrderItems = new();
                    createDmsOrderItems.OrderItemSapNumber = dmOrderitem.Id.ToString();
                    createDmsOrderItems.OrderId = dmsOrder.Id;
                    createDmsOrderItems.UserId = LoggedInUserId;
                    createDmsOrderItems.Quantity = decimal.Parse(dmOrderitem.orderQuantity);
                    createDmsOrderItems.SalesUnitOfMeasureCode = dmOrderitem.salesUnitOfMeasure.code;
                    createDmsOrderItems.ProductId = product.Id;
                    createDmsOrderItems.DateCreated = DateTime.UtcNow;

                    await _orderItem.AddAsync(createDmsOrderItems, cancellationToken);
                    await _orderItem.CommitAsync(cancellationToken);
                }

                //Azure ServiceBus Orders.DmsOrder.Created
                DmsOrderCreatedMessage dmsOrderCreatedMessage = new();
                dmsOrderCreatedMessage.UserId = LoggedInUserId;
                dmsOrderCreatedMessage.DmsOrderId = (int)sapOrder.Id;
                dmsOrderCreatedMessage.CompanyCode = distributorSapAccount.CompanyCode;
                dmsOrderCreatedMessage.CountryCode = distributorSapAccount.CountryCode;
                dmsOrderCreatedMessage.OrderStatus = new Vm.ViewModels.Responses.Status
                {
                    name = _orderStatus.Table.FirstOrDefault(ot => ot.Id == dmsOrder.OrderStatusId).Name,
                    code = _orderStatus.Table.FirstOrDefault(ot => ot.Id == dmsOrder.OrderStatusId).Code
                };
                dmsOrderCreatedMessage.OrderType = new Vm.ViewModels.Responses.Status
                {
                    name = _orderType.Table.FirstOrDefault(ot => ot.Code == sapOrder.orderType.Code).Name,
                    code = _orderType.Table.FirstOrDefault(ot => ot.Code == sapOrder.orderType.Code).Code,
                };
                dmsOrderCreatedMessage.DistributorSapAccount = new DistributorSapAccountResponse(distributorSapAccount.DistributorSapAccountId, distributorSapAccount.DistributorSapNumber, distributorSapAccount.DistributorName);
                dmsOrderCreatedMessage.CreatedByUserId = LoggedInUserId;
                dmsOrderCreatedMessage.EstimatedNetValue = decimal.TryParse(sapOrder.netValue, out decimal netval) ? netval : 0;
                var orderItemsList = new List<OrderItemsResponse>();
                foreach (var item in sapOrder.orderItems)
                {
                    orderItemsList.Add(new OrderItemsResponse
                    {
                        DateCreated = DateTime.UtcNow,
                        DmsOrderItemId = 0,
                        Quantity = decimal.Parse(item.orderQuantity),
                        SalesUnitOfMeasureCode = item.salesUnitOfMeasure.code,
                        Product = new ProductResponse(0, decimal.Parse(item.pricePerUnit), item.product.name)

                    });
                }
                dmsOrderCreatedMessage.DmsOrderItems = orderItemsList;
                await _messageBus.PublishTopicMessage(dmsOrderCreatedMessage, EventMessages.ORDER_DMS_CREATED);
            }
            else
            {
                dmsOrder.OrderStatusId = (byte)Application.Enums.OrderStatus.PendingOtp;
                dmsOrder.DeliveryDate = request.DeliveryDate;
                dmsOrder.TruckSizeCode = request.TruckSizeCode;
                dmsOrder.DeliveryMethodCode = request.DeliveryMethodCode;
                dmsOrder.PlantCode = request.PlantCode;
                dmsOrder.DeliveryAddress = request.DeliveryAddress;
                dmsOrder.DeliveryCity = request.DeliveryCity;
                dmsOrder.DeliveryStateCode = request.DeliveryStateCode;
                dmsOrder.CountryCode = request.DeliveryCountryCode;
                dmsOrder.ChannelCode = request.ChannelCode;
                await _dmsOrder.UpdateAsync(dmsOrder);
                await _dmsOrder.CommitAsync(default);
            }
            GetEmail();
            GetPhone();
            var otpItem = await _otpService.GenerateOtp(Email, dmsOrder.Id, null, false, PhoneNumber, LoggedInUser()); ;

            OrdersOtpGeneratedMessage otpVM = new()
            {
                DateCreated = DateTime.UtcNow,
                DateExpiry = otpItem.DateExpiry,
                EmailAddress = Email,
                PhoneNumber = PhoneNumber,
                OtpCode = otpItem.Code,
                OtpId = otpItem.Id,
            };

            await _messageBus.PublishTopicMessage(otpVM, EventMessages.ORDER_OTP_GENERATED);

            return ResponseHandler.SuccessResponse(SuccessMessages.OTP_GENERATED_SUCCESSFULLY, new { otp = new { otpid = otpItem.Id } });
        }
        public async Task<ApiResponse> AutoSubmitOrder(CancellationToken cancellationToken)
        {
            _orderLogger.LogInformation($"{"About to Submit DMS Order"}{" | "}{DateTime.Now}");
            var dmsOrderslist = _dmsOrder.Table.Where(u => u.OrderStatusId == (byte)Application.Enums.OrderStatus.ProcessingSubmission)
                .Include(c => c.DmsOrderItems).Include(x => x.OrderType).Include(c => c.DistributorSapAccount).ToList();

            var failedStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == (byte)Application.Enums.OrderStatus.Failed);
            var submittedStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == (byte)Application.Enums.OrderStatus.Submitted);

            foreach (var dmsitems in dmsOrderslist)
            {
                try
                {
                    if (dmsitems.DateCreated.AddMinutes(CacheKeys.MaximumSapSubmissionWIndowHours) < DateTime.UtcNow)
                    {
                        dmsitems.OrderStatusId = (byte)Application.Enums.OrderStatus.Failed;
                        dmsitems.DateModified = DateTime.UtcNow;
                        await _dmsOrder.UpdateAsync(dmsitems);
                        _dmsOrder.Commit();

                        var orderPlant = await _plant.Table.FirstOrDefaultAsync(p => p.Code == dmsitems.PlantCode);
                        var itemList = new List<FailSubmissionOrderItemsResponse>();
                        var item = dmsitems.DmsOrderItems.First();
                        var product = await _product.Table.FirstOrDefaultAsync(x => x.Id == item.ProductId);
                        itemList.Add(new FailSubmissionOrderItemsResponse
                        {
                            DmsOrderItemId = item.Id,
                            Quantity = item.Quantity,
                            SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode,
                            Product = new ProductResponse(product.Id, product.Price, product.Name),
                        });

                        DmsOrderSubmissionFailedMessage dmsOrderFailedSubmissionMessage = new()
                        {
                            UserId = dmsitems.UserId,
                            DmsOrderId = dmsitems.Id,
                            CompanyCode = dmsitems.CompanyCode,
                            CountryCode = dmsitems.CountryCode,
                            DateSubmittedtoDMS = dmsitems.DateCreated,
                            DeliveryDate = (DateTime)dmsitems.DeliveryDate,
                            DeliveryAddress = dmsitems.DeliveryAddress,
                            DeliveryCity = dmsitems.DeliveryCity,
                            DeliveryStateCode = dmsitems.DeliveryStateCode,
                            DeliveryCountryCode = dmsitems.DeliveryCountryCode,
                            DeliveryMethodCode = dmsitems.DeliveryMethodCode,
                            NumberOfSubmissionAttempts = (int)dmsitems.NumberOfSapsubmissionAttempts,
                            TruckSizeCode = dmsitems.TruckSizeCode,
                            EstimatedNetValue = (decimal)dmsitems.EstimatedNetValue,
                            DateCreated = dmsitems.DateCreated,
                            DateModified = (DateTime)dmsitems.DateModified,
                            OrderStatus = new StatusRep
                            {
                                Name = failedStatus.Name,
                                Code = failedStatus.Code,
                            },
                            OrderType = new StatusRep
                            {
                                Name = dmsitems.OrderType?.Name,
                                Code = dmsitems.OrderType?.Code,
                            },
                            Plant = new FailOrdersPlant(orderPlant.Id, orderPlant.Code, orderPlant.Name),
                            DistributorSapAccount = new FailOrderDistributorSapAccount(dmsitems.DistributorSapAccount.DistributorSapAccountId, dmsitems.DistributorSapAccount.DistributorSapNumber),
                            DmsOrderItems = itemList,

                        };
                        await _messageBus.PublishTopicMessage(dmsOrderFailedSubmissionMessage, EventMessages.DMSORDER_FAILED_SUBMISSION);
                    }
                    else if (!dmsitems.IsAtc && string.IsNullOrEmpty(dmsitems.OrderSapNumber))
                    {
                        var sapResult = await _sapService.CreateOrder(dmsitems);

                        dmsitems.NumberOfSapsubmissionAttempts = (dmsitems.NumberOfSapsubmissionAttempts ?? 0) + 1;
                        if (sapResult == null)
                        {
                            dmsitems.DateModified = DateTime.UtcNow;
                            await _dmsOrder.UpdateAsync(dmsitems);
                            _dmsOrder.Commit();
                        }
                        else if (sapResult.ActionFailed)
                        {
                            dmsitems.DateModified = DateTime.UtcNow;
                            dmsitems.OrderStatusId = (byte)Application.Enums.OrderStatus.Failed;
                            await _dmsOrder.UpdateAsync(dmsitems);
                            _dmsOrder.Commit();

                            var orderPlant = await _plant.Table.FirstOrDefaultAsync(p => p.Code == dmsitems.PlantCode);
                            var itemList = new List<FailSubmissionOrderItemsResponse>();
                            var item = dmsitems.DmsOrderItems.First();
                            var product = await _product.Table.FirstOrDefaultAsync(x => x.Id == item.ProductId);
                            itemList.Add(new FailSubmissionOrderItemsResponse
                            {
                                DmsOrderItemId = item.Id,
                                Quantity = item.Quantity,
                                SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode,
                                Product = new ProductResponse(product.Id, product.Price, product.Name),
                            });

                            DmsOrderSubmissionFailedMessage dmsOrderFailedSubmissionMessage = new()
                            {
                                UserId = dmsitems.UserId,
                                DmsOrderId = dmsitems.Id,
                                CompanyCode = dmsitems.CompanyCode,
                                CountryCode = dmsitems.CountryCode,
                                DateSubmittedtoDMS = dmsitems.DateCreated,
                                DeliveryDate = (DateTime)dmsitems.DeliveryDate,
                                DeliveryAddress = dmsitems.DeliveryAddress,
                                DeliveryCity = dmsitems.DeliveryCity,
                                DeliveryStateCode = dmsitems.DeliveryStateCode,
                                DeliveryCountryCode = dmsitems.DeliveryCountryCode,
                                DeliveryMethodCode = dmsitems.DeliveryMethodCode,
                                NumberOfSubmissionAttempts = (int)dmsitems.NumberOfSapsubmissionAttempts,
                                TruckSizeCode = dmsitems.TruckSizeCode,
                                EstimatedNetValue = (decimal)dmsitems.EstimatedNetValue,
                                DateCreated = dmsitems.DateCreated,
                                DateModified = (DateTime)dmsitems.DateModified,
                                OrderStatus = new StatusRep
                                {
                                    Name = failedStatus.Name,
                                    Code = failedStatus.Code,
                                },
                                OrderType = new StatusRep
                                {
                                    Name = dmsitems.OrderType?.Name,
                                    Code = dmsitems.OrderType?.Code,
                                },
                                Plant = new FailOrdersPlant(orderPlant.Id, orderPlant.Code, orderPlant.Name),
                                DistributorSapAccount = new FailOrderDistributorSapAccount(dmsitems.DistributorSapAccount.DistributorSapAccountId, dmsitems.DistributorSapAccount.DistributorSapNumber),
                                DmsOrderItems = itemList,
                            };
                            await _messageBus.PublishTopicMessage(dmsOrderFailedSubmissionMessage, EventMessages.DMSORDER_FAILED_SUBMISSION);
                        }
                        else
                        {
                            dmsitems.OrderStatusId = (byte)Application.Enums.OrderStatus.Submitted;
                            dmsitems.DateModified = DateTime.UtcNow;
                            dmsitems.SapDateCreated = DateTime.UtcNow;
                            dmsitems.DateSubmittedToSap = DateTime.UtcNow;
                            dmsitems.OrderSapNumber = sapResult.SapOrder.Id.ToString();
                            dmsitems.OrderSapNetValue = decimal.Parse(sapResult.SapOrder.netValue);
                            dmsitems.SapVat = (decimal?)sapResult.SapOrder.vat;
                            dmsitems.SapReference = sapResult.SapOrder.reference;
                            await _dmsOrder.UpdateAsync(dmsitems);
                            _dmsOrder.Commit();

                            var dmsOrderItem = dmsitems.DmsOrderItems.FirstOrDefault();
                            dmsOrderItem.SapNetValue = (decimal?)sapResult.SapOrder.lineItemNetValue;
                            dmsOrderItem.SapPricePerUnit = (decimal?)sapResult.SapOrder.lineItemPricePerUnit;
                            dmsOrderItem.DateModified = DateTime.UtcNow;
                            await _orderItem.UpdateAsync(dmsOrderItem);
                            _orderItem.Commit();

                            //Orders.DmsOrder.SubmissionSuccessful
                            var orderPlant = await _plant.Table.FirstOrDefaultAsync(p => p.Code == dmsitems.PlantCode);
                            var itemList = new List<FailSubmissionOrderItemsResponse>();
                            var item = dmsitems.DmsOrderItems.First();
                            var product = await _product.Table.FirstOrDefaultAsync(x => x.Id == item.ProductId);
                            itemList.Add(new FailSubmissionOrderItemsResponse
                            {
                                DmsOrderItemId = item.Id,
                                Quantity = item.Quantity,
                                SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode,
                                Product = new ProductResponse(product.Id, product.Price, product.Name),
                            });

                            DmsOrderSubmissionSuccessfulMessage dmsOrderSuccessfulSubmissionMessage = new();
                            dmsOrderSuccessfulSubmissionMessage.DmsOrderId = dmsitems.Id;
                            dmsOrderSuccessfulSubmissionMessage.DateCreated = dmsitems.DateCreated;
                            dmsOrderSuccessfulSubmissionMessage.DateModified = (DateTime)dmsitems.DateModified;
                            dmsOrderSuccessfulSubmissionMessage.DateSubmittedOnDMS = dmsitems.DateCreated;
                            dmsOrderSuccessfulSubmissionMessage.UserId = dmsitems.UserId;
                            dmsOrderSuccessfulSubmissionMessage.CompanyCode = dmsitems.CompanyCode;
                            dmsOrderSuccessfulSubmissionMessage.CountryCode = dmsitems.CountryCode;
                            dmsOrderSuccessfulSubmissionMessage.DistributorSapAccount = new FailOrderDistributorSapAccount(dmsitems.DistributorSapAccount.DistributorSapAccountId, dmsitems.DistributorSapAccount.DistributorSapNumber);
                            dmsOrderSuccessfulSubmissionMessage.EstimatedNetValue = dmsitems.EstimatedNetValue;
                            dmsOrderSuccessfulSubmissionMessage.OrderStatus = new StatusRep
                            {
                                Name = submittedStatus.Name,
                                Code = submittedStatus.Code,
                            };
                            dmsOrderSuccessfulSubmissionMessage.OrderType = new StatusRep
                            {
                                Name = dmsitems.OrderType?.Name,
                                Code = dmsitems.OrderType?.Code,
                            };
                            dmsOrderSuccessfulSubmissionMessage.TruckSizeCode = dmsitems.TruckSizeCode;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryMethodCode = dmsitems.DeliveryMethodCode;
                            dmsOrderSuccessfulSubmissionMessage.Plant = new FailOrdersPlant(orderPlant.Id, orderPlant.Code, orderPlant.Name);
                            dmsOrderSuccessfulSubmissionMessage.OrderSapNumber = dmsitems.OrderSapNumber;
                            dmsOrderSuccessfulSubmissionMessage.OrderSapNetValue = dmsitems.OrderSapNetValue;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryDate = (DateTime)dmsitems.DeliveryDate;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryAddress = dmsitems.DeliveryAddress;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryCity = dmsitems.DeliveryCity;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryStateCode = dmsitems.DeliveryStateCode;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryCountryCode = dmsitems.DeliveryCountryCode;
                            dmsOrderSuccessfulSubmissionMessage.DateSubmittedOnDMS = DateTime.UtcNow;
                            dmsOrderSuccessfulSubmissionMessage.DateSubmittedToSap = DateTime.UtcNow;
                            dmsOrderSuccessfulSubmissionMessage.NumberOfSubmissionAttempts = dmsitems.NumberOfSapsubmissionAttempts;
                            dmsOrderSuccessfulSubmissionMessage.DmsOrderItems = itemList;

                            await _messageBus.PublishTopicMessage(dmsOrderSuccessfulSubmissionMessage, EventMessages.SAPORDER_CREATED);
                        }

                    }
                    else if (dmsitems.IsAtc || !string.IsNullOrEmpty(dmsitems.OrderSapNumber))
                    {
                        var sapResult = await _sapService.CreateSAPDelivary(dmsitems);
                        dmsitems.NumberOfSapsubmissionAttempts = dmsitems.NumberOfSapsubmissionAttempts + 1;

                        if (!sapResult)
                        {
                            dmsitems.DateModified = DateTime.UtcNow;
                            await _dmsOrder.UpdateAsync(dmsitems);
                            _dmsOrder.Commit();
                        }
                        else
                        {
                            dmsitems.OrderStatusId = (byte)Application.Enums.OrderStatus.Submitted;
                            dmsitems.DateModified = DateTime.UtcNow;
                            dmsitems.SapDateCreated = DateTime.UtcNow;
                            dmsitems.DeliverySapNumber = string.Empty;
                            await _dmsOrder.UpdateAsync(dmsitems);
                            _dmsOrder.Commit();

                            //Orders.DmsOrder.SubmissionSuccessful
                            var orderPlant = await _plant.Table.FirstOrDefaultAsync(p => p.Code == dmsitems.PlantCode);
                            var itemList = new List<FailSubmissionOrderItemsResponse>();
                            var item = dmsitems.DmsOrderItems.First();
                            var product = await _product.Table.FirstOrDefaultAsync(x => x.Id == item.ProductId);
                            itemList.Add(new FailSubmissionOrderItemsResponse
                            {
                                DmsOrderItemId = item.Id,
                                Quantity = item.Quantity,
                                SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode,
                                Product = new ProductResponse(product.Id, product.Price, product.Name),

                            });

                            DmsOrderSubmissionSuccessfulMessage dmsOrderSuccessfulSubmissionMessage = new();
                            dmsOrderSuccessfulSubmissionMessage.DmsOrderId = dmsitems.Id;
                            dmsOrderSuccessfulSubmissionMessage.DateCreated = dmsitems.DateCreated;
                            dmsOrderSuccessfulSubmissionMessage.DateModified = (DateTime)dmsitems.DateModified;
                            dmsOrderSuccessfulSubmissionMessage.DateSubmittedOnDMS = dmsitems.DateCreated;
                            dmsOrderSuccessfulSubmissionMessage.UserId = dmsitems.UserId;
                            dmsOrderSuccessfulSubmissionMessage.CompanyCode = dmsitems.CompanyCode;
                            dmsOrderSuccessfulSubmissionMessage.CountryCode = dmsitems.CountryCode;
                            dmsOrderSuccessfulSubmissionMessage.DistributorSapAccount = new FailOrderDistributorSapAccount(dmsitems.DistributorSapAccount.DistributorSapAccountId, dmsitems.DistributorSapAccount.DistributorSapNumber);
                            dmsOrderSuccessfulSubmissionMessage.EstimatedNetValue = dmsitems.EstimatedNetValue;
                            dmsOrderSuccessfulSubmissionMessage.OrderStatus = new StatusRep
                            {
                                Name = submittedStatus.Name,
                                Code = submittedStatus.Code,
                            };
                            dmsOrderSuccessfulSubmissionMessage.OrderType = new StatusRep
                            {
                                Name = dmsitems.OrderType?.Name,
                                Code = dmsitems.OrderType?.Code,
                            };
                            dmsOrderSuccessfulSubmissionMessage.TruckSizeCode = dmsitems.TruckSizeCode;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryMethodCode = dmsitems.DeliveryMethodCode;
                            dmsOrderSuccessfulSubmissionMessage.Plant = new FailOrdersPlant(orderPlant.Id, orderPlant.Code, orderPlant.Name);
                            dmsOrderSuccessfulSubmissionMessage.OrderSapNumber = dmsitems.OrderSapNumber;
                            dmsOrderSuccessfulSubmissionMessage.OrderSapNetValue = dmsitems.OrderSapNetValue;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryDate = (DateTime)dmsitems.DeliveryDate;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryAddress = dmsitems.DeliveryAddress;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryCity = dmsitems.DeliveryCity;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryStateCode = dmsitems.DeliveryStateCode;
                            dmsOrderSuccessfulSubmissionMessage.DeliveryCountryCode = dmsitems.DeliveryCountryCode;
                            dmsOrderSuccessfulSubmissionMessage.DateSubmittedOnDMS = DateTime.UtcNow;
                            dmsOrderSuccessfulSubmissionMessage.DateSubmittedToSap = DateTime.UtcNow;
                            dmsOrderSuccessfulSubmissionMessage.NumberOfSubmissionAttempts = dmsitems.NumberOfSapsubmissionAttempts;
                            dmsOrderSuccessfulSubmissionMessage.DmsOrderItems = itemList;

                            await _messageBus.PublishTopicMessage(dmsOrderSuccessfulSubmissionMessage, EventMessages.SAPORDER_UPDATED);
                        }
                    }
                }
                catch (Exception err)
                {
                    _orderLogger.LogError($"{"Order submittion failed with error message : "} {err.Message} {" | "}{DateTime.UtcNow}");
                }
            }


            return ResponseHandler.SuccessResponse(SuccessMessages.ORDERS_SUCCESSFULLY_SUBMITTED);
        }
        public async Task<ApiResponse> AutoRefreshOrder(CancellationToken cancellationToken)
        {
            _orderLogger.LogInformation($"{"About to Start Auto Refresh order"}{" | "}{DateTime.UtcNow}");

            var dmsOrderslist = await _dmsOrder.Table.Include(o => o.OrderStatus).Include(g => g.DistributorSapAccount).Where(u =>
                         (u.DateRefreshed == null || u.DateRefreshed.Value.AddMinutes(10) <= DateTime.UtcNow)
                         && u.OrderStatusId != (byte)Application.Enums.OrderStatus.New && u.OrderStatusId != (byte)Application.Enums.OrderStatus.Saved
                         && u.OrderStatusId != (byte)Application.Enums.OrderStatus.Cancelled && u.OrderStatusId != (byte)Application.Enums.OrderStatus.Failed
                         && u.OrderStatusId != (byte)Application.Enums.OrderStatus.PendingOtp && u.OrderStatusId != (byte)Application.Enums.OrderStatus.ProcessingSubmission
                         && u.OrderStatusId != (byte)Application.Enums.OrderStatus.GoodsCollected && u.OrderStatusId != (byte)Application.Enums.OrderStatus.GoodsDelivered)
                        .OrderBy(u => u.DateCreated).Take(CacheKeys.RefreshBatchLimit).ToListAsync();

            List<Shared.Data.Models.OrderStatus> orderStatuses = await _orderStatus.Table.ToListAsync();
            var deliveryStatuses = await _deliveryStatus.Table.ToListAsync();
            var Ordertypes = await _orderType.Table.ToListAsync();
            var tripStatuses = await _tripStatus.Table.ToListAsync();
            var plants = await _plant.Table.ToListAsync();

            var parentDmsOrder = dmsOrderslist.Where(pdm => !pdm.IsAtc).ToList();

            if (parentDmsOrder.Count > 0)
            {

                foreach (var itemDmsOrder in parentDmsOrder)
                {
                    try
                    {
                        var SapOrder = await _sapService.GetOrderDetails(itemDmsOrder.CompanyCode, itemDmsOrder.CountryCode, itemDmsOrder.OrderSapNumber);
                        if (SapOrder == null)
                        {
                            itemDmsOrder.DateRefreshed = DateTime.UtcNow;
                            await _dmsOrder.UpdateAsync(itemDmsOrder);
                            _dmsOrder.Commit();
                        }
                        else
                        {

                            if (itemDmsOrder.OrderStatus?.Code.Trim().ToLower() != SapOrder.Status.Code.Trim().ToLower())
                            {
                                //Send Delivery Status Changed Mail
                                var StatusChangeMessage = new DmsOrderStatusChangeMessage();
                                StatusChangeMessage.DmsOrderId = itemDmsOrder.Id;
                                StatusChangeMessage.OrderSapNumber = itemDmsOrder.OrderSapNumber;
                                StatusChangeMessage.DateCreated = itemDmsOrder.DateCreated;
                                StatusChangeMessage.UserId = itemDmsOrder.UserId;
                                StatusChangeMessage.CompanyCode = itemDmsOrder.CompanyCode;
                                StatusChangeMessage.CountryCode = itemDmsOrder.CountryCode;
                                StatusChangeMessage.DistributorSapAccount = new distributorSapAccount
                                {
                                    DistributorSapAccountId = itemDmsOrder.DistributorSapAccount.DistributorSapAccountId,
                                    DistributorSapNumber = itemDmsOrder.DistributorSapAccount?.DistributorSapNumber
                                };
                                StatusChangeMessage.EstimatedNetValue = (decimal)itemDmsOrder.EstimatedNetValue;
                                StatusChangeMessage.OrderSapNetValue = (decimal)itemDmsOrder.OrderSapNetValue;
                                StatusChangeMessage.OldOrderStatus = new messageStatus
                                {
                                    Name = itemDmsOrder.OrderStatus.Name,
                                    Code = itemDmsOrder.OrderStatus.Code
                                };
                                StatusChangeMessage.NewOrderStatus = new messageStatus
                                {
                                    Name = SapOrder.Status.Name,
                                    Code = SapOrder.Status.Code
                                };
                                StatusChangeMessage.OrderType = new messageStatus
                                {
                                    Code = SapOrder.orderType.Code,
                                    Name = SapOrder.orderType.Name,
                                };
                                if (SapOrder.Status.Code == "E")
                                {
                                    await _messageBus.PublishTopicMessage(StatusChangeMessage, EventMessages.SAPORDER_GOODS_DELIVERED);

                                }
                                else if (SapOrder.Status.Code == "C")
                                {

                                    await _messageBus.PublishTopicMessage(StatusChangeMessage, EventMessages.SAPORDER_GOODS_DISPATCHED);
                                }
                                await _messageBus.PublishTopicMessage(StatusChangeMessage, EventMessages.DMSORDER_STATUS_CHANGED);
                            }



                            var orderType = Ordertypes.FirstOrDefault(x => x.Code == SapOrder.orderType.Code);
                            var stat = orderStatuses.FirstOrDefault(x => x.Code == SapOrder.Status.Code);

                            itemDmsOrder.OrderStatusId = stat.Id;
                            itemDmsOrder.OrderSapNetValue = decimal.Parse(SapOrder.netValue);
                            itemDmsOrder.SapVat = decimal.TryParse(SapOrder.vat, out decimal va) ? va : 0;
                            itemDmsOrder.DeliveryStatusId = deliveryStatuses.FirstOrDefault(x => x.Code == SapOrder.deliveryStatus.code).Id;
                            itemDmsOrder.WayBillNumber = SapOrder.delivery?.WayBillNumber?.ToString();
                            itemDmsOrder.DateModified = DateTime.UtcNow;
                            itemDmsOrder.DateRefreshed = DateTime.UtcNow;
                            itemDmsOrder.OrderTypeId = orderType.Id;

                            await _dmsOrder.UpdateAsync(itemDmsOrder);
                            _dmsOrder.Commit();

                            if (itemDmsOrder.DistributorSapAccount.AccountType != "Bank Guarantee")
                            {
                                var atcdetails = await _sapService.GetChildOrder(itemDmsOrder.CompanyCode, itemDmsOrder.CountryCode, itemDmsOrder.OrderSapNumber);
                                var count = atcdetails.Where(x => string.IsNullOrEmpty(x.DeliveryBlock.Code)).ToList().Count;
                                if (count != 0 && (itemDmsOrder.NumberOfChildAtc == null || itemDmsOrder.NumberOfChildAtc < count))
                                {
                                    itemDmsOrder.NumberOfChildAtc = count;


                                    //ServiceBus.Send(Orders.DmsOrder.ATCAvailable)
                                    var atcAvailableMessage = new ATCAvailableMessage();
                                    atcAvailableMessage.DmsOrderId = itemDmsOrder.Id;
                                    atcAvailableMessage.OrderSapNumber = itemDmsOrder.OrderSapNumber;
                                    atcAvailableMessage.DateCreated = itemDmsOrder.DateCreated;
                                    atcAvailableMessage.UserId = itemDmsOrder.UserId;
                                    atcAvailableMessage.CompanyCode = itemDmsOrder.CompanyCode;
                                    atcAvailableMessage.CountryCode = itemDmsOrder.CountryCode;
                                    atcAvailableMessage.DistributorSapAccount = new distributorSapAccount
                                    {
                                        DistributorSapAccountId = itemDmsOrder.DistributorSapAccountId,
                                        DistributorSapNumber = itemDmsOrder.DistributorSapAccount.DistributorSapNumber
                                    };
                                    atcAvailableMessage.EstimatedNetValue = (decimal)itemDmsOrder.EstimatedNetValue;
                                    atcAvailableMessage.OrderSapNetValue = (decimal)itemDmsOrder.OrderSapNetValue;
                                    atcAvailableMessage.NumberOfChildATC = count;
                                    atcAvailableMessage.NewOrderStatus = new messageStatus
                                    {
                                        Code = itemDmsOrder.OrderStatus.Code,
                                        Name = itemDmsOrder.OrderStatus.Name
                                    };
                                    atcAvailableMessage.OrderType = new messageStatus
                                    {
                                        Code = SapOrder.orderType.Code,
                                        Name = SapOrder.orderType.Name
                                    };

                                    await _messageBus.PublishTopicMessage(atcAvailableMessage, EventMessages.DMSORDER_ATC_AVAILABLE);
                                }
                            }

                            await _dmsOrder.UpdateAsync(itemDmsOrder);
                            _dmsOrder.Commit();

                            var detailOrderItems = await _orderItem.Table.FirstOrDefaultAsync(di => di.OrderId == itemDmsOrder.Id);

                            if (detailOrderItems != null)
                            {
                                detailOrderItems.OrderItemSapNumber = SapOrder.orderItems[0].Id.ToString();
                                detailOrderItems.SapPricePerUnit = decimal.TryParse(SapOrder.orderItems[0].pricePerUnit, out decimal pp) ? pp : 0;
                                detailOrderItems.SapNetValue = decimal.TryParse(SapOrder.orderItems[0].netValue, out decimal netv) ? netv : 0;
                                detailOrderItems.SapDeliveryQuality = decimal.Parse(SapOrder.orderItems[0].deliveryQuantity);
                                await _orderItem.UpdateAsync(detailOrderItems);
                                _orderItem.Commit();
                            }

                            //Orders.DmsOrder.Refreshed
                            DmsOrderRefreshedMessage dmsOrderRefreshedMessage = new();

                            dmsOrderRefreshedMessage.DmsOrderId = itemDmsOrder.Id;
                            dmsOrderRefreshedMessage.UserId = itemDmsOrder.UserId;
                            dmsOrderRefreshedMessage.DateModifed = (DateTime)itemDmsOrder.DateModified;
                            dmsOrderRefreshedMessage.DateRefreshed = DateTime.UtcNow;
                            dmsOrderRefreshedMessage.ModifiedByUserId = itemDmsOrder.UserId;
                            dmsOrderRefreshedMessage.UserId = itemDmsOrder.UserId;
                            dmsOrderRefreshedMessage.CompanyCode = itemDmsOrder.CompanyCode;
                            dmsOrderRefreshedMessage.CountryCode = itemDmsOrder.CountryCode;
                            dmsOrderRefreshedMessage.DistributorSapAccount = new DistributorSapAccountResponse(itemDmsOrder.DistributorSapAccountId, itemDmsOrder.DistributorSapAccount.DistributorSapNumber, itemDmsOrder.DistributorSapAccount.DistributorName);
                            dmsOrderRefreshedMessage.EstimatedNetValue = itemDmsOrder.EstimatedNetValue;
                            dmsOrderRefreshedMessage.oldOrderSapNetValue = itemDmsOrder.OrderSapNetValue;
                            dmsOrderRefreshedMessage.newOrderSapNetValue = decimal.Parse(SapOrder.netValue);
                            dmsOrderRefreshedMessage.OldOrderStatus = new OldOrderStatusResponse
                            {
                                Code = orderStatuses.FirstOrDefault(x => x.Id == itemDmsOrder.OrderStatusId)?.Code,
                                Name = orderStatuses.FirstOrDefault(x => x.Id == itemDmsOrder.OrderStatusId)?.Name,
                            };
                            dmsOrderRefreshedMessage.newOrderStatus = new NewOrderStatusResponse
                            {
                                Code = SapOrder.Status.Code,
                                Name = SapOrder.Status.Name,
                            };
                            dmsOrderRefreshedMessage.OrderType = new NewOrderStatusResponse
                            {
                                Code = Ordertypes.FirstOrDefault(x => x.Id == itemDmsOrder.OrderStatusId)?.Code,
                                Name = Ordertypes.FirstOrDefault(x => x.Id == itemDmsOrder.OrderStatusId)?.Name,

                            };

                            dmsOrderRefreshedMessage.OldDelivery = new NewOrderStatusResponse
                            {
                                Code = deliveryStatuses.FirstOrDefault(x => x.Id == itemDmsOrder.DeliveryStatusId)?.Code,
                                Name = deliveryStatuses.FirstOrDefault(x => x.Id == itemDmsOrder.DeliveryStatusId)?.Name,

                            };
                            dmsOrderRefreshedMessage.newDelivery = new NewOrderStatusResponse
                            {
                                Code = SapOrder.deliveryStatus.code,
                                Name = SapOrder.deliveryStatus.name,
                            };
                            dmsOrderRefreshedMessage.OldTrip = itemDmsOrder.TripStatusCode == null ? null : new TripResponse
                            {
                                TripStatus = new NewOrderStatusResponse
                                {
                                    Code = tripStatuses.FirstOrDefault(x => x.Code == itemDmsOrder.TripStatusCode)?.Code,
                                    Name = tripStatuses.FirstOrDefault(x => x.Code == itemDmsOrder.TripStatusCode)?.Name,
                                },
                                DispatchDate = (DateTime)itemDmsOrder.TripDispatchDate,
                                OdometerEnd = (int)itemDmsOrder?.TripOdometerEnd,
                                OdometerStart = (int)itemDmsOrder?.TripOdometerStart,
                                TripSapNumber = itemDmsOrder.SapTripNumber,

                            };
                            dmsOrderRefreshedMessage.NewTrip = new TripResponse();
                            dmsOrderRefreshedMessage.OldtruckSizeCode = itemDmsOrder.TruckSizeCode;
                            dmsOrderRefreshedMessage.NewtruckSizeCode = String.Empty;
                            dmsOrderRefreshedMessage.OldDeliveryMethodCode = itemDmsOrder.DeliveryMethodCode;
                            dmsOrderRefreshedMessage.NewDeliveryMethodCode = SapOrder.deliveryMethod?.code;
                            dmsOrderRefreshedMessage.Oldplant = new Application.DTOs.Events.PlantResponse
                            {
                                Code = plants.FirstOrDefault(x => x.Code == itemDmsOrder.PlantCode)?.Code,
                                PlantId = (int)(plants.FirstOrDefault(x => x.Code == itemDmsOrder.PlantCode)?.Id),
                                Name = plants.FirstOrDefault(x => x.Code == itemDmsOrder.PlantCode)?.Name,
                            };
                            dmsOrderRefreshedMessage.Newplant = new Application.DTOs.Events.PlantResponse
                            {
                                Code = plants.FirstOrDefault(x => x.Code == itemDmsOrder.PlantCode)?.Code,
                                PlantId = (int)(plants.FirstOrDefault(x => x.Code == itemDmsOrder.PlantCode)?.Id),
                                Name = plants.FirstOrDefault(x => x.Code == itemDmsOrder.PlantCode)?.Name,
                            };
                            dmsOrderRefreshedMessage.OldDeliveryDate = itemDmsOrder.DeliveryDate;
                            dmsOrderRefreshedMessage.NewDeliveryDate = DateTime.TryParse(SapOrder.delivery.deliveryDate, out DateTime dat) ? dat : null;
                            dmsOrderRefreshedMessage.OldDeliveryAddress = itemDmsOrder.DeliveryAddress;
                            dmsOrderRefreshedMessage.NewDeliveryAddress = itemDmsOrder.DeliveryAddress;

                            foreach (var item in itemDmsOrder.DmsOrderItems)
                            {
                                var product = await _product.Table.FirstOrDefaultAsync(x => x.Id == item.ProductId);
                                var ites = new OrderItemsResponse();
                                ites.DateCreated = item.DateCreated;
                                ites.DmsOrderItemId = item.Id;
                                ites.Product = new ProductResponse(item.ProductId, product.Price, product.Name);
                                ites.Quantity = item.Quantity;
                                ites.SalesUnitOfMeasureCode = item.SalesUnitOfMeasureCode;
                                dmsOrderRefreshedMessage.DmsOrderItems.Add(ites);
                            }
                            //await _messageBus.PublishTopicMessage(dmsOrderRefreshedMessage, EventMessages.DMSORDER_REFRESHED);
                        }
                    }
                    catch (Exception e)
                    {

                        _orderLogger.LogError($"Order Refresh completed with error : {e.Message} | {DateTime.UtcNow}");

                    }
                }

            }
            var parentAtcDmsOrders = dmsOrderslist.Where(pdm => pdm.IsAtc).ToList();
            if (parentAtcDmsOrders.Count > 0)
            {

                foreach (var itemAtcDmsOrder in parentAtcDmsOrders)
                {
                    try
                    {
                        var sapDetails = _distributorSapNo.Table.FirstOrDefault(x => x.DistributorSapAccountId == itemAtcDmsOrder.DistributorSapAccountId);

                        var SapOrder = await _sapService.GetOrderDetails(itemAtcDmsOrder.CompanyCode, itemAtcDmsOrder.CountryCode, itemAtcDmsOrder.OrderSapNumber);

                        if (SapOrder == null)
                        {
                            ////Alerts.Error
                            //var errorMessage = new DmsAlertErrorMessage
                            //{
                            //    Message = "Order Details Retrieval failed",
                            //    Source = "SAP Order Refresh"
                            //};
                            //await _messageBus.PublishTopicMessage(errorMessage, EventMessages.ALERT_ERROR);
                        }
                        else
                        {

                            if (itemAtcDmsOrder.OrderStatus?.Code.Trim().ToLower() != SapOrder.Status.Code.Trim().ToLower())
                            {
                                //Send Delivery Status Changed Mail
                                var DeliveryChangeMessage = new DmsOrderStatusChangeMessage();
                                DeliveryChangeMessage.DmsOrderId = itemAtcDmsOrder.Id;
                                DeliveryChangeMessage.OrderSapNumber = itemAtcDmsOrder.OrderSapNumber;
                                DeliveryChangeMessage.DateCreated = itemAtcDmsOrder.DateCreated;
                                DeliveryChangeMessage.UserId = itemAtcDmsOrder.UserId;
                                DeliveryChangeMessage.CompanyCode = itemAtcDmsOrder.CompanyCode;
                                DeliveryChangeMessage.CountryCode = itemAtcDmsOrder.CountryCode;
                                DeliveryChangeMessage.DistributorSapAccount = new distributorSapAccount
                                {
                                    DistributorSapAccountId = itemAtcDmsOrder.DistributorSapAccount.DistributorSapAccountId,
                                    DistributorSapNumber = itemAtcDmsOrder.DistributorSapAccount?.DistributorSapNumber
                                };
                                DeliveryChangeMessage.EstimatedNetValue = (decimal)itemAtcDmsOrder.EstimatedNetValue;
                                DeliveryChangeMessage.OrderSapNetValue = (decimal)itemAtcDmsOrder.OrderSapNetValue;
                                DeliveryChangeMessage.OldOrderStatus = new messageStatus
                                {
                                    Name = itemAtcDmsOrder.OrderStatus.Name,
                                    Code = itemAtcDmsOrder.OrderStatus.Code
                                };
                                DeliveryChangeMessage.NewOrderStatus = new messageStatus
                                {
                                    Name = SapOrder.Status.Name,
                                    Code = SapOrder.Status.Code
                                };
                                DeliveryChangeMessage.OrderType = new messageStatus
                                {
                                    Code = SapOrder.orderType.Code,
                                    Name = SapOrder.orderType.Name,
                                };
                                if (itemAtcDmsOrder.OrderStatus?.Code != "E" && SapOrder.Status.Code == "E")
                                {
                                    await _messageBus.PublishTopicMessage(DeliveryChangeMessage, EventMessages.SAPORDER_GOODS_DELIVERED);

                                }
                                else if (itemAtcDmsOrder.OrderStatus?.Code != "C" && SapOrder.Status.Code == "C")
                                {

                                    await _messageBus.PublishTopicMessage(DeliveryChangeMessage, EventMessages.SAPORDER_GOODS_DISPATCHED);
                                }


                                //await _messageBus.PublishTopicMessage(DeliveryChangeMessage, EventMessages.DMSORDER_STATUS_CHANGED);
                            }

                            itemAtcDmsOrder.OrderStatusId = _orderStatus.Table.FirstOrDefault(os => os.Code == SapOrder.Status.Code).Id;
                            itemAtcDmsOrder.OrderSapNetValue = decimal.Parse(SapOrder.netValue);
                            itemAtcDmsOrder.SapVat = decimal.TryParse(SapOrder.vat, out decimal vat) ? vat : 0;
                            itemAtcDmsOrder.DeliveryMethodCode = SapOrder.deliveryMethod.code;
                            itemAtcDmsOrder.DeliveryBlockCode = SapOrder.deliveryBlock.code;
                            itemAtcDmsOrder.SapFreightCharges = decimal.TryParse(SapOrder.freightCharges, out decimal fry) ? fry : 0;
                            itemAtcDmsOrder.DeliveryStateCode = SapOrder.deliveryStatus.code;
                            itemAtcDmsOrder.PlantCode = SapOrder.orderItems[0].plant.Code;
                            itemAtcDmsOrder.DeliverySapNumber = SapOrder.delivery.Id.ToString();
                            itemAtcDmsOrder.WayBillNumber = SapOrder.delivery.WayBillNumber;

                            itemAtcDmsOrder.DeliverySapNumber = SapOrder.delivery.Id.ToString();
                            var tripDetails = await _sapService.GetTrips(itemAtcDmsOrder.CompanyCode, itemAtcDmsOrder.CountryCode, SapOrder.delivery.Id.ToString());
                            //.OrderByDesc(atcdetails.Result.DateCreated).first()
                            if (tripDetails != null)
                            {
                                itemAtcDmsOrder.SapTripNumber = tripDetails.Id.ToString();
                                itemAtcDmsOrder.TripStatusCode = tripDetails.TripStatus.Code;
                                itemAtcDmsOrder.TripDispatchDate = tripDetails.DispatchDate;

                            }

                            itemAtcDmsOrder.DateModified = DateTime.UtcNow;
                            itemAtcDmsOrder.DateRefreshed = DateTime.UtcNow;

                            await _dmsOrder.UpdateAsync(itemAtcDmsOrder);
                            _dmsOrder.Commit();

                            foreach (var items in SapOrder.orderItems)
                            {
                                var OrderItemdetail = _orderItem.Table.FirstOrDefault(di => di.OrderItemSapNumber == items.Id.ToString());
                                if (OrderItemdetail != null)
                                {
                                    OrderItemdetail.SapPricePerUnit = string.IsNullOrEmpty(items.pricePerUnit) ? 0 : decimal.Parse(items.pricePerUnit);
                                    OrderItemdetail.SapNetValue = items.netValue == null ? 0 : decimal.Parse(items.netValue);
                                    OrderItemdetail.SapDeliveryQuality = items.deliveryQuantity == null ? 0 : decimal.TryParse(items.deliveryQuantity, out decimal quan) ? quan : 0;
                                    await _orderItem.UpdateAsync(OrderItemdetail);
                                    _orderItem.Commit();
                                }

                            }
                            //Orders.DmsOrder.Refreshed
                            DmsOrderRefreshedMessage dmsOrderRefreshedMessage = new();

                            dmsOrderRefreshedMessage.DmsOrderId = itemAtcDmsOrder.Id;
                            dmsOrderRefreshedMessage.UserId = itemAtcDmsOrder.UserId;
                            dmsOrderRefreshedMessage.DateModifed = (DateTime)itemAtcDmsOrder.DateModified;
                            dmsOrderRefreshedMessage.DateRefreshed = DateTime.UtcNow;
                            dmsOrderRefreshedMessage.ModifiedByUserId = itemAtcDmsOrder.UserId;
                            dmsOrderRefreshedMessage.UserId = itemAtcDmsOrder.UserId;
                            dmsOrderRefreshedMessage.CompanyCode = itemAtcDmsOrder.CompanyCode;
                            dmsOrderRefreshedMessage.CountryCode = itemAtcDmsOrder.CountryCode;
                            dmsOrderRefreshedMessage.DistributorSapAccount = new DistributorSapAccountResponse(sapDetails.DistributorSapAccountId, sapDetails.DistributorSapNumber, sapDetails.DistributorName);
                            dmsOrderRefreshedMessage.EstimatedNetValue = itemAtcDmsOrder.EstimatedNetValue;
                            dmsOrderRefreshedMessage.oldOrderSapNetValue = itemAtcDmsOrder.OrderSapNetValue;
                            dmsOrderRefreshedMessage.newOrderSapNetValue = decimal.Parse(SapOrder.netValue);
                            dmsOrderRefreshedMessage.OldOrderStatus = new OldOrderStatusResponse
                            {
                                Code = orderStatuses.FirstOrDefault(x => x.Id == itemAtcDmsOrder.OrderStatusId)?.Code,
                                Name = orderStatuses.FirstOrDefault(x => x.Id == itemAtcDmsOrder.OrderStatusId)?.Name,
                            };
                            dmsOrderRefreshedMessage.newOrderStatus = new NewOrderStatusResponse
                            {
                                Code = SapOrder.Status.Code,
                                Name = SapOrder.Status.Name,
                            };
                            dmsOrderRefreshedMessage.OrderType = new NewOrderStatusResponse
                            {
                                Code = Ordertypes.FirstOrDefault(x => x.Id == itemAtcDmsOrder.OrderStatusId)?.Code,
                                Name = Ordertypes.FirstOrDefault(x => x.Id == itemAtcDmsOrder.OrderStatusId)?.Name,

                            };

                            dmsOrderRefreshedMessage.OldDelivery = new NewOrderStatusResponse
                            {
                                Code = deliveryStatuses.FirstOrDefault(x => x.Id == itemAtcDmsOrder.DeliveryStatusId)?.Code,
                                Name = deliveryStatuses.FirstOrDefault(x => x.Id == itemAtcDmsOrder.DeliveryStatusId)?.Name,

                            };
                            dmsOrderRefreshedMessage.newDelivery = new NewOrderStatusResponse
                            {
                                Code = SapOrder.deliveryStatus.code,
                                Name = SapOrder.deliveryStatus.name,
                            };
                            dmsOrderRefreshedMessage.OldTrip = itemAtcDmsOrder.SapTripNumber == null ? null : new TripResponse
                            {
                                TripStatus = new NewOrderStatusResponse
                                {
                                    Code = tripStatuses.FirstOrDefault(x => x.Code == itemAtcDmsOrder.TripStatusCode)?.Code,
                                    Name = tripStatuses.FirstOrDefault(x => x.Code == itemAtcDmsOrder.TripStatusCode)?.Name,
                                },
                                DispatchDate = (DateTime)itemAtcDmsOrder.TripDispatchDate,
                                OdometerEnd = (int)itemAtcDmsOrder?.TripOdometerEnd,
                                OdometerStart = (int)itemAtcDmsOrder?.TripOdometerStart,
                                TripSapNumber = itemAtcDmsOrder.SapTripNumber,

                            };
                            dmsOrderRefreshedMessage.NewTrip = new TripResponse();
                            dmsOrderRefreshedMessage.OldtruckSizeCode = itemAtcDmsOrder.TruckSizeCode;
                            dmsOrderRefreshedMessage.NewtruckSizeCode = String.Empty;
                            dmsOrderRefreshedMessage.OldDeliveryMethodCode = itemAtcDmsOrder.DeliveryMethodCode;
                            dmsOrderRefreshedMessage.NewDeliveryMethodCode = SapOrder.deliveryMethod?.code;
                            dmsOrderRefreshedMessage.Oldplant = new Application.DTOs.Events.PlantResponse
                            {
                                Code = plants.FirstOrDefault(x => x.Code == itemAtcDmsOrder.PlantCode)?.Code,
                                PlantId = (int)(plants.FirstOrDefault(x => x.Code == itemAtcDmsOrder.PlantCode)?.Id),
                                Name = plants.FirstOrDefault(x => x.Code == itemAtcDmsOrder.PlantCode)?.Name,
                            };
                            dmsOrderRefreshedMessage.Newplant = new Application.DTOs.Events.PlantResponse
                            {
                                Code = plants.FirstOrDefault(x => x.Code == itemAtcDmsOrder.PlantCode)?.Code,
                                PlantId = (int)(plants.FirstOrDefault(x => x.Code == itemAtcDmsOrder.PlantCode)?.Id),
                                Name = plants.FirstOrDefault(x => x.Code == itemAtcDmsOrder.PlantCode)?.Name,
                            };
                            dmsOrderRefreshedMessage.OldDeliveryDate = itemAtcDmsOrder.DeliveryDate;
                            dmsOrderRefreshedMessage.NewDeliveryDate = DateTime.Parse(SapOrder.delivery.deliveryDate);
                            dmsOrderRefreshedMessage.OldDeliveryAddress = itemAtcDmsOrder.DeliveryAddress;
                            dmsOrderRefreshedMessage.NewDeliveryAddress = itemAtcDmsOrder.DeliveryAddress;

                           // await _messageBus.PublishTopicMessage(dmsOrderRefreshedMessage, EventMessages.DMSORDER_REFRESHED);
                        }
                    }
                    catch (Exception e)
                    {

                        _orderLogger.LogError($"Order Refresh completed with error : {e.Message} | {DateTime.UtcNow}");

                    }

                }

            }
            _orderLogger.LogInformation($"{"End"}{" | "}{DateTime.UtcNow}");
            return ResponseHandler.SuccessResponse(SuccessMessages.ORDERS_SUCCESSFULLY_REFRESHED);
        }
        public async Task<ApiResponse> OrderStatuses(CancellationToken cancellationToken)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order Statuses By The Id"}{" | "}{LoggedInUser()}{" | "}{DateTime.UtcNow}");
            var OrderStatuses = await _orderStatus.Table.Select(x => new Application.ViewModels.Responses.OrderStatus
            {
                Name = x.Name,
                Code = x.Code,
            }).ToListAsync(cancellationToken);
            _orderLogger.LogInformation($"{"DMS Order Statuses DB Response:-"}{" | "}{JsonConvert.SerializeObject(OrderStatuses)}");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_ORDERSTATUSES, new { orderStatuses = OrderStatuses });
        }
        public async Task<ApiResponse> GetMyDMSOrderChangeLog(int dmsOrderId, int UserId)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order Change Log By The Id"}{" | "}{LoggedInUser()}{" | "}{DateTime.UtcNow}");

            var dmsOrder = await _dmsOrder.SingleOrDefaultAsync(x => x.Id == dmsOrderId && x.UserId == UserId);
            if (dmsOrder == null)
                throw new NotFoundException(ErrorCodes.DMS_ORDER_NOTFOUND.Key, ErrorCodes.DMS_ORDER_NOTFOUND.Value);
            var dmsOrdersChangeLog = await _dmsChangeLogOrder.Table.Where(x => x.OrderId == dmsOrderId && x.OldOrderStatusId != x.NewOrderStatusId).ToListAsync();
            var logResponse = new List<ChangeLogResponse>();
            foreach (var log in dmsOrdersChangeLog)
            {
                var oldStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == log.OldOrderStatusId);
                var newStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == log.NewOrderStatusId);
                logResponse.Add(new ChangeLogResponse
                {
                    dmsOrderId = dmsOrderId,
                    ChangeType = log.ChangeType,
                    NewDateModified = log.NewDateModified.Value.ConvertToLocal(),
                    OldOrderStatus = new Application.ViewModels.Responses.OrderStatus { Code = oldStatus.Code, Name = oldStatus.Name },
                    NewOrderStatus = new Application.ViewModels.Responses.OrderStatus { Code = newStatus.Code, Name = newStatus.Name },

                });
            }
            _orderLogger.LogInformation($"{"DMS Order Change Log DB Response:-"}{" | "}{JsonConvert.SerializeObject(logResponse)}");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_DMS_CHANGE_LOG, new { dmsOrderChangeLog = logResponse });
        }
        public async Task<ApiResponse> GetMyDMSOrderChangeLog(string orderSapNumber, int UserId)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order Change Log By The Id"}{" | "}{LoggedInUser()}{" | "}{DateTime.UtcNow}");

            var dmsOrder = await _dmsOrder.Table.FirstOrDefaultAsync(x => x.OrderSapNumber == orderSapNumber && x.UserId == UserId);
            if (dmsOrder == null)
                throw new NotFoundException(ErrorCodes.NOT_DMS_ORIGIN.Key, ErrorCodes.NOT_DMS_ORIGIN.Value);
            var dmsOrdersChangeLog = await _dmsChangeLogOrder.Table.Where(x => x.OrderId == dmsOrder.Id && x.OldOrderStatusId != x.NewOrderStatusId).ToListAsync();
            var logResponse = new List<ChangeLogResponse>();
            foreach (var log in dmsOrdersChangeLog)
            {
                var oldStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == log.OldOrderStatusId);
                var newStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == log.NewOrderStatusId);
                logResponse.Add(new ChangeLogResponse
                {
                    dmsOrderId = log.Id,
                    ChangeType = log.ChangeType,
                    NewDateModified = log.NewDateModified.Value.ConvertToLocal(),
                    OldOrderStatus = new Application.ViewModels.Responses.OrderStatus { Code = oldStatus.Code, Name = oldStatus.Name },
                    NewOrderStatus = new Application.ViewModels.Responses.OrderStatus { Code = newStatus.Code, Name = newStatus.Name },

                });
            }
            _orderLogger.LogInformation($"{"DMS Order Change Log DB Response:-"}{" | "}{JsonConvert.SerializeObject(logResponse)}");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_RETRIEVAL_OF_DMS_CHANGE_LOG, new { dmsOrderChangeLog = logResponse });
        }
        public async Task<ApiResponse> ExportMySapOrder(int distributorSapAccountId, string orderSapNumber)
        {
            _orderLogger.LogInformation($"{"About to Export My Sap Order: Sap No"}{" | "}{LoggedInUser()}{" | "}{orderSapNumber}{" | "}{DateTime.UtcNow}");

            var sapDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == distributorSapAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(sapDetail)}");

            if (sapDetail == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            List<DmsOrder> redisDmsOrderItems = _sapService.GetSapOrders();
            var redisDmsOrderItem = redisDmsOrderItems?.Where(c => c.OrderSapNumber == orderSapNumber)?.FirstOrDefault();
            if (redisDmsOrderItem == null)
            {
                redisDmsOrderItem = _mapper.Map<DmsOrder>(await _sapService.GetOrderDetails(sapDetail.CompanyCode, sapDetail.CountryCode, orderSapNumber));
                if (redisDmsOrderItem == null)
                {
                    redisDmsOrderItem = await _dmsOrder.Table.FirstOrDefaultAsync(x => x.OrderSapNumber.Equals(orderSapNumber));

                }

                var DmsOrderItem = await _orderItem.Table.Where(x => x.OrderId == redisDmsOrderItem.Id).ToListAsync();
                redisDmsOrderItem.DmsOrderItems = DmsOrderItem;
                foreach (var item in redisDmsOrderItem.DmsOrderItems)
                {
                    item.Product = await _product.Table.Where(x => x.Id == item.ProductId).FirstOrDefaultAsync();
                }

            }

            if (redisDmsOrderItem.DistributorSapAccount.DistributorSapNumber != sapDetail.DistributorSapNumber)
                throw new NotFoundException(ErrorCodes.INVALID_ROUTE.Key, ErrorCodes.INVALID_ROUTE.Value);

            string contentType = String.Empty;
            string fileName = String.Empty;
            byte[] content = null;
            contentType = "application/pdf";
            fileName = "Order_data_Export.pdf";
            content = GeneratePdf(redisDmsOrderItem);

            _orderLogger.LogInformation($"{"DMS Order Export DB Response:-"}{" | "}{JsonConvert.SerializeObject(redisDmsOrderItem)}");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_RETRIEVAL,
                new { file = content });

            //throw new NotImplementedException();

        }
        public async Task<ApiResponse> ExportMySapOrder2(int distributorSapAccountId, string orderSapNumber)
        {
            _orderLogger.LogInformation($"{"About to Export My Sap Order: Sap No"}{" | "}{LoggedInUser()}{" | "}{orderSapNumber}{" | "}{DateTime.UtcNow}");

            var sapDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == distributorSapAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(sapDetail)}");

            if (sapDetail == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            var redisDmsOrderItem = _dmsOrder.Table.Include(x => x.DmsOrderItems).Where(c => c.OrderSapNumber == orderSapNumber)?.FirstOrDefault();
            if (redisDmsOrderItem == null)
            {
                var sapOrder = await _sapService.GetOrderDetails(sapDetail.CompanyCode, sapDetail.CountryCode, orderSapNumber);

                var Renderering = new ChromePdfRenderer(); // Instantiates Chrome Renderer
                var pdfs = Renderering.RenderHtmlAsPdf(GetHTMLString(sapOrder));
                using var memory = new MemoryStream();
                pdfs.Stream.CopyTo(memory);
                var files = memory.ToArray();

                return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_RETRIEVAL,
                    new { file = files });
            }
            foreach (var item in redisDmsOrderItem.DmsOrderItems)
            {
                item.Product = await _product.Table.Where(x => x.Id == item.ProductId).FirstOrDefaultAsync();
            }

            if (redisDmsOrderItem.DistributorSapAccount.DistributorSapNumber != sapDetail.DistributorSapNumber)
                throw new NotFoundException(ErrorCodes.INVALID_ROUTE.Key, ErrorCodes.INVALID_ROUTE.Value);

            var Renderer = new ChromePdfRenderer(); // Instantiates Chrome Renderer
            var pdf = Renderer.RenderHtmlAsPdf(GetHTMLString(redisDmsOrderItem));
            using var memoryStream = new MemoryStream();
            pdf.Stream.CopyTo(memoryStream);
            var file = memoryStream.ToArray();

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_RETRIEVAL,
                new { file = file });


        }
        public async Task<ApiResponse> SearchAtc(int distributorSapAccountId, string atcNumbers, CancellationToken cancellation)
        {
            _orderLogger.LogInformation($"{"About to Search for Child ATC: Sap No"} {atcNumbers + " At: "}{DateTime.UtcNow}");

            //var distributorSapAccount = await _distributorSapNo.whe
            var distributorSapAccount = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == distributorSapAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(distributorSapAccount)}");

            if (distributorSapAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);
            var sapChildOrders = new List<SapSearchDTO>();

            if (!string.IsNullOrEmpty(atcNumbers))
            {
                var atcNumberList = atcNumbers.Split(',');

                foreach (var atcNumber in atcNumberList)
                {
                    var SapChildren = await _sapService.SearchChildOrder(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, atcNumber);

                    if (atcNumberList.Count() == 1 && !string.IsNullOrEmpty(SapChildren.DeliveryBlock.Code))
                        return ResponseHandler.FailureResponse("error", "This ATC is Blocked and cannot ve viewed, contact your sales representative.");

                    if (SapChildren != null && string.IsNullOrEmpty(SapChildren.DeliveryBlock.Code))
                    {
                        if (SapChildren.DistributorNumber != distributorSapAccount.DistributorSapNumber)
                            throw new NotFoundException(ErrorCodes.ORDER_NOT_FOR_DISTRIBUTOR.Key, ErrorCodes.ORDER_NOT_FOR_DISTRIBUTOR.Value);

                        var dmsOrder = new DmsOrder();
                        dmsOrder.OrderSapNumber = SapChildren.Id;
                        dmsOrder.ParentOrderSapNumber = SapChildren.ParentId;
                        dmsOrder.IsAtc = true;
                        dmsOrder.OrderSapNetValue = decimal.Parse(SapChildren.NetValue);
                        dmsOrder.ChannelCode = null;
                        dmsOrder.DateCreated = DateTime.UtcNow;
                        dmsOrder.DateRefreshed = DateTime.UtcNow;
                        dmsOrder.UserId = distributorSapAccount.UserId;
                        dmsOrder.EstimatedNetValue = decimal.Parse(SapChildren.NetValue);



                        sapChildOrders.Add(new SapSearchDTO
                        {
                            DateCreated = SapChildren.DateCreated,
                            DeliveryBlock = SapChildren.DeliveryBlock,
                            DistributorNumber = SapChildren.DistributorNumber,
                            Id = SapChildren.Id,
                            NetValue = SapChildren.NetValue,
                            NumberOfItem = SapChildren.NumberOfItems,
                            OrderType = SapChildren.OrderType,
                            ParentId = SapChildren.ParentId,
                            Status = SapChildren.Status,
                            DeliveryAddress = SapChildren.DeliveryAddress,
                            DeliveryDate = SapChildren.DeliveryDate,

                        });
                    }

                }
            }

            _orderLogger.LogInformation($"{"Sap Child Orders Response:-"}{" | "}{JsonConvert.SerializeObject(sapChildOrders)}");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_SAP_CHILD_SEARCH, new { sapOrders = sapChildOrders });
        }
        Byte[] GeneratePdf(DmsOrder orderItem)
        {
            DataTable dataTable = new("DmsOrderDetails");
            dataTable.Columns.AddRange(new DataColumn[6] { new DataColumn("Id"),
                                            new DataColumn("CompanyCode"),
                                            new DataColumn("CountryCode"),
                                             //new DataColumn("DistributorSapAccountId"),
                                            new DataColumn("OrderSapNetValue"),
                                             new DataColumn("EstimatedNetValue"),
                                            new DataColumn("DateCreated") });
            dataTable.Rows.Add(orderItem.Id, orderItem.CompanyCode,
                       orderItem.CountryCode,
                       orderItem.OrderSapNetValue, orderItem.EstimatedNetValue, orderItem.DateCreated);
            var document = new Document
            {
                PageInfo = new PageInfo { Margin = new MarginInfo(28, 28, 28, 30) }
            };
            var pdfPage = document.Pages.Add();
            Aspose.Pdf.Table table = new()
            {
                ColumnWidths = "7% 16% 16% 16% 10% 16% 15%",
                DefaultCellPadding = new MarginInfo(10, 5, 10, 5),
                Border = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.Black),
                DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Aspose.Pdf.Color.Black),
            };

            table.ImportDataTable(dataTable, true, 0, 0);
            document.Pages[1].Paragraphs.Add(table);

            using var memoryStream = new MemoryStream();
            document.Save(memoryStream);
            return memoryStream.ToArray();

        }
        public static string GetHTMLString(DmsOrder orderItem)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <div class='header'><h1>Order Item Report</h1></div>
                                <table align='center'>
                                    <tr>
                                        <th>Order Id</th>
                                        <th>Order Sap Number</th>
                                        <th>Date Created</th>
                                        <th>Estimated Net Value</th>
                                    </tr>");

            sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                   <td>{2}</td>
                                   <td>{3}</td>
                                  </tr>", orderItem.Id, orderItem.OrderSapNumber, orderItem.DateCreated, orderItem.EstimatedNetValue);


            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }
        public static string GetHTMLString(Shared.ExternalServices.DTOs.SapOrder1 orderItem)
        {
            var sb = new StringBuilder();
            sb.Append(@"
                        <html>
                            <head>
                            </head>
                            <body>
                                <div class='header'><h1>Order Item Report</h1></div>
                                <table align='center'>
                                    <tr>
                                        <th>Order Id</th>
                                        <th>Order Sap Number</th>
                                        <th>Date Created</th>
                                        <th>Estimated Net Value</th>
                                    </tr>");

            sb.AppendFormat(@"<tr>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                   <td>{2}</td>
                                   <td>{3}</td>
                                  </tr>", orderItem.Id, orderItem.Id, DateTime.Now, orderItem.netValue);


            sb.Append(@"
                                </table>
                            </body>
                        </html>");

            return sb.ToString();
        }
        public async Task<ApiResponse> TrackMyOrder2(int distributorSapAccountId, string orderSapNumber, CancellationToken cancellation)
        {
            _orderLogger.LogInformation($"{"About to Get Order Tracking Info by Order Sap Number: Sap No"}{" | "}{LoggedInUser()}{" | "}{orderSapNumber}{DateTime.UtcNow}");

            //var distributorSapAccount = await _distributorSapNo.whe
            var distributorSapAccount = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == distributorSapAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(distributorSapAccount)}");
            var sapDelivery = new SapDeliveryDto();

            if (distributorSapAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);
            var sapTripDto = new OrderTrackerDTO();


            var sapTrip = new Shared.ExternalServices.DTOs.APIModels.SapTrip();
            var order = await _dmsOrder.Table.FirstOrDefaultAsync(x => x.OrderSapNumber == orderSapNumber && x.DistributorSapAccountId == distributorSapAccountId);
            if (order == null)
            {
                var orders = _sapService.GetSapOrders();
                order = orders?.Where(c => c.OrderSapNumber == orderSapNumber)?.FirstOrDefault();
                if (order == null)
                {
                    var sapOrder = await _sapService.GetOrderDetails(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, orderSapNumber);

                    if (sapOrder == null)
                    {
                        throw new NotFoundException(ErrorCodes.SAP_ORDER_NOTFOUND.Value, ErrorCodes.SAP_ORDER_NOTFOUND.Key);
                    }
                    else if (sapOrder.distirbutorNumber.ToString() != distributorSapAccount.DistributorSapNumber)
                    {
                        throw new NotFoundException(ErrorCodes.INVALID_ROUTE.Key, ErrorCodes.INVALID_ROUTE.Value);
                    }
                    else
                    {
                        var uom = await _unitOfMeasureRepository.ListAllAsync();

                        order = new DmsOrder();
                        order.DateCreated = DateTime.UtcNow;
                        order.DateRefreshed = DateTime.UtcNow;
                        order.DateModified = DateTime.UtcNow;
                        order.OrderSapNumber = sapOrder.Id.ToString();
                        order.ParentOrderSapNumber = sapOrder.parentId.ToString();
                        order.OrderSapNetValue = decimal.Parse(sapOrder.netValue);
                        order.DistributorSapAccountId = distributorSapAccount.DistributorSapAccountId;
                        order.IsAtc = true;
                        order.UserId = distributorSapAccount.UserId;
                        order.CompanyCode = distributorSapAccount.CompanyCode;
                        order.CountryCode = distributorSapAccount.CountryCode;
                        order.OrderStatusId = _orderStatus.Table.FirstOrDefault(x => x.Code.Trim().ToLower() == sapOrder.Status.Code.ToLower().Trim()).Id;
                        order.DeliveryStatusId = _deliveryStatus.Table.FirstOrDefault(x => x.Code.Trim().ToLower() == sapOrder.deliveryStatus.code.ToLower().Trim()).Id;
                        order.DeliverySapNumber = sapOrder.delivery.Id.ToString();

                        //sapDelivery = new SapDeliveryDto
                        //{
                        //    deliveryDate = DateTime.TryParse(sapOrder.delivery.deliveryDate, out DateTime dat) ? dat : null,
                        //    transportDate = DateTime.TryParse(sapOrder.delivery.transportDate, out DateTime dat1) ? dat1 : null,
                        //    Id = (int?)sapOrder.delivery.Id,
                        //    loadingDate = DateTime.TryParse(sapOrder.delivery.loadingDate, out DateTime dat2) ? dat2 : null,
                        //    pickUpDate = DateTime.TryParse(sapOrder.delivery.pickUpDate, out DateTime dat3) ? dat3 : null,
                        //    plannedGoodsMovementDate = DateTime.TryParse(sapOrder.delivery.plannedGoodsMovementDate, out DateTime dat4) ? dat4 : null,
                        //    WayBillNumber = sapOrder.delivery.WayBillNumber,
                        //};

                    }
                }
            }
            if (order.IsAtc && order.SapTripNumber == null)
            {
                sapTrip = await _sapService.GetTrips(order.CompanyCode, order.CountryCode, order.DeliverySapNumber);
                if (sapTrip != null)
                {
                    order.SapTripNumber = sapTrip.Id.ToString();
                    order.TripStatusCode = sapTrip.TripStatus.Code;
                    order.TripDispatchDate = sapTrip.DispatchDate;
                    //Check other sapTrip Details Here
                }
                else
                {
                    order.TripStatusCode = "Invalid or Self collection Order";
                }

                order.DateModified = DateTime.UtcNow;
                order.DateRefreshed = DateTime.UtcNow;
            }
            else
            {
                sapTrip = null;
            }
            var delivery = new DeliveryStatus();
            if (order.DeliveryStatusId != null)
            {
                delivery = await _deliveryStatus.GetByIdAsync((int)order.DeliveryStatusId);
                order.DeliveryStatus = delivery;
            }
            var orderStatus = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == order.OrderStatusId);
            order.OrderStatus = orderStatus;
            //var tripStatus = await _tripStatus.GetByIdAsync(order.tr)

            var orderTracker = new OrderTrackerDTO
            {
                dmsOrderId = order.Id,
                orderSapNumber = order.OrderSapNumber,
                parentOrderSapNumber = order.ParentOrderSapNumber,
                isATC = order.IsAtc,
                lastUpdated = order.DateRefreshed,
                deliveryStatus = delivery != null ? new Shared.ExternalServices.DTOs.APIModels.Status() { Code = delivery.Code, Name = delivery.Name } : null,
                orderStatus = orderStatus != null ? new Shared.ExternalServices.DTOs.APIModels.Status() { Name = orderStatus.Name, Code = orderStatus.Code } : null,
                //tripStatus = sapTrip == null ? null : new Shared.ExternalServices.DTOs.APIModels.Status() { Code = sapTrip.TripStatus.Code, Name =  sapTrip.TripStatus.Name },
                sapTrip = sapTrip == null ? null : new SapTripDto
                {
                    DateCreated = sapTrip.DateCreated,
                    DeliveryId = sapTrip.DeliveryId,
                    DispatchDate = sapTrip.DispatchDate,
                    Id = sapTrip.Id,
                    TruckLocation = sapTrip.TruckLocation,
                    TripStatus = new Shared.ExternalServices.DTOs.APIModels.Status
                    {
                        Code = sapTrip.TripStatus.Code,
                        Name = sapTrip.TripStatus.Name,
                    },
                },
                delivery = sapDelivery,

            };

            if (order.OrderStatus.Code == "A")
            {
                orderTracker.overalStatusMessage = _config.GetValue<string>("Messaging:OpenOrderStatus");
            }
            else if (order.OrderStatus.Code == "C" && orderTracker.sapTrip == null)
            {
                orderTracker.overalStatusMessage = _config.GetValue<string>("Messaging:ProcessingOrderStatus");
            }
            else if (order.OrderStatus.Code == "C" && order.SapTripNumber != null && order.TripStatusCode == "X" && order.TripDispatchDate != null)
            {
                orderTracker.overalStatusMessage = _config.GetValue<string>("Messaging:AwaitingLoadingOrderStatus");
            }
            else if (order.OrderStatus.Code == "C" && order.SapTripNumber != null && order.TripStatusCode == "X"
                && order.TripDispatchDate != null && order.TripOdometerStart <= 1)
            {
                orderTracker.overalStatusMessage = _config.GetValue<string>("Messaging:DispatchedOrderStatus");
            }
            else if (order.OrderStatus.Code == "C" && order.SapTripNumber != null && order.TripStatusCode == "X"
                && order.TripDispatchDate != null && order.TripOdometerStart > 1 && order.TripOdometerEnd == null)
            {
                orderTracker.overalStatusMessage = _config.GetValue<string>("Messaging:EnrouteOrderStatus");
            }
            else if (order.OrderStatus.Code == "C" && order.SapTripNumber != null && order.TripStatusCode == "C")
            {
                orderTracker.overalStatusMessage = _config.GetValue<string>("Messaging:DeliveredOrderStatus");
            }
            else if (order.OrderStatus.Code == "C" && order.SapTripNumber != null && order.TripStatusCode == "L")
            {
                orderTracker.overalStatusMessage = _config.GetValue<string>("Messaging:CancelledOrderStatus");
            }
            else if (order.OrderStatus.Code == "C" && order.SapTripNumber != null && order.TripStatusCode == "P")
            {
                orderTracker.overalStatusMessage = _config.GetValue<string>("Messaging:CancelledOrderStatus");
            }

            _orderLogger.LogInformation($"{"Order Tracker Response:-"}{" | "}{JsonConvert.SerializeObject(orderTracker)}");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_ORDER_TRACKER, new { orderTracker = orderTracker });
        }
        public async Task<ApiResponse> TrackMyOrder(int distributorSapAccountId, string orderSapNumber, CancellationToken cancellation)
        {
            _orderLogger.LogInformation($"{"About to Get Order Tracking Info by Order Sap Number: Sap No"}{" | "}{LoggedInUser()}{" | "}{orderSapNumber}{DateTime.UtcNow}");
            var distributorSapAccount = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == distributorSapAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(distributorSapAccount)}");

            if (distributorSapAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);
            var sapTripDto = new OrderTrackerDTO();

            var sapOrder = await _sapService.GetOrderDetails(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, orderSapNumber);
            if (sapOrder == null)
            {
                throw new NotFoundException(ErrorCodes.SAP_ORDER_NOTFOUND.Value, ErrorCodes.SAP_ORDER_NOTFOUND.Key);
            }
            else if (sapOrder.distirbutorNumber.ToString() != distributorSapAccount.DistributorSapNumber)
            {
                throw new NotFoundException(ErrorCodes.INVALID_ROUTE.Key, ErrorCodes.INVALID_ROUTE.Value);
            }

            sapTripDto.orderSapNumber = sapOrder.Id.ToString();
            sapTripDto.parentOrderSapNumber = sapOrder.parentId.ToString();
            sapTripDto.isATC = sapOrder.parentId != 0;
            sapTripDto.lastUpdated = DateTime.Now;
            sapTripDto.orderStatus = new Shared.ExternalServices.DTOs.APIModels.Status
            {
                Code = sapOrder.Status.Code,
                Name = sapOrder.Status.Name,
            };
            sapTripDto.deliveryStatus = new Shared.ExternalServices.DTOs.APIModels.Status
            {
                Code = sapOrder.deliveryStatus.code,
                Name = sapOrder.deliveryStatus.name,
            };
            sapTripDto.delivery = new SapDeliveryDto
            {
                deliveryDate = DateTime.TryParse(sapOrder.delivery.deliveryDate, out DateTime deliverydate) ? deliverydate : null,
                transportDate = DateTime.TryParse(sapOrder.delivery.transportDate, out DateTime transportDate) ? transportDate : null,
                Id = sapOrder.delivery.Id.ToString(),
                loadingDate = DateTime.TryParse(sapOrder.delivery.loadingDate, out DateTime loadingDate) ? loadingDate : null,
                pickUpDate = DateTime.TryParse(sapOrder.delivery.pickUpDate, out DateTime pickUpDate) ? pickUpDate : null,
                plannedGoodsMovementDate = DateTime.TryParse(sapOrder.delivery.plannedGoodsMovementDate, out DateTime plannedGoodsMovementDate) ? plannedGoodsMovementDate : null,
                WayBillNumber = sapOrder.delivery.WayBillNumber,
            };
            var sapTrip = await _sapService.GetTrips(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, sapOrder.delivery.Id.ToString());

            sapTripDto.sapTrip = new SapTripDto
            {
                DateCreated = sapTrip?.DateCreated,
                DeliveryId = sapTrip?.DeliveryId,
                DispatchDate = sapTrip?.DispatchDate,
                Id = sapTrip?.Id,
                TruckLocation = sapTrip?.TruckLocation,
                TripStatus = new Shared.ExternalServices.DTOs.APIModels.Status
                {
                    Code = sapTrip?.TripStatus.Code,
                    Name = sapTrip?.TripStatus.Name,
                },
            };

            _orderLogger.LogInformation($"{"Order Tracker Response:-"}{" | "}{JsonConvert.SerializeObject(sapTripDto)}");

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_ORDER_TRACKER, new { orderTracker = sapTripDto });
        }
        public async Task<ApiResponse> Dashboard(int distributorSapAccountId)
        {
            var report = new DashboardReport();
            _orderLogger.LogInformation($"{"About to Get User Dashboard Metrics"}{" | "}{LoggedInUser()}{" | "}{DateTime.UtcNow}");

            //var distributorSapAccount = await _distributorSapNo.whe
            var distributorSapAccount = await _distributorSapNo.Table.Where(c => c.DistributorSapAccountId == distributorSapAccountId && c.UserId == LoggedInUser()).FirstOrDefaultAsync();
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(distributorSapAccount)}");
            if (distributorSapAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            var distributorSapAcc = new distributorSapAccount1
            {
                distributorSapAccountId = distributorSapAccountId,
                companyCode = distributorSapAccount.CompanyCode,
                distributorName = distributorSapAccount.DistributorName,
                distributorNumber = distributorSapAccount.DistributorSapNumber,
            };

            var toDate = DateTime.UtcNow;
            //This is to be gotten from Appsettings
            var defaultOrderPeriodDays = 60;
            var fromDate = DateTime.UtcNow.AddDays(-1 * defaultOrderPeriodDays);

            var SapOrders = await _sapService.GetOrder(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, distributorSapAccount.DistributorSapNumber, fromDate, toDate);
            var recentOrders = new List<DashBoardOrders>();
            foreach (var order in SapOrders)
            {
                recentOrders.Add(new DashBoardOrders
                {
                    Id = order.Id,
                    DateCreated = order.DateCreated,
                    OrderType = order.OrderType,
                    OrderStatus = order.Status,
                    Netvalue = decimal.Parse(order.NetValue),
                    NumItems = int.Parse(order.NumberOfItems)
                });
            }

            var dmsOrders = await _dmsOrder.Table.Where(x => x.UserId == LoggedInUser() /*&& x.OrderTypeId == (int)OrderTypesEnum.ZDOR*/
            && x.DistributorSapAccountId == distributorSapAccountId && (x.OrderStatusId == 1 || x.OrderStatusId == 2 || x.OrderStatusId == 7)).ToListAsync();
            report.RecentOrders = recentOrders;
            report.distributorSapAccount = distributorSapAcc;
            report.Metrics = new DashboardMetric
            {
                NumNew = dmsOrders.Count(x => x.OrderStatusId == 1 /*(int)Application.Enums.OrderStatus.New*/),
                NumSaved = dmsOrders.Count(x => x.OrderStatusId == 2 /*(int)Application.Enums.OrderStatus.Saved*/),
                NumSubmitted = dmsOrders.Count(x => x.OrderStatusId == 7/*(int)Application.Enums.OrderStatus.Submitted*/),
            };

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_DASHBOARD_ORDER, new { dashboardReport = report });
        }
        public async Task<ApiResponse> Dashboard()
        {
            var report = new DashboardReport2();
            _orderLogger.LogInformation($"{"About to Get User Dashboard Metrics"}{" | "}{LoggedInUser()}{" | "}{DateTime.Now}");

            //var distributorSapAccount = await _distributorSapNo.whe
            var distributorSapAccount = await _distributorSapNo.Table.Where(c => c.UserId == LoggedInUser()).ToListAsync();
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(distributorSapAccount)}");
            if (distributorSapAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            var toDate = DateTime.Now;
            //This is to be gotten from Appsettings
            var defaultOrderPeriodDays = 60;
            var fromDate = DateTime.Now.AddDays(-1 * defaultOrderPeriodDays);
            var recentOrders = new List<DashBoardOrders>();
            foreach (var dist in distributorSapAccount)
            {
                var SapOrders = await _sapService.GetOrder(dist.CompanyCode, dist.CountryCode, dist.DistributorSapNumber, fromDate, toDate);
                foreach (var order in SapOrders)
                {
                    recentOrders.Add(new DashBoardOrders
                    {
                        Id = order.Id,
                        DateCreated = order.DateCreated,
                        OrderType = order.OrderType,
                        OrderStatus = order.Status,
                        Netvalue = decimal.Parse(order.NetValue),
                        NumItems = int.Parse(order.NumberOfItems)
                    });
                }
            }
            var dmsOrders = await _dmsOrder.Table.Where(x => x.UserId == LoggedInUser() /*&& x.OrderTypeId == (int)OrderTypesEnum.ZDOR*/
                             && (x.OrderStatusId == 1 || x.OrderStatusId == 2 || x.OrderStatusId == 7)).ToListAsync();
            report.RecentOrders = recentOrders;
            report.Metrics = new DashboardMetric
            {
                NumNew = dmsOrders.Count(x => x.OrderStatusId == 1),
                NumSaved = dmsOrders.Count(x => x.OrderStatusId == 2),
                NumSubmitted = dmsOrders.Count(x => x.OrderStatusId == 7),
            };
            report.RecentOrders = recentOrders;

            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_DASHBOARD_ORDER, new { dashboardReport = report });
        }
        public async Task<ApiResponse> RequestReportByProduct(RequestReportByProductViewModel vm)
        {
            _orderLogger.LogInformation($"{"About to Get Order Tracking Info by Order Sap Number: Sap No"}{" | "}{LoggedInUser()}{" | "}{vm.distributorSapAccountId}{DateTime.UtcNow}");

            //var distributorSapAccount = await _distributorSapNo.whe
            var distributorSapAccount = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == vm.distributorSapAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(distributorSapAccount)}");
            if (distributorSapAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            var dmsOrdersItems = await _orderItem.Table.Include(X => X.Product).Include(x => x.Order).Where(x => x.DateCreated.Date >= vm.fromDate.Date && x.DateCreated.Date <= vm.toDate.Date && x.UserId == LoggedInUser()
            && x.Order.DistributorSapAccountId == vm.distributorSapAccountId && x.Order.OrderStatusId != 1 && x.Order.OrderStatusId != 2 && x.Order.OrderStatusId != 8
            && x.Order.OrderStatusId != 9).ToListAsync();

            if (vm.productId > 0)
            {
                dmsOrdersItems = dmsOrdersItems.Where(x => x.ProductId == vm.productId).ToList();
            }

            var totalCount = dmsOrdersItems.Count();
            var totalPages = NumberManipulator.PageCountConverter(totalCount, vm.pageSize);
            if (vm.pageSize > totalCount)
            {
                vm.pageSize = totalCount;
            }
            if (vm.sort == ReportSortingEnum.DateAscending)
            {
                dmsOrdersItems = dmsOrdersItems.OrderBy(x => x.Order.DateCreated).ToList();
            }
            else if (vm.sort == ReportSortingEnum.DateDescending)
            {
                dmsOrdersItems = dmsOrdersItems.OrderByDescending(x => x.Order.DateCreated).ToList();
            }
            else if (vm.sort == ReportSortingEnum.ValueAscending)
            {
                dmsOrdersItems = dmsOrdersItems.OrderBy(x => x.Order.OrderSapNetValue).ToList();
            }
            else if (vm.sort == ReportSortingEnum.ValueDescending)
            {
                dmsOrdersItems = dmsOrdersItems.OrderByDescending(x => x.Order.OrderSapNetValue).ToList();
            }
            dmsOrdersItems = dmsOrdersItems.Skip((vm.pageIndex - 1) * vm.pageSize).Take(vm.pageSize).ToList();

            var Itemresponse = new List<ReportByProductViewModel>();
            foreach (var item in dmsOrdersItems)
            {
                var status = await _orderStatus.Table.FirstOrDefaultAsync(x => x.Id == item.Order.OrderStatusId);
                var type = await _orderType.Table.FirstOrDefaultAsync(x => x.Id == item.Order.OrderTypeId);
                Itemresponse.Add(new ReportByProductViewModel
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    ProductSapNumber = item.Product.ProductSapNumber,
                    Quantity = item.Quantity,
                    SalesUnitOfMeasure = new StatusRep
                    {
                        Code = item.SalesUnitOfMeasureCode,
                        Name = item.SalesUnitOfMeasureCode
                    },
                    dmsOrder = new OrderDms
                    {
                        SAPOrderNumber = item.Order.OrderSapNumber,
                        DmsOrderId = item.Order.Id,
                        DmsOrderGroupId = item.Order.DmsOrderGroupId,
                        EstimatedNetvalue = item.Order.EstimatedNetValue,
                        CompanyCode = item.Order.CompanyCode,
                        CountryCode = item.Order.CountryCode,
                        DateCreated = item.Order.DateCreated.ConvertToLocal(),
                        IsATC = item.Order.IsAtc,
                        OrderSapNetValue = item.Order.OrderSapNetValue,
                        OrderStatus = new StatusRep
                        {
                            Code = status.Code,
                            Name = status.Name,
                        },
                        OrderType = new StatusRep
                        {
                            Code = type.Code,
                            Name = type.Name
                        }
                    }
                });
            };

            var dmsOrderItemsResponse = new PaginatedListVM<object>(Itemresponse,
                new PaginationMetaData(vm.pageIndex, vm.pageSize, totalPages, totalCount));
            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_DASHBOARD_ORDER, dmsOrderItemsResponse);
        }
        public async Task<ApiResponse> ProofOfDelivery(ProofRequestDto model)
        {

            var distributorSapAccount = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == model.distributorSapAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(distributorSapAccount)}");
            if (distributorSapAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            var proofModel = new ProofOfDelivery
            {
                atcNumber = model.atcNumber,
                companyCode = distributorSapAccount.CompanyCode,
                countryCode = distributorSapAccount.CountryCode,
                distributorNumber = distributorSapAccount.DistributorSapNumber,
            };

            var requestProof = await _sapService.ProofOfDelivery(proofModel);
            return ResponseHandler.SuccessResponse($"{requestProof.message}");

        }
        public async Task<ApiResponse> RequestOrderDocument(ProofRequestDto model)
        {

            var distributorSapAccount = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == model.distributorSapAccountId);
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(distributorSapAccount)}");
            if (distributorSapAccount == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);

            var sapOrder = await _sapService.GetOrderDetails(distributorSapAccount.CompanyCode, distributorSapAccount.CountryCode, model.atcNumber);
            if (sapOrder == null)
                throw new NotFoundException(ErrorCodes.SAP_ORDER_NOTFOUND.Value, ErrorCodes.SAP_ORDER_NOTFOUND.Key);

            if (sapOrder?.distirbutorNumber.ToString() != distributorSapAccount.DistributorSapNumber)
                throw new NotFoundException(ErrorCodes.DMS_ORDER_NOTFOUND.Value, ErrorCodes.DMS_ORDER_NOTFOUND.Key);

            if (!sapOrder.orderType.Code.StartsWith('Y') && sapOrder.orderType.Code.ToUpper() != "ZDBO" && sapOrder.orderType.Code.ToUpper() != "ZDCI" && sapOrder.orderType.Code.ToUpper() != "ZSBO" && sapOrder.orderType.Code.ToUpper() != "ZSCI"
                && sapOrder.orderType.Code.ToUpper() != "ZXDJ" && sapOrder.orderType.Code.ToUpper() != "ZPBO" && sapOrder.orderType.Code.ToUpper() != "ZXSJ")
                throw new NotFoundException(ErrorCodes.DOCUMENT_REQUEST_ERROR.Value, ErrorCodes.DOCUMENT_REQUEST_ERROR.Key);

            var documentModel = new OrderDocumentVm()
            {
                atcNumber = model.atcNumber,
                companyCode = distributorSapAccount.CompanyCode,
                countryCode = distributorSapAccount.CountryCode,
                //distributorNumber = distributorSapAccount.DistributorSapNumber,
            };

            var documentReportResponse = await _sapService.RequestOrderDocument(documentModel);
            if (string.IsNullOrEmpty(documentReportResponse.message))
            {
                throw new NotFoundException("An error occurred while performing operation", "Error");
            }
            return ResponseHandler.SuccessResponse($"{documentReportResponse.message}");

        }
    }
    public partial class OrderService
    {
        private IConverter _converter;
        private readonly IAsyncRepository<DmsOrder> _dmsOrder;
        private readonly IAsyncRepository<Product> _product;
        private readonly IAsyncRepository<DistributorSapAccount> _distributorSapNo;
        private readonly IAsyncRepository<DmsOrderItem> _orderItem;
        private readonly IAsyncRepository<DmsOrdersChangeLog> _dmsChangeLogOrder;
        private readonly IAsyncRepository<TripStatus> _tripStatus;
        private readonly IAsyncRepository<Shared.Data.Models.OrderStatus> _orderStatus;
        private readonly IAsyncRepository<OrderType> _orderType;
        private readonly IAsyncRepository<Plant> _plant;
        private readonly IAsyncRepository<ShoppingCart> _shoppingCartRepository;
        private readonly IAsyncRepository<Shared.Data.Models.UnitOfMeasure> _unitOfMeasureRepository;
        private readonly IAsyncRepository<Otp> _otp;
        private readonly ILogger<OrderService> _orderLogger;
        private readonly ICachingService _cache;
        private readonly ISapService _sapService;
        private readonly IOtpService _otpService;
        private IMapper _mapper;
        public readonly IQueueMessagingService _messageBus;
        private readonly IAsyncRepository<DeliveryMethod> _deliveryMethod;
        private readonly IAsyncRepository<ShoppingCartItem> _shoppingCartItemRepo;
        private readonly IAsyncRepository<TruckSize> _truckSize;
        private readonly IAsyncRepository<DeliveryStatus> _deliveryStatus;
        private readonly IAsyncRepository<DmsOrderGroup> _orderGroup;
        private readonly IConfiguration _config;
        public OrderService(IConverter _converter, IAsyncRepository<DmsOrder> dmsOrder, IAsyncRepository<Plant> _plant, IAuthenticatedUserService authenticatedUserService,
           ILogger<OrderService> orderLogger, ICachingService cache, IMapper mapper, ISapService sapService, IOtpService otpService,
           IAsyncRepository<DistributorSapAccount> distributorSapNo, IAsyncRepository<DmsOrdersChangeLog> dmsChangeLogOrder,
            IAsyncRepository<ShoppingCart> shoppingCartRepository, IAsyncRepository<DmsOrderItem> dmsOrderItem,
            IAsyncRepository<Shared.Data.Models.OrderStatus> orderStatus, IAsyncRepository<OrderType> orderType,
           IQueueMessagingService messageBus, IAsyncRepository<Otp> otp,
          IAsyncRepository<DmsOrderGroup> _orderGroup, IAsyncRepository<ShoppingCartItem> _shoppingCartItemRepo,
           IAsyncRepository<DeliveryMethod> deliveryMethod, IAsyncRepository<TruckSize> truckSize, IConfiguration _config,
           IAsyncRepository<Shared.Data.Models.UnitOfMeasure> _unitOfMeasureRepository, IAsyncRepository<TripStatus> _tripStatus,
           IAsyncRepository<DeliveryStatus> deliveryStatus, IAsyncRepository<Product> product) : base(authenticatedUserService)
        {
            this._unitOfMeasureRepository = _unitOfMeasureRepository;
            this._tripStatus = _tripStatus;
            this._config = _config;
            this._plant = _plant;
            this._orderGroup = _orderGroup;
            this._converter = _converter;
            _dmsOrder = dmsOrder;
            _orderLogger = orderLogger;
            _cache = cache;
            _mapper = mapper;
            _distributorSapNo = distributorSapNo;
            _sapService = sapService;
            _messageBus = messageBus;
            _otpService = otpService;
            _otp = otp;
            _shoppingCartRepository = shoppingCartRepository;
            _orderStatus = orderStatus;
            _orderType = orderType;
            _dmsChangeLogOrder = dmsChangeLogOrder;
            _orderItem = dmsOrderItem;
            _deliveryMethod = deliveryMethod;
            _truckSize = truckSize;
            _deliveryStatus = deliveryStatus;
            _product = product;
            this._shoppingCartItemRepo = _shoppingCartItemRepo;
        }
        #region Private Methods
        private static Func<IQueryable<DmsOrder>, IOrderedQueryable<DmsOrder>> ProcessOrderFunc(RequestSortingDto orderExpression = null)
        {
            IOrderedQueryable<DmsOrder> orderFunction(IQueryable<DmsOrder> queryable)
            {
                if (orderExpression == null)
                    return queryable.OrderByDescending(p => p.DateCreated);
                IOrderedQueryable<DmsOrder> orderQueryable = null;
                if (orderExpression.IsDateAscending)
                    orderQueryable = queryable.OrderBy(p => p.DateCreated).ThenByDescending(p => p.DateCreated);
                else if (orderExpression.IsDateDescending)
                    orderQueryable = queryable.OrderByDescending(p => p.DateCreated).ThenByDescending(p => p.DateCreated);
                else if (orderExpression.IsValueAscending)
                    orderQueryable = queryable.OrderBy(p => p.OrderSapNetValue ?? p.EstimatedNetValue).ThenByDescending(p => p.OrderSapNetValue ?? p.EstimatedNetValue);
                else if (orderExpression.IsValueDescending)
                    orderQueryable = queryable.OrderByDescending(p => p.OrderSapNetValue ?? p.EstimatedNetValue).ThenByDescending(p => p.OrderSapNetValue ?? p.EstimatedNetValue);


                return orderQueryable;
            }
            return orderFunction;
        }

        private static Func<IQueryable<Shared.ExternalServices.DTOs.APIModels.SapOrderDto>, IOrderedQueryable<Shared.ExternalServices.DTOs.APIModels.SapOrderDto>> ProcessOrderFunc1(RequestSortingDto orderExpression = null)
        {
            IOrderedQueryable<Shared.ExternalServices.DTOs.APIModels.SapOrderDto> orderFunction(IQueryable<Shared.ExternalServices.DTOs.APIModels.SapOrderDto> queryable)
            {
                if (orderExpression == null)
                    return queryable.OrderByDescending(p => p.DateCreated);
                IOrderedQueryable<Shared.ExternalServices.DTOs.APIModels.SapOrderDto> orderQueryable = null;
                if (orderExpression.IsDateAscending)
                    orderQueryable = queryable.OrderBy(p => p.DateCreated).ThenByDescending(p => p.DateCreated);
                else if (orderExpression.IsDateDescending)
                    orderQueryable = queryable.OrderByDescending(p => p.DateCreated).ThenByDescending(p => p.DateCreated);
                else if (orderExpression.IsValueAscending)
                    orderQueryable = queryable.OrderBy(p => p.NetValue).ThenByDescending(p => p.NetValue /*?? p.NetValue*/);
                else if (orderExpression.IsValueDescending)
                    orderQueryable = queryable.OrderByDescending(p => p.NetValue /* ?? p.NetValue*/).ThenByDescending(p => p.NetValue /*?? p.NetValue*/);


                return orderQueryable;
            }
            return orderFunction;
        }

        private void ConstructSorting(OrderSortingEnum model, ref RequestSortingDto sorting)
        {
            switch (model)
            {
                case Application.Enums.OrderSortingEnum.DateAscending:
                    sorting.IsDateAscending = true;
                    break;
                case Application.Enums.OrderSortingEnum.DateDescending:
                    sorting.IsDateDescending = true;
                    break;
                case Application.Enums.OrderSortingEnum.ValueDescending:
                    sorting.IsValueDescending = true;
                    break;
                case Application.Enums.OrderSortingEnum.ValueAscending:
                    sorting.IsValueAscending = true;
                    break;
                default:
                    break;
            }
        }


        private static OrderDetailResponse ProcessQuery(Shared.Data.Models.DmsOrdersChangeLog ordersChangeLog)
        {
            if (ordersChangeLog == null)
                return null;

            var ordersChangeLogResponse = new OrderDetailResponse()
            {
                DmsOrderId = (int)ordersChangeLog.OrderId,
                ChangeType = ordersChangeLog.ChangeType,
                NewDateModified = ordersChangeLog.NewDateModified,
                NewOrderStatus = ordersChangeLog.Order.OrderStatus?.Name,
                OldOrderStatus = ordersChangeLog.Order.OrderStatus?.Name == null ? null : ordersChangeLog.Order.OrderStatus?.Name
            };
            return ordersChangeLogResponse;
        }

        #endregion
    }
}
