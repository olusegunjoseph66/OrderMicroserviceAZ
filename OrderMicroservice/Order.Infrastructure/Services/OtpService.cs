
using Account.Application.Constants;
using Account.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Order.Application.Configurations;
using Order.Application.Constants;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Events;
using Order.Application.DTOs.Features.Otp;
using Order.Application.DTOs.Request;
using Order.Application.Enums;
using Order.Application.Exceptions;
using Order.Application.Interfaces.Services;
using Order.Application.ViewModels.Responses;
using Order.Infrastructure.Services;
using Shared.Data.Models;
using Shared.Data.Repository;
using Shared.ExternalServices.Interfaces;
using Shared.Utilities.Helpers;

namespace Account.Infrastructure.Services
{
    public class OtpService : BaseService, IOtpService
    {
        private readonly IAsyncRepository<Otp> _otpRepository;
        private readonly OtpSettings _otpSetting;
        public readonly IQueueMessagingService _messageBus;
        private readonly ILogger<OtpService> _otpLogger;
        private readonly IAsyncRepository<DmsOrder> _dmsOrder;
        private readonly IAsyncRepository<Shared.Data.Models.OrderStatus> _orderStatus;
        private readonly IAsyncRepository<OrderType> _orderType;
        private readonly IAsyncRepository<Plant> _plant;
        public OtpService(IAuthenticatedUserService authenticatedUserService,
            IAsyncRepository<Shared.Data.Models.OrderStatus> _orderStatus,
            IAsyncRepository<OrderType> _orderType,
            IAsyncRepository<Plant> _plant,
        IAsyncRepository<Otp> otpRepository, IOptions<OtpSettings> otpSetting,
            IQueueMessagingService messageBus, ILogger<OtpService> otpLogger, IAsyncRepository<DmsOrder> dmsOrder) : base(authenticatedUserService)
        {
            _otpRepository = otpRepository;
            _otpSetting = otpSetting.Value;
            _messageBus = messageBus;
            _otpLogger = otpLogger;
            _dmsOrder = dmsOrder;
            this._orderStatus = _orderStatus;
            this._orderType = _orderType;
            this._plant = _plant;
        }

        public async Task<OtpDto> GenerateOtp(string emailAddress, int? orderid, int? dmsOrderGroupId,
              bool isNewOtp = true, string phoneNumber = null, int? userId = 0,
              CancellationToken cancellationToken = default)
        {
            var otpCode = RandomValueGenerator.GenerateRandomDigits(_otpSetting.OtpCodeLength);
            Otp otp = new();

            otp = new Otp
            {
                Code = otpCode,
                DateCreated = DateTime.UtcNow,
                DateExpiry = DateTime.UtcNow.AddMinutes(_otpSetting.OtpExpiryInMinutes),
                EmailAddress = emailAddress,
                NumberOfRetries = 0,
                DmsOrderId = orderid,
                DmsOrderGroupId = dmsOrderGroupId,
                OtpStatusId = (byte)OtpStatusEnum.New
            };
            if (phoneNumber != null)
                otp.PhoneNumber = phoneNumber;

            //if (userId > 0)
            //    otp.us = userId;

            await _otpRepository.AddAsync(otp, cancellationToken);

            await _otpRepository.CommitAsync(cancellationToken);

            return new OtpDto { Id = otp.Id, Code = otpCode, DateExpiry = otp.DateExpiry };
        }
        public async Task<ApiResponse> ResendOtp(ResendOtpRequestDTO otp)
        {
            var otpItem = await _otpRepository.Table.FirstOrDefaultAsync(c => c.Id == otp.otpId);
            _otpLogger.LogInformation($"{"DMS OTP DB Response:-"}{" | "}{JsonConvert.SerializeObject(otpItem)}");

            if (otpItem == null)
                throw new NotFoundException(ErrorCodes.INCORECT_OTP.Key, ErrorCodes.INCORECT_OTP.Value);

            else if (otpItem.OtpStatusId == (byte)OtpStatusEnum.Validated)
                throw new ConflictException(ErrorCodes.OTP_HAS_BEEN_USED.Key, ErrorCodes.OTP_HAS_BEEN_USED.Value);

            else if (otpItem.OtpStatusId == (byte)OtpStatusEnum.Invalidated)
                throw new ConflictException(ErrorCodes.INVALID_OTP.Key, ErrorCodes.INVALID_OTP.Value);

            //else if (otpItem.DateExpiry < DateTime.UtcNow)
            //    throw new ConflictException(ErrorCodes.OPT_EXPIRED.Key, ErrorCodes.OPT_EXPIRED.Value);

            //else if (otpItem.DateCreated.AddMinutes(_otpSetting.RetryIntervalInMinutes) < DateTime.UtcNow)
            //    throw new ConflictException(ErrorCodes.RESEND_OTP_TIME_NOT_ELAPSED.Key, ErrorCodes.RESEND_OTP_TIME_NOT_ELAPSED.Value.
            //        Replace("{n}", _otpSetting.RetryIntervalInMinutes.ToString()));

            else
            {
                var otpCode = RandomValueGenerator.GenerateRandomDigits(_otpSetting.OtpCodeLength);
                //Otp otpModel = new();

                //otpModel = new Otp
                //{
                //    Code = otpCode,
                //    Id = otpModel.Id,
                //    DateCreated = DateTime.UtcNow,
                //    DateExpiry = DateTime.UtcNow.AddMinutes(_otpSetting.OtpExpiryInMinutes),
                //    EmailAddress = otpItem.EmailAddress,
                //    NumberOfRetries = 0,
                //    OtpStatusId = (byte)OtpStatusEnum.New
                //};

                otpItem.Code = otpCode;
                otpItem.DateExpiry = DateTime.UtcNow.AddMinutes(_otpSetting.OtpExpiryInMinutes);
                otpItem.NumberOfRetries += 1;
                
                await _otpRepository.UpdateAsync(otpItem);
                await _otpRepository.CommitAsync(default);

                OrdersOtpGeneratedMessage otpVM = new()
                {
                    DateCreated = DateTime.UtcNow,
                    DateExpiry = otpItem.DateExpiry,
                    EmailAddress = otpItem.EmailAddress,
                    PhoneNumber = otpItem.PhoneNumber,
                    OtpCode = otpItem.Code,
                    OtpId = otpItem.Id,
                };
                await _messageBus.PublishTopicMessage(otpVM, EventMessages.ORDER_OTP_GENERATED);

                return ResponseHandler.SuccessResponse(SuccessMessages.OTP_GENERATED_SUCCESSFULLY);
            }
        }

