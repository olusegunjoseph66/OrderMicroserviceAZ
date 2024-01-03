using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public class ReportByProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSapNumber { get; set; }
        public decimal Quantity { get; set; }
        public StatusRep SalesUnitOfMeasure { get; set; }
        public OrderDms dmsOrder { get; set; }
    }


    public class StatusRep
    {
        public string Code { get; set;}
        public string Name { get; set;}
    }

    public class OrderDms
    {
        public string SAPOrderNumber { get; set; }
        public int DmsOrderId { get; set;}
        public DateTime DateCreated { get; set;}
        public string CompanyCode { get; set;}
        public string CountryCode { get; set;}
        public StatusRep OrderStatus { get; set;}
        public StatusRep OrderType { get; set;}
        public bool IsATC { get; set;}
        public decimal? EstimatedNetvalue { get; set;}
        public decimal? OrderSapNetValue { get; set;}
        public int? DmsOrderGroupId { get; set;}
    }
}
