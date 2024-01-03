using Account.Application.Constants;
using Account.Application.Interfaces.Services;
using AutoMapper;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Order.Application.Configurations;
using Order.Application.Constants;
using Order.Application.DTOs.Events;
using Order.Application.DTOs.Response;
using Order.Application.Interfaces.Messaging;
using Order.Application.Interfaces.Services;
using Order.Application.ViewModels.Responses;
using Shared.Data.Models;
using Shared.Data.Repository;
using Shared.ExternalServices.Configurations;
using Shared.ExternalServices.Interfaces;
using System.Text;

namespace Order.Infrastructure.Services.Messaging
{
    public partial class AzureServiceBusConsumer : BaseService, IAzureServiceBusConsumer
    {
        private IAsyncRepository<DmsOrder> _dmsOrder;
        private IAsyncRepository<Product> _product;
        private IAsyncRepository<DistributorSapAccount> _distributorSapNo;
        private IAsyncRepository<DmsOrderItem> _orderItem;
        private IAsyncRepository<DmsOrdersChangeLog> _changeLog;
        private IAsyncRepository<ShoppingCartItem> _cartItem;
        private IAsyncRepository<DmsOrderGroup> _orderGroup;
        private IAsyncRepository<ShoppingCart> _cart;
        private IAsyncRepository<Otp> _otp;
        private readonly ILogger<OrderService> _orderLogger;
        private readonly ICachingService _cache;
        private readonly ISapService _sapService;
        private readonly IOtpService _otpService;
        private IMapper _mapper;
        private readonly MessagingServiceSetting _messagingSetting;
        public readonly IQueueMessagingService _messageBus;
        private readonly OtpSettings _otpSetting;
        private SubscriptionClient productClient;
        private ServiceBusProcessor productProcessor;
        //private ServiceBusProcessor orderProcessor;
        private ServiceBusProcessor accountProcessor;
        public readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AzureServiceBusConsumer> _logger;
        private readonly IConfiguration _config;
        public AzureServiceBusConsumer(IAsyncRepository<DmsOrder> dmsOrder, IAuthenticatedUserService authenticatedUserService,
            IAsyncRepository<ShoppingCartItem> _cartItem, IAsyncRepository<ShoppingCart> _cart, IAsyncRepository<DmsOrderGroup> _orderGroup,
          ILogger<OrderService> orderLogger, ICachingService cache, IMapper mapper, ISapService sapService, IOtpService otpService,
          IAsyncRepository<DistributorSapAccount> distributorSapNo, IAsyncRepository<Product> product, IAsyncRepository<DmsOrderItem> orderItem,
          IQueueMessagingService messageBus, IAsyncRepository<Otp> otp, IOptions<OtpSettings> otpSetting, IAsyncRepository<DmsOrdersChangeLog> _changeLog,
          IOptions<MessagingServiceSetting> messagingSetting, IServiceScopeFactory scopeFactory, ILogger<AzureServiceBusConsumer> logger, IConfiguration _config
            ) : base(authenticatedUserService)
        {
            this._config = _config;
            this._orderItem = orderItem;
            _dmsOrder = dmsOrder;
            _orderLogger = orderLogger;
            _cache = cache;
            _mapper = mapper;
            _distributorSapNo = distributorSapNo;
            _sapService = sapService;
            _messageBus = messageBus;
            _otpService = otpService;
            _otp = otp;
            _otpSetting = otpSetting.Value;
            _product = product;
            _messagingSetting = messagingSetting.Value;
            _scopeFactory = scopeFactory;
            this._changeLog = _changeLog;
            this._cartItem = _cartItem;
            this._cart = _cart;
            this._orderGroup = _orderGroup;
            //orderProcessor = _messageBus.ConsumeMessage(EventMessages.SAP_ORDER_UPDATED, EventMessagesSubscription.SAP_ORDER_UPDATED);
            productProcessor = _messageBus.ConsumeMessage(EventMessages.PRODUCTS_TOPIC, EventMessagesSubscription.PRODUCTS_PRODUCT_REFRESHED);
            accountProcessor = _messageBus.ConsumeMessage(EventMessages.ACCOUNT_TOPIC, EventMessagesSubscription.PRODUCTS_PRODUCT_REFRESHED);

            _logger = logger;
        }
        public async Task StartOrderMsg()
        {
            //try
            //{
            //    subscriptionClient = new SubscriptionClient(sbConnectionString, sbTopic, sbSubscription);

            //    var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            //    {
            //        MaxConcurrentCalls = 1,
            //        AutoComplete = false
            //    };
            //    subscriptionClient.RegisterMessageHandler(ReceiveMessagesAsync, messageHandlerOptions);
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //}
            //finally
            //{
            //    Console.ReadKey();
            //    subscriptionClient.CloseAsync();
            //}
        }
        //public async Task StartOrderMsg()
        //{
        //    orderProcessor.ProcessMessageAsync += OnAutoUpdateDmsOrderFromSapUpdate;
        //    orderProcessor.ProcessErrorAsync += ErrorHanler;
        //    await orderProcessor.StartProcessingAsync();
        //}
        private SubscriptionClient ConfigureSubClient(string topic, string subscription)
        {
            return new SubscriptionClient(_messagingSetting.ConnectionString, topic, subscription);
        }
        public async Task StopOrderMsg()
        {
            //await orderProcessor.StopProcessingAsync();
            //await orderProcessor.DisposeAsync();
        }

