using Aspose.Pdf;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Order.Application.Constants;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Filters;
using Order.Application.DTOs.Request;
using Order.Application.DTOs.Response;
using Order.Application.DTOs.Sortings;
using Order.Application.Enums;
using Order.Infrastructure.QueryObjects;
using Shared.Data.Extensions;
using Shared.Data.Models;
using Shared.Utilities.DTO.Pagination;
using Shared.Utilities.Helpers;
using System.Data;

namespace Order.Infrastructure.Services
{
    public partial class OrderService
    {
        public async Task<ApiResponse> AdminGetDmsOrder(AdminDmsOrderQueryRequestDTO query)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order"}{" | "}{LoggedInUser()}{" | "}{DateTime.Now}");

            List<DmsOrder> redisDmsOrderList = new();
            _orderLogger.LogInformation($"{"DMS Order Cache Response:-"}{" | "}{JsonConvert.SerializeObject(redisDmsOrderList)}");

            if (redisDmsOrderList == null || redisDmsOrderList.Count == 0)
            {
                redisDmsOrderList = await _dmsOrder.Table.Include(c => c.OrderStatus).Include(c => c.OrderType)
                    .Include(c => c.DistributorSapAccount).Include(c => c.DeliveryStatus).Where(x => x.IsAtc == query.IsATC && !string.IsNullOrEmpty(x.ChannelCode)).ToListAsync();
            }
            BasePageFilter pageFilter = new(query.PageSize, query.PageIndex);
            RequestSortingDto sorting = new();
            ConstructSorting(query.Sort, ref sorting);

            var requestFilter = new RequestFilterDto(query.CountryCode, query.CompanyCode, query.SearchKeyword, "",
               0, query.OrderStatusCode, query.IsATC, query.FromDate, query.ToDate, query.OrderTypeCode, userId: query.UserId);

            var expression = new DmsOrderDistrubutorQueryObject(requestFilter).Expression;
            var orderExpression = ProcessOrderFunc(sorting);

            var orderItems = redisDmsOrderList.AsQueryable().AsNoTrackingWithIdentityResolution()
                .OrderByWhere(expression, orderExpression);


            var totalCount = orderItems.Count();
            var totalPages = NumberManipulator.PageCountConverter(totalCount, query.PageSize);

            var queryResult = orderItems.Paginate(pageFilter.PageNumber, pageFilter.PageSize);