        public async Task<ApiResponse> ValidateOtp(ValidateOtpRequestDTO otp)
        {
            var otpItem = await _otpRepository.Table.FirstOrDefaultAsync(c => c.Id == otp.otpId && c.Code == otp.otpCode);
            _otpLogger.LogInformation($"{"DMS OTP DB Response:-"}{" | "}{JsonConvert.SerializeObject(otpItem)}");

            if (otpItem == null)
                throw new NotFoundException(ErrorCodes.INCORECT_OTP.Value, ErrorCodes.INCORECT_OTP.Key);

            else if (otpItem.OtpStatusId == (byte)OtpStatusEnum.Validated)
                throw new ConflictException(ErrorCodes.OTP_HAS_BEEN_USED.Value, ErrorCodes.OTP_HAS_BEEN_USED.Key);

            else if (otpItem.OtpStatusId == (byte)OtpStatusEnum.Invalidated)
                throw new ConflictException(ErrorCodes.INVALID_OTP.Value, ErrorCodes.INVALID_OTP.Key);

            else if (otpItem.DateExpiry < DateTime.UtcNow)
                throw new ConflictException(ErrorCodes.OPT_EXPIRED.Value, ErrorCodes.OPT_EXPIRED.Key);

            else if (otpItem.Code != otp.otpCode)
            {
                otpItem.NumberOfRetries++;
                if (otpItem.NumberOfRetries > _otpSetting.MaximumRequiredRetries)
                {
                    otpItem.OtpStatusId = (byte)OtpStatusEnum.Invalidated;
                }
                await _otpRepository.UpdateAsync(otpItem);
                await _otpRepository.CommitAsync(default);
                throw new ConflictException(ErrorCodes.INVALID_OTP.Value, ErrorCodes.INVALID_OTP.Key);
            }
            else
            {
                otpItem.OtpStatusId = (byte)OtpStatusEnum.Validated;

                await _otpRepository.UpdateAsync(otpItem);
                await _otpRepository.CommitAsync(default);

                var dmsOrdersList = new List<DmsOrder>();
                if (otpItem.DmsOrderId != null)
                {
                    var dmsOrder = await _dmsOrder.Table.Include(c => c.DistributorSapAccount).FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.Id == otpItem.DmsOrderId);
                    if (dmsOrder == null)
                        throw new NotFoundException(ErrorCodes.DMS_ORDER_NOTFOUND.Key, ErrorCodes.DMS_ORDER_NOTFOUND.Value);
                    dmsOrdersList.Add(dmsOrder);
                }
                if (otpItem.DmsOrderGroupId != null)
                {
                    //Check the groupId Here
                    var dmsOrders = await _dmsOrder.Table.Include(c => c.DistributorSapAccount).Where(c => c.UserId == LoggedInUser() && c.DmsOrderGroupId == otpItem.DmsOrderGroupId).ToListAsync();
                    foreach (var ord in dmsOrders)
                    {
                        dmsOrdersList.Add(ord);
                    }
                }
                foreach (var item in dmsOrdersList)
                {
                    var oldOrder = await _dmsOrder.Table.AsNoTracking().Include(c => c.DistributorSapAccount).FirstOrDefaultAsync(c => c.Id == item.Id);
                    var sapDetails = oldOrder?.DistributorSapAccount;
                    var orderItem = item;
                    orderItem.OrderStatusId = (byte)Order.Application.Enums.OrderStatus.ProcessingSubmission;
                    orderItem.DateCreated = DateTime.Now;
                    orderItem.DateSubmittedOnDms = DateTime.Now;

                    await _dmsOrder.UpdateAsync(orderItem);
                    await _otpRepository.CommitAsync(default);


                    DmsOrderUpdatedMessage dmsOrderUpdatedMessage = new();

                    dmsOrderUpdatedMessage.DmsOrderId = orderItem.Id;
                    dmsOrderUpdatedMessage.DateModified = DateTime.Now;
                    dmsOrderUpdatedMessage.ModifiedByUserId = orderItem.UserId;
                    dmsOrderUpdatedMessage.UserId = orderItem.UserId;
                    dmsOrderUpdatedMessage.CompanyCode = orderItem.CompanyCode;
                    dmsOrderUpdatedMessage.CountryCode = orderItem.CountryCode;
                    dmsOrderUpdatedMessage.OrderSapNumber = orderItem.OrderSapNumber;
                    dmsOrderUpdatedMessage.DistributorSapAccount = new DistributorSapAccountResponse(sapDetails.DistributorSapAccountId, sapDetails.DistributorSapNumber, sapDetails.DistributorName);
                    dmsOrderUpdatedMessage.EstimatedNetValue = orderItem.EstimatedNetValue;
                    dmsOrderUpdatedMessage.OldOrderStatus = new OldOrderStatusResponse
                    {
                        Code = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Code,
                        Name = _orderStatus.Table.FirstOrDefault(x => x.Id == oldOrder.OrderStatusId)?.Name,
                    };
                    dmsOrderUpdatedMessage.NewOrderStatus = new NewOrderStatusResponse
                    {
                        Code = _orderStatus.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Code,
                        Name = _orderStatus.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Name,
                    };
                    dmsOrderUpdatedMessage.OrderType = new NewOrderStatusResponse
                    {
                        Code = _orderType.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Code,
                        Name = _orderType.Table.FirstOrDefault(x => x.Id == orderItem.OrderStatusId)?.Name,
                    };
                    dmsOrderUpdatedMessage.OldtruckSizeCode = oldOrder.TruckSizeCode;
                    dmsOrderUpdatedMessage.NewtruckSizeCode = orderItem.TruckSizeCode;
                    dmsOrderUpdatedMessage.OldDeliveryMethodCode = orderItem.DeliveryMethodCode;
                    dmsOrderUpdatedMessage.NewDeliveryMethodCode = orderItem.DeliveryMethodCode;
                    dmsOrderUpdatedMessage.Oldplant = new Order.Application.DTOs.Events.PlantResponse
                    {
                        PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Id),
                        Name = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Name,
                        Code = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Code,

                    };
                    dmsOrderUpdatedMessage.Newplant = new Order.Application.DTOs.Events.PlantResponse
                    {
                        PlantId = (int)(_plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Id),
                        Name = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Name,
                        Code = _plant.Table.FirstOrDefault(p => p.Code == orderItem.PlantCode)?.Code,

                    };
                    dmsOrderUpdatedMessage.OldDeliveryDate = (DateTime)orderItem.DeliveryDate;
                    dmsOrderUpdatedMessage.NewDeliveryDate = (DateTime)orderItem.DeliveryDate;
                    dmsOrderUpdatedMessage.OldDeliveryAddress = oldOrder.DeliveryAddress;
                    dmsOrderUpdatedMessage.NewDeliveryAddress = orderItem.DeliveryAddress;
                    dmsOrderUpdatedMessage.OldDeliveryCity = oldOrder.DeliveryCity;
                    dmsOrderUpdatedMessage.NewDeliveryCity = orderItem.DeliveryCity;
                    dmsOrderUpdatedMessage.OldDeliveryStateCode = oldOrder.DeliveryStateCode;
                    dmsOrderUpdatedMessage.NewDeliveryStateCode = orderItem.DeliveryStateCode;
                    dmsOrderUpdatedMessage.OldDeliveryCountryCode = oldOrder.DeliveryCountryCode;
                    dmsOrderUpdatedMessage.NewDeliveryCountryCode = orderItem.DeliveryCountryCode;
                    //dmsOrderUpdatedMessage.DmsOrderItems = (List<OrderItemsResponse>)orderItem.DmsOrderItems;
                    foreach (var item1 in orderItem.DmsOrderItems)
                    {
                        dmsOrderUpdatedMessage.DmsOrderItems.Add(new OrderItemsResponse
                        {
                            DateCreated = item.DateCreated,
                            DmsOrderItemId = item.Id,
                            Product = new ProductResponse(item1.ProductId, item1.Product.Price, item1.Product.Name),
                            Quantity = item1.Quantity,
                            SalesUnitOfMeasureCode = item1.SalesUnitOfMeasureCode
                        });
                    }

                    await _messageBus.PublishTopicMessage(dmsOrderUpdatedMessage, EventMessages.ORDER_DMS_UPDATED);
                }

            }

            return ResponseHandler.SuccessResponse(SuccessMessages.ORDER_SENT_FOR_SUBMISSION);
        }
    }
}
