using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Filters
{
    public class RequestFilterDto
    {
        public RequestFilterDto(string countryCode, string companyCode, string searchKeyword,
            string deliveryMethodCode, int distributorSapAccountId,
            string orderStatusCode, bool isATC, DateTime? fromDate, DateTime? toDate, string orderTypeCode = "", string orderSapNumber = "", int userId = 0)
        {
            CountryCode = countryCode;
            CompanyCode = companyCode;
            SearchKeyword = searchKeyword;
            DeliveryMethodCode = deliveryMethodCode;
            DistributorSapAccountId = distributorSapAccountId;
            OrderStatusCode = orderStatusCode;
            IsATC = isATC;
            FromDate = fromDate;
            ToDate = toDate;
            OrderTypeCode = orderTypeCode;
            OrderSapNumber = orderSapNumber;
            UserId = userId;
        }

        public string CountryCode { get; set; }
        public string CompanyCode { get; set; }
        public string SearchKeyword { get; set; }
        public int UserId { get; set; }
        public string DeliveryMethodCode { get; set; }
        public int DistributorSapAccountId { get; set; }
        public string OrderStatusCode { get; set; }
        public string OrderTypeCode { get; set; }
        public bool IsATC { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string OrderSapNumber { get; set; }
    }
}