        Task ErrorHanler(ProcessErrorEventArgs args)
        {
            Console.WriteLine(args.Exception.ToString());
            return Task.CompletedTask;
        }

        public async Task StartAccountMsg()
        {
            accountProcessor.ProcessMessageAsync += OnAutoDistributorAccount;
            accountProcessor.ProcessErrorAsync += ErrorHanler;
            await accountProcessor.StartProcessingAsync();
        }

        public async Task StopAccountMsg()
        {
            //await accountProcessor.StopProcessingAsync();
            //await accountProcessor.DisposeAsync();
        }
        //public async Task StartProductMsg()
        //{
        //    try
        //    {
        //        productClient = ConfigureSubClient(EventMessages.PRODUCTS_TOPIC, EventMessagesSubscription.PRODUCTS_PRODUCT_REFRESHED);
        //        var messageHandlerOptions = new MessageHandlerOptions(ExceptionProductReceivedHandler)
        //        {
        //            MaxConcurrentCalls = 1,
        //            AutoComplete = false,
        //        };
        //        productClient.RegisterMessageHandler(ReceiveProductMessagesAsync, messageHandlerOptions);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //    finally
        //    {
        //        Console.ReadKey();
        //        await ConfigureSubClient(EventMessages.PRODUCTS_TOPIC, EventMessagesSubscription.PRODUCTS_PRODUCT_REFRESHED)
        //            .CloseAsync();
        //    }
        //}
        async Task ReceiveProductMessagesAsync(Message message, CancellationToken token)
        {
            Console.WriteLine($"Subscribed message: {Encoding.UTF8.GetString(message.Body)}");
            if (message.Label.ToLower() == EventMessages.PRODUCTS_PRODUCT_REFRESHED.ToLower())
            {
                var body = Encoding.UTF8.GetString(message.Body);

                var productMsg = JsonConvert.DeserializeObject<ProductRefreshedMessage>(body);

                using var scope = _scopeFactory.CreateScope();
                _product = scope.ServiceProvider.GetRequiredService<IAsyncRepository<Product>>();
                var productFromDb = await _product.Table.FirstOrDefaultAsync(c => c.Id == productMsg.ProductId);
                if (productFromDb == null)
                {
                    Product mapProduct = new();
                    mapProduct.ProductSapNumber = productMsg.ProductSapNumber;
                    mapProduct.Name = productMsg.Name;
                    mapProduct.Id = productMsg.ProductId;
                    mapProduct.DateRefreshed = productMsg.DateRefreshed;
                    mapProduct.CompanyCode = productMsg.CompanyCode;
                    mapProduct.CountryCode = productMsg.CountryCode;
                    mapProduct.Price = productMsg.Price;
                    mapProduct.UnitOfMeasureCode = productMsg.UnitOfMeasureCode;

                    await _product.AddAsync(mapProduct);
                }
                else
                {
                    productFromDb = new Product();
                    productFromDb.ProductSapNumber = productMsg.ProductSapNumber;
                    productFromDb.Name = productMsg.Name;
                    productFromDb.DateRefreshed = productMsg.DateRefreshed;
                    productFromDb.CompanyCode = productMsg.CompanyCode;
                    productFromDb.CountryCode = productMsg.CountryCode;
                    productFromDb.Price = productMsg.Price;
                    productFromDb.UnitOfMeasureCode = productMsg.UnitOfMeasureCode;
                    productFromDb.DateRefreshed = DateTime.UtcNow;
                    await _product.UpdateAsync(productFromDb);
                }
                await _product.CommitAsync(default);
            }
            //var sub = ConfigureSubClient(EventMessages.PRODUCTS_TOPIC, EventMessagesSubscription.PRODUCTS_PRODUCT_REFRESHED);
            await productClient.CompleteAsync(message.SystemProperties.LockToken);
        }

