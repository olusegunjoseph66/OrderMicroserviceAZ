using Account.Application.Constants;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Order.Application.Constants;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Events;
using Order.Application.Interfaces.Services;
using Shared.Data.Models;
using Shared.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Infrastructure.Services
{
    public class DeadLetterService : IDeadLetterService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<DeadLetterService> _deadLetterLogger;
        private IAsyncRepository<Product> _product;


        public DeadLetterService(IConfiguration config, ILogger<DeadLetterService> deadLetterLogger, IAsyncRepository<Product> product)
        {
            _config = config;
            _deadLetterLogger = deadLetterLogger;
            _product = product;
        }

        public async Task<ApiResponse> ProcessAccountDeadLetterMessages(CancellationToken cancellationToken = default)
        {
            _deadLetterLogger.LogInformation("About to start dead-letter service");
            var serviceBusClient = new ServiceBusClient(_config.GetValue<string>("MessagingServiceSetting:ConnectionString"));
            var receiverOptions = new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter };
            var receiver = serviceBusClient.CreateReceiver(EventMessages.PRODUCTS_TOPIC, EventMessagesSubscription.PRODUCTS_PRODUCT_REFRESHED, receiverOptions);
            var letter = await receiver.PeekMessagesAsync(20);
          
            foreach (var item in letter)
            {
                try
                {
                    var productMsg = JsonConvert.DeserializeObject<ProductRefreshedMessage>(item.Body.ToString());
                    _deadLetterLogger.LogInformation($"Product Details = {JsonConvert.SerializeObject(productMsg)}");
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

                       // await receiver.CompleteMessageAsync(item);
                    }
                    else
                    {
                        //productFromDb = new Product();
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
                    //await _product.CommitAsync(default);
                    //await receiver.CompleteMessageAsync(item);
                }
                catch (Exception ex)
                {
                    _deadLetterLogger.LogError($"{ex.Message}");
                }
            }
            _deadLetterLogger.LogInformation($"dead-letter service ended with {letter.Count} processed messages.");
            return ResponseHandler.SuccessResponse("Operation Completed Successfully");

        }
    }
}