            var orderList = queryResult.ToList();
            if (orderList == null || orderList.Count == 0)
                return ResponseHandler.SuccessResponse("No record found...");
            var mapRecord = orderList.Select(c => new
            {
                dmsorderId = c.Id,
                dmsOrderGroupId = c.DmsOrderGroupId,
                dateCreated = c.DateCreated,
                companyCode = c.CompanyCode,
                orderSapNumber = c.OrderSapNumber,
                parentOrderSapNumber = c.ParentOrderSapNumber,
                userId = c.UserId,
                countryCode = c.CountryCode,
                orderStatus = new OrderStatusDto { Code = c?.OrderStatus?.Code, Name = c?.OrderStatus?.Name },
                orderType = new OrderTypeDto { Code = c.OrderType?.Code, Name = c?.OrderType.Name },
                deliveryStatus = new DeliveryStatusDto { Code = c?.DeliveryStatus?.Code, Name = c?.DeliveryStatus?.Name },
                isATC = c.IsAtc,
                estimatedNetvalue = c.EstimatedNetValue,
                orderSapNetValue = c.OrderSapNetValue,
                channelCode = c.ChannelCode,
                //One of the recent changes
                numItems = c.DmsOrderItems.Count,
                distributorsSapAccount = new
                {
                    distrobutorSapAccountId = c.DistributorSapAccount.DistributorSapAccountId,
                    distributorName = c.DistributorSapAccount.DistributorName,
                    distributorSapNumber = c.DistributorSapAccount.DistributorSapNumber
                }

            }).ToList();
            var response = new PaginatedListVM<object>(mapRecord,
                new PaginationMetaData(query.PageIndex, query.PageSize, totalPages, totalCount));
            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_ORDER_LIST_RETRIEVAL,
                new { pagination = response.Pagination, dmsOrders = response.Items });
        }

        public async Task<ApiResponse> ExportDmsOrder(ExportDmsOrderFilterDto filter)
        {
            _orderLogger.LogInformation($"{"About to retrieve DMS Order"}{" | "}{LoggedInUser()}{" | "}{DateTime.UtcNow}");
            List<DmsOrder> redisDmsOrderList = new();


            if (redisDmsOrderList == null || redisDmsOrderList.Count == 0)
            {
                redisDmsOrderList = await _dmsOrder.Table.Include(c => c.OrderStatus).Include(c => c.OrderType)
                   .Include(c => c.DistributorSapAccount).Include(c => c.DeliveryStatus).ToListAsync();
            }

            RequestSortingDto sorting = new();
            ConstructSorting(filter.Sort, ref sorting);

            var requestFilter = new RequestFilterDto(filter.CountryCode, filter.CompanyCode, filter.SearchKeyword, "",
               0, filter.OrderStatusCode, filter.IsATC, filter.FromDate, filter.ToDate, filter.OrderTypeCode);

            var expression = new DmsOrderDistrubutorQueryObject(requestFilter).Expression;
            var orderExpression = ProcessOrderFunc(sorting);

            var orderItems = redisDmsOrderList.AsQueryable().AsNoTrackingWithIdentityResolution()
                .OrderByWhere(expression, orderExpression);

            var orderList = orderItems.ToList();
            var response = orderList.Select(c => new GetAdminDMSOrderDto()
            {
                Id = c.Id,
                DateCreated = c.DateCreated,
                CompanyCode = c.CompanyCode,
                OrderSapNumber = c.OrderSapNumber,
                ParentOrderSapNumber = c.ParentOrderSapNumber,
                UserId = c.UserId,
                CountryCode = c.CountryCode,
                OrderStatus = new OrderStatusDto { Code = c?.OrderStatus?.Code, Name = c?.OrderStatus?.Name },
                OrderType = new OrderTypeDto { Code = c.OrderType?.Code, Name = c?.OrderType.Name },
                DeliveryStatus = new DeliveryStatusDto { Code = c?.DeliveryStatus?.Code, Name = c?.DeliveryStatus?.Name },
                IsAtc = c.IsAtc,
                EstimatedNetValue = c.EstimatedNetValue,
                OrderSapNetValue = c.OrderSapNetValue,
                distributorSapAccount = new DistributorSapAccountDto() { DistributorName = c.DistributorSapAccount.DistributorName, DistributorSapNumber = c.DistributorSapAccount.DistributorSapNumber }

            }).ToList();

            string contentType = String.Empty;
            string fileName = String.Empty;
            byte[] content = null;
            switch (filter.Format)
            {
                case ExportFileTypeEnum.Xls:
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = "DmsOrder_Data_Export_Data_Export.xlsx";
                    content = GenerateExcelWorksheet(response);
                    break;
                case ExportFileTypeEnum.Csv:
                    contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    fileName = "DmsOrder_Data_Export.csv";
                    content = GenerateExcelWorksheet(response);
                    break;
                case ExportFileTypeEnum.PdfFile:
                    contentType = "application/pdf";
                    fileName = "DmsOrder_Data_Export.pdf";
                    content = GenerateExcelWorksheet(response, true);
                    break;
                default:
                    break;
            }
            return ResponseHandler.SuccessResponse(SuccessMessages.SUCCESSFUL_REQUEST_RETRIEVAL,
                new { file = content });
        }
        Byte[] GenerateExcelWorksheet(List<GetAdminDMSOrderDto> results, bool isPdf = false)
        {
            if (isPdf)
            {
                DataTable dataTable = new("DmsOrderDetails");
                dataTable.Columns.AddRange(new DataColumn[8] { new DataColumn("Id"),
                                            new DataColumn("Distributor Name"),
                                            new DataColumn("Distributor SapNumber"),
                                            new DataColumn("Company Code"),
                                            new DataColumn("Country Code"),
                                            new DataColumn("Order SapNetValue"),
                                            new DataColumn("Estimated NetValue"),
                                            new DataColumn("Date Created") });
                foreach (var item in results)
                {
                    dataTable.Rows.Add(item.Id, item.distributorSapAccount.DistributorName,
                        item.distributorSapAccount.DistributorSapNumber,
                        item.CompanyCode,
                        item.CountryCode,
                        item.OrderSapNetValue,
                        item.EstimatedNetValue,
                        item.DateCreated);
                }

                var document = new Document
                {
                    PageInfo = new PageInfo { Margin = new MarginInfo(28, 28, 28, 30) }
                };
                var pdfPage = document.Pages.Add();
                Table table = new()
                {
                    ColumnWidths = "7% 16% 16% 16% 10% 16% 15%",
                    DefaultCellPadding = new MarginInfo(10, 5, 10, 5),
                    Border = new BorderInfo(BorderSide.All, .5f, Color.Black),
                    DefaultCellBorder = new BorderInfo(BorderSide.All, .5f, Color.Black),
                };

                table.ImportDataTable(dataTable, true, 0, 0);
                document.Pages[1].Paragraphs.Add(table);

                using var memoryStream = new MemoryStream();
                document.Save(memoryStream);
                return memoryStream.ToArray();
            }
            else
            {
                using XLWorkbook workbook = new();
                IXLWorksheet worksheet = workbook.Worksheets.Add("DmsOrderLists");
                worksheet.Cell(1, 1).Value = "Id";
                worksheet.Cell(1, 2).Value = "Distributor Name";
                worksheet.Cell(1, 3).Value = "Distributor SapNumber";
                worksheet.Cell(1, 4).Value = "Company Code";
                worksheet.Cell(1, 5).Value = "Country Code";
                worksheet.Cell(1, 6).Value = "Order SapNetValue";
                worksheet.Cell(1, 7).Value = "Estimated NetValue";
                worksheet.Cell(1, 8).Value = "Date Created";

                for (int index = 1; index <= results.Count; index++)
                {
                    worksheet.Cell(index + 1, 1).Value = results[index - 1].Id;
                    worksheet.Cell(index + 1, 2).Value = results[index - 1].distributorSapAccount.DistributorName;
                    worksheet.Cell(index + 1, 3).Value = results[index - 1].distributorSapAccount.DistributorSapNumber;
                    worksheet.Cell(index + 1, 4).Value = results[index - 1].CompanyCode;
                    worksheet.Cell(index + 1, 5).Value = results[index - 1].CountryCode;
                    worksheet.Cell(index + 1, 6).Value = results[index - 1].OrderSapNetValue;
                    worksheet.Cell(index + 1, 7).Value = results[index - 1].EstimatedNetValue;
                    worksheet.Cell(index + 1, 8).Value = results[index - 1].DateCreated;
                }
                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                return stream.ToArray();
            }

        }
    }
}