        static Task ExceptionProductReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine(exceptionReceivedEventArgs.Exception);
            return Task.CompletedTask;
        }
        public async Task StartProductMsg()
        {
            productProcessor.ProcessMessageAsync += OnAutoUpdateProduct;
            productProcessor.ProcessErrorAsync += ErrorHanler;
            await productProcessor.StartProcessingAsync();
            //await SyncOrderFromDeadLetter();
        }


        public async Task StopProductMsg()
        {
            await productProcessor.StopProcessingAsync();
            await productProcessor.DisposeAsync();
        }
    }
    public partial class AzureServiceBusConsumer
    {
        private async Task OnAutoUpdateProduct(ProcessMessageEventArgs args)
        {
            _logger.LogInformation($"{"On Auto Update Product"}{" | "}{DateTime.UtcNow}");
            var subject = args.Message.Subject;
            if (subject.ToLower() == EventMessages.PRODUCTS_PRODUCT_REFRESHED.ToLower())
            {
                var body = Encoding.UTF8.GetString(args.Message.Body);

                var productMsg = JsonConvert.DeserializeObject<ProductRefreshedMessage>(body);
                _logger.LogInformation($"{"Response from service Bus"}{" | "}{JsonConvert.SerializeObject(productMsg)}");

                using var scope = _scopeFactory.CreateScope();
                _product = scope.ServiceProvider.GetRequiredService<IAsyncRepository<Product>>();
                var productFromDb = await _product.Table.FirstOrDefaultAsync(c => c.Id == productMsg.ProductId);
                if (productFromDb == null)
                {
                    Product mapProduct = new();
                    mapProduct.ProductSapNumber = productMsg.ProductSapNumber;
                    mapProduct.Name = productMsg.Name;
                    mapProduct.Id = productMsg.ProductId;
                    mapProduct.DateRefreshed = productMsg.DateRefreshed;
                    mapProduct.CompanyCode = productMsg.CompanyCode;
                    mapProduct.CountryCode = productMsg.CountryCode;
                    mapProduct.Price = productMsg.Price;
                    mapProduct.UnitOfMeasureCode = productMsg.UnitOfMeasureCode;

                    await _product.AddAsync(mapProduct);
                }
                else
                {
                    //productFromDb = new Product();
                    productFromDb.ProductSapNumber = productMsg.ProductSapNumber;
                    productFromDb.Name = productMsg.Name;
                    //productFromDb.Id = productMsg.ProductId;
                    productFromDb.DateRefreshed = productMsg.DateRefreshed;
                    productFromDb.CompanyCode = productMsg.CompanyCode;
                    productFromDb.CountryCode = productMsg.CountryCode;
                    productFromDb.Price = productMsg.Price;
                    productFromDb.UnitOfMeasureCode = productMsg.UnitOfMeasureCode;
                    productFromDb.DateRefreshed = DateTime.UtcNow;
                    await _product.UpdateAsync(productFromDb);
                }
                await _product.CommitAsync(default);
            }

        }
        private async Task SyncOrderFromDeadLetter()
        {
            var serviceBusClient = new ServiceBusClient(_config.GetValue<string>("MessagingServiceSetting:ConnectionString"));
            var receiverOptions = new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter };
            var receiver = serviceBusClient.CreateReceiver(EventMessages.PRODUCTS_TOPIC, EventMessagesSubscription.PRODUCTS_PRODUCT_REFRESHED, receiverOptions);
            var letter = await receiver.PeekMessagesAsync(int.MaxValue);
            foreach (var item in letter)
            {
                var productMsg = JsonConvert.DeserializeObject<ProductRefreshedMessage>(item.Body.ToString());
                var productFromDb = await _product.Table.FirstOrDefaultAsync(c => c.Id == productMsg.ProductId);
                if (productFromDb == null)
                {
                    Product mapProduct = new();
                    mapProduct.ProductSapNumber = productMsg.ProductSapNumber;
                    mapProduct.Name = productMsg.Name;
                    mapProduct.Id = productMsg.ProductId;
                    mapProduct.DateRefreshed = productMsg.DateRefreshed;
                    mapProduct.CompanyCode = productMsg.CompanyCode;
                    mapProduct.CountryCode = productMsg.CountryCode;
                    mapProduct.Price = productMsg.Price;
                    mapProduct.UnitOfMeasureCode = productMsg.UnitOfMeasureCode;

                    await _product.AddAsync(mapProduct);
                }
                else
                {
                    //productFromDb = new Product();
                    productFromDb.ProductSapNumber = productMsg.ProductSapNumber;
                    productFromDb.Name = productMsg.Name;
                    //productFromDb.Id = productMsg.ProductId;
                    productFromDb.DateRefreshed = productMsg.DateRefreshed;
                    productFromDb.CompanyCode = productMsg.CompanyCode;
                    productFromDb.CountryCode = productMsg.CountryCode;
                    productFromDb.Price = productMsg.Price;
                    productFromDb.UnitOfMeasureCode = productMsg.UnitOfMeasureCode;
                    productFromDb.DateRefreshed = DateTime.UtcNow;
                    await _product.UpdateAsync(productFromDb);
                }
                await _product.CommitAsync(default);

            }
        }
        private async Task OnAutoUpdateDmsOrderFromSapUpdate(ProcessMessageEventArgs args)
        {
            var subject = args.Message.Subject;
            if (subject == EventMessages.SAP_ORDER_UPDATED)
            {
                var message = args.Message;
                var body = Encoding.UTF8.GetString(message.Body);

                var sapOrderMsg = JsonConvert.DeserializeObject<DmsOrderDto>(body);

                var productFromDb = _dmsOrder.Table.FirstOrDefaultAsync(c => c.Id == sapOrderMsg.Id);
                if (productFromDb == null)
                {
                    DmsAlertErrorMessage dmsAlert = new()
                    {
                        DateReported = DateTime.UtcNow,
                        Message = "No order available",
                        Source = "DMSORDER"
                    };
                    await _messageBus.PublishTopicMessage(dmsAlert, EventMessages.ALERT_ERROR);
                }
                else
                {
                    var mapOrder = _mapper.Map<DmsOrder>(sapOrderMsg);
                    mapOrder.DateRefreshed = DateTime.UtcNow;
                    mapOrder.DateCreated = DateTime.UtcNow;

                    await _dmsOrder.UpdateAsync(mapOrder);
                    await _orderItem.CommitAsync(default);

                    var dmsOrderItems = _orderItem.Table.Where(c => c.OrderId == mapOrder.Id).ToList();
                    foreach (var orderItem in dmsOrderItems)
                    {
                        var dmsOrderItem = await _orderItem.Table.FirstOrDefaultAsync(c => c.Id == orderItem.Id);
                        dmsOrderItem.SapPricePerUnit = orderItem.SapPricePerUnit;
                        dmsOrderItem.SapNetValue = orderItem.SapNetValue;
                        dmsOrderItem.SapDeliveryQuality = orderItem.SapDeliveryQuality;
                        await _orderItem.UpdateAsync(dmsOrderItem);
                        await _orderItem.CommitAsync(default);
                    }

                    DmsOrderRefreshedMessage refreshedMessage = new()
                    {
                        DmsOrderId = mapOrder.Id,
                        OrderSapNumber = mapOrder.OrderSapNumber,
                        DateModifed = mapOrder.DateCreated,
                        DateRefreshed = mapOrder.DateRefreshed.Value,
                        ModifiedByUserId = mapOrder.UserId,
                        UserId = mapOrder.UserId,
                        CompanyCode = mapOrder.CompanyCode,
                        CountryCode = mapOrder.CountryCode,
                        DistributorSapAccount = new Application.ViewModels.Responses
                        .DistributorSapAccountResponse(mapOrder.DistributorSapAccountId, mapOrder.DistributorSapAccount.DistributorSapNumber, mapOrder.DistributorSapAccount.DistributorName),
                        EstimatedNetValue = mapOrder.EstimatedNetValue.Value,
                        newOrderSapNetValue = mapOrder.OrderSapNetValue,
                        OrderType = new NewOrderStatusResponse { Code = mapOrder.OrderType.Code, Name = mapOrder.OrderType.Name },
                        DmsOrderItems = JsonConvert.DeserializeObject<List<OrderItemsResponse>>(Convert.ToString(mapOrder.DmsOrderItems))
                    };
                    await _messageBus.PublishTopicMessage(refreshedMessage, EventMessages.ORDER_DMSORDER_REFRESHED);
                }
            }
        }
        private async Task OnAutoDistributorAccount(ProcessMessageEventArgs args)
        {
            _logger.LogInformation($"{"On Auto Distributor Account"}{" | "}{DateTime.UtcNow}");
            var subject = args.Message.Subject;

            if (subject.ToLower() == EventMessages.ACCOUNT_SAPACCOUNT_CREATED.ToLower())
            {
                var message = args.Message;
                var body = Encoding.UTF8.GetString(message.Body);

                var disAccount = JsonConvert.DeserializeObject<AccountsSapAccountCreatedMessage>(body);
                _logger.LogInformation($"{"Response from service Bus"}{" | "}{JsonConvert.SerializeObject(disAccount)}");
                using var scope = _scopeFactory.CreateScope();
                _distributorSapNo = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DistributorSapAccount>>();

                var sapAccountDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c =>
                c.DistributorSapAccountId == disAccount.DistributorSapAccountId && c.UserId == disAccount.UserId);
                if (sapAccountDetail == null)
                {
                    DistributorSapAccount sapAccount = new()
                    {
                        AccountType = disAccount.AccountType.Name,
                        DateRefreshed = DateTime.UtcNow,
                        DistributorName = disAccount.DistributorName,
                        CompanyCode = disAccount.CompanyCode,
                        CountryCode = disAccount.CountryCode,
                        UserId = disAccount.UserId,
                        DistributorSapNumber = disAccount.DistributorSapNumber,
                        DistributorSapAccountId = disAccount.DistributorSapAccountId
                    };

                    await _distributorSapNo.AddAsync(sapAccount);
                    await _distributorSapNo.CommitAsync(default);
                }

            }
            else if (subject == EventMessages.ACCOUNT_SAPACCOUNT_UPDATED)
            {
                var message = args.Message;
                var body = Encoding.UTF8.GetString(message.Body);
                var disAccount = JsonConvert.DeserializeObject<AccountsSapAccountCreatedMessage>(body);

                var sapAccountDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.DistributorSapAccountId == disAccount.DistributorSapAccountId);
                if (sapAccountDetail == null)
                {
                    var sapAccountToCreate = new DistributorSapAccount()
                    {
                        AccountType = disAccount.AccountType.Name,
                        DateRefreshed = disAccount.DateModified,
                        DistributorName = disAccount.DistributorName,
                        CompanyCode = disAccount.CompanyCode,
                        CountryCode = disAccount.CountryCode,
                        UserId = disAccount.UserId,
                        DistributorSapNumber = disAccount.DistributorSapNumber,
                    };
                    using var scope = _scopeFactory.CreateScope();
                    _distributorSapNo = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DistributorSapAccount>>();

                    await _distributorSapNo.AddAsync(sapAccountToCreate);
                    await _distributorSapNo.CommitAsync(default);
                }
                else
                {
                    sapAccountDetail.AccountType = disAccount.AccountType.Name;
                    sapAccountDetail.DateRefreshed = disAccount.DateRefreshed;
                    sapAccountDetail.DistributorName = disAccount.DistributorName;
                    sapAccountDetail.CompanyCode = disAccount.CompanyCode;
                    sapAccountDetail.CountryCode = disAccount.CountryCode;
                    sapAccountDetail.UserId = disAccount.UserId;
                    sapAccountDetail.DistributorSapNumber = disAccount.DistributorSapNumber;
                    using var scope = _scopeFactory.CreateScope();
                    _distributorSapNo = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DistributorSapAccount>>();

                    await _distributorSapNo.UpdateAsync(sapAccountDetail);
                    await _distributorSapNo.CommitAsync(default);
                }
            }
            else if (subject == EventMessages.ACCOUNT_SAP_DELETED)
            {
                var message = args.Message;
                var body = Encoding.UTF8.GetString(message.Body);
                SapAccountDeletedMessage disAccount = JsonConvert.DeserializeObject<SapAccountDeletedMessage>(body);

                using var scope = _scopeFactory.CreateScope();
                _distributorSapNo = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DistributorSapAccount>>();
                _orderItem = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DmsOrderItem>>();
                _dmsOrder = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DmsOrder>>();
                _changeLog = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DmsOrdersChangeLog>>();
                _cartItem = scope.ServiceProvider.GetRequiredService<IAsyncRepository<ShoppingCartItem>>();
                _cart = scope.ServiceProvider.GetRequiredService<IAsyncRepository<ShoppingCart>>();
                _otp = scope.ServiceProvider.GetRequiredService<IAsyncRepository<Otp>>();

                _orderGroup = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DmsOrderGroup>>();

                var sapAccountDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.DistributorSapAccountId == disAccount.SapAccountId && c.DistributorSapNumber == disAccount.DistributorSapNumber
                && c.UserId == disAccount.UserId);
                if (sapAccountDetail != null)
                {
                    List<DmsOrderItem> orderItemsList = new List<DmsOrderItem>();
                    List<DmsOrdersChangeLog> ChangeLogList = new List<DmsOrdersChangeLog>();
                    List<DmsOrderGroup> orderGroups = new List<DmsOrderGroup>();
                    List<Otp> otps = new List<Otp>();
                    List<ShoppingCartItem> shoppingCartItems = new List<ShoppingCartItem>();

                    var dmsOrders = await _dmsOrder.Table.Where(x => x.DistributorSapAccountId == sapAccountDetail.DistributorSapAccountId).ToListAsync();
                    foreach (var dmsOrder in dmsOrders)
                    {
                        var items = await _orderItem.Table.Where(x => x.OrderId == dmsOrder.Id).ToListAsync();
                        orderItemsList.AddRange(items);
                        var changeLogs = await _changeLog.Table.Where(x => x.OrderId == dmsOrder.Id).ToListAsync();
                        ChangeLogList.AddRange(changeLogs);
                        var groups = await _orderGroup.Table.FirstOrDefaultAsync(x => x.Id == dmsOrder.DmsOrderGroupId);
                        orderGroups.Add(groups);
                    }

                    var shoppingCarts = await _cart.Table.Where(x => x.DistributorSapAccountId == sapAccountDetail.DistributorSapAccountId).ToListAsync();
                    foreach (var shoppingCart in shoppingCarts)
                    {
                        var cartItems = await _cartItem.Table.Where(x => x.ShoppingCartId == shoppingCart.Id).ToListAsync();
                        shoppingCartItems.AddRange(cartItems);
                    }
                    foreach (var group in orderGroups)
                    {
                        var otpss = await _otp.Table.Where(x => x.DmsOrderGroupId == group.Id).ToListAsync();
                        otps.AddRange(otpss);
                    }

                    //Removing orderitems
                    _orderItem.DeleteRange(orderItemsList);
                    _changeLog.DeleteRange(ChangeLogList);
                    _dmsOrder.DeleteRange(dmsOrders);
                    _cartItem.DeleteRange(shoppingCartItems);
                    _cart.DeleteRange(shoppingCarts);
                    _otp.DeleteRange(otps);
                    _orderGroup.DeleteRange(orderGroups);

                    //remove the distributors account
                    _distributorSapNo.Delete(sapAccountDetail);
                    await _distributorSapNo.CommitAsync(default);
                }


            }
        }
        private async Task OnAutoInvalidateDMSOrderCache(ProcessMessageEventArgs args)
        {
            var subject = args.Message.Subject;
            if (subject == EventMessages.ORDER_DMS_CREATED)
            {
                var message = args.Message;
                var body = Encoding.UTF8.GetString(message.Body);

                DmsOrder order = JsonConvert.DeserializeObject<DmsOrder>(body);
                var cacheId = $"{CacheKeys.DMS_ORDER_USER_ACCOUNT_ID}{LoggedInUser}{order.Id}";
                await _cache.RemoveAsync(cacheId);
            }
            else if (subject == EventMessages.ORDER_DMS_UPDATED || subject == EventMessages.ORDER_DMSORDER_REFRESHED)
            {
                var message = args.Message;
                var body = Encoding.UTF8.GetString(message.Body);

                DmsOrder order = JsonConvert.DeserializeObject<DmsOrder>(body);
                var cacheId = $"{CacheKeys.DMS_ORDER_USER_ACCOUNT_ID}{LoggedInUser}{order.Id}";
                await _cache.RemoveAsync(cacheId);
            }
        }
        private async Task OnAutoInvalidateSapOrderCache(ProcessMessageEventArgs args)
        {
            var subject = args.Message.Subject;
            if (subject == EventMessages.SAP_ORDER_CREATED)
            {
                var message = args.Message;
                var body = Encoding.UTF8.GetString(message.Body);

                DmsOrder order = JsonConvert.DeserializeObject<DmsOrder>(body);
                var cacheKey = $"{CacheKeys.DMS_ORDER_USER_ACCOUNT_ID}{LoggedInUser}{order.OrderSapNumber}";
                var cacheKey1 = $"{CacheKeys.DMS_ORDER_USER_ACCOUNT_ID}{LoggedInUser}{order.DistributorSapAccount.DistributorSapNumber}";
                await _cache.RemoveAsync(cacheKey);
                await _cache.RemoveAsync(cacheKey1);
            }
            else if (subject == EventMessages.SAP_ORDER_UPDATED)
            {
                var message = args.Message;
                var body = Encoding.UTF8.GetString(message.Body);

                DmsOrder order = JsonConvert.DeserializeObject<DmsOrder>(body);
                var cacheKey = $"{CacheKeys.DMS_ORDER_USER_ACCOUNT_ID}{LoggedInUser}{order.OrderSapNumber}";
                var cacheKey1 = $"{CacheKeys.DMS_ORDER_USER_ACCOUNT_ID}{LoggedInUser}{order.DistributorSapAccount.DistributorSapNumber}";
                await _cache.RemoveAsync(cacheKey);
                await _cache.RemoveAsync(cacheKey1);
            }
        }

    }
}
