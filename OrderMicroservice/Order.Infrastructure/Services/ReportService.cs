using Azure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Order.Application.Constants;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Request;
using Order.Application.Exceptions;
using Order.Application.Interfaces.Services;
using Order.Application.SerilogService;
using Shared.Data.Models;
using Shared.Data.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Infrastructure.Services
{
    public class ReportService : BaseService, IReportService
    {
        private readonly ILogger<OrderService> _orderLogger;
        private readonly IAsyncRepository<DistributorSapAccount> _distributorSapNo;
        private readonly IAsyncRepository<Product> _product;

        public ReportService(ILogger<OrderService> orderLogger, 
            IAsyncRepository<DistributorSapAccount> _distributorSapNo, 
            IAsyncRepository<Product> _product, IAuthenticatedUserService authenticatedUserService) : base(authenticatedUserService)
        {
            _orderLogger = orderLogger;
            this._distributorSapNo = _distributorSapNo;
            this._product = _product;
        }

        public async Task<ApiResponse> GetMyReportByProduct(ProductReportRequestVM productReport)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order for Distributor Account ID"}{" | "}{LoggedInUser()}{" | "}{productReport.DistributorSapAccountId}{" | "}{DateTime.UtcNow}");

            var sapDetail = await _distributorSapNo.Table.FirstOrDefaultAsync(c => c.UserId == LoggedInUser() && c.DistributorSapAccountId == Convert.ToInt32(productReport.DistributorSapAccountId));
            _orderLogger.LogInformation($"{"SAP Distributor DB Response:-"}{" | "}{JsonConvert.SerializeObject(sapDetail)}");

            if (sapDetail == null)
                throw new NotFoundException(ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Key, ErrorCodes.INVALID_OR_NOTFOUND_DISTRIBUTOR_NUMBER.Value);
            if (sapDetail.CompanyCode == "100")
                throw new NotFoundException(ErrorCodes.ORDER_REPORT_GENERATION_ERROR.Key, ErrorCodes.ORDER_REPORT_GENERATION_ERROR.Value);

            var product = await _product.Table.FirstOrDefaultAsync(x => x.Id == productReport.ProductId);
            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL, new { sapOrders = product });

        }
    }
}
