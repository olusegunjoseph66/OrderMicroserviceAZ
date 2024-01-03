using Order.Application.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Request
{
    public class DmsOrderQueryRequestDTO
    {
        public string CountryCode { get; set; }
        public string OrderStatusCode { get; set; }
        public string DeliveryMethodCode { get; set; }
        //[Required(ErrorMessage = "Distributor Sap Account-Id")]
        public string DistributorSapAccountId { get; set; }
        public string CompanyCode { get; set; }
        public string SearchKeyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [Required(ErrorMessage = "Is ATC")]
        public bool IsATC { get; set; }
        [Required(ErrorMessage = "Page Index")]
        public int PageIndex { get; set; }
        [Required(ErrorMessage = "Page Size")]
        public int PageSize { get; set; }
        //[Required(ErrorMessage = "Sort")]
        public OrderSortingEnum Sort { get; set; }
    }

    public class AdminDmsOrderQueryRequestDTO
    {
        public int UserId { get; set; }
        public string CountryCode { get; set; }
        public string OrderTypeCode { get; set; }
        public string OrderStatusCode { get; set; }
        public string CompanyCode { get; set; }
        //public string ChannelCode { get; set; }
        public string SearchKeyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [Required]
        public bool IsATC { get; set; } = false;
        [Required(ErrorMessage = "Page Index")]
        public int PageIndex { get; set; }
        [Required(ErrorMessage = "Page Size")]
        public int PageSize { get; set; }
        [Required(ErrorMessage = "Sort")]
        public OrderSortingEnum Sort { get; set; }
    }

    public class DmsOrderBySapAccountIdQueryRequestDTO
    {
        
        [Required(ErrorMessage = "Distributor Sap Account-Id")]
        public string DistributorSapAccountId { get; set; }
        public string OrderStatusCode { get; set; }
        //public string SearchKeyword { get; set; }
        //public string CountryCode { get; set; }
        //public string DeliveryMethodCode { get; set; }
        //public string CompanyCode { get; set; }
        public string OrderTypeCode { get; set; }
        [Required]
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        [Required(ErrorMessage = "Page Index")]
        public int PageIndex { get; set; }
        [Required(ErrorMessage = "Page Size")]
        public int PageSize { get; set; }
        //[Required(ErrorMessage = "Sort")]
        public OrderSortingEnum Sort { get; set; }
    }

    public class DmsOrderSapChildByQueryRequestDTO
    {

        [Required(ErrorMessage = "Distributor Sap Account-Id")]
        public int? DistributorSapAccountId { get; set; }
        [Required(ErrorMessage = "Order Sap Number")]
        public string OrderSapNumber { get; set; }
        [Required(ErrorMessage = "Page Index")]
        public int PageIndex { get; set; }
        [Required(ErrorMessage = "Page Size")]
        public int PageSize { get; set; }
        //[Required(ErrorMessage = "Sort")]
        public OrderSortingEnum Sort { get; set; }
    }
}
