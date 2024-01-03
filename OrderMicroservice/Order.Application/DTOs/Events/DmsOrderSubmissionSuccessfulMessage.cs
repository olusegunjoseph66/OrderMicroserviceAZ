using Order.Application.ViewModels.Responses;
using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class DmsOrderSubmissionSuccessfulMessage: IntegrationBaseMessage
    {
        public int DmsOrderId { get; set; }
        public string OrderSapNumber { get; set; }
        public DateTime DateModified { get; set; }
        public DateTime DateSubmittedOnDMS { get; set; }
        public DateTime DateSubmittedToSap { get; set; }
        public int UserId { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public FailOrderDistributorSapAccount DistributorSapAccount { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        public decimal? OrderSapNetValue { get; set; }
        public StatusRep OrderStatus { get; set; }
        public StatusRep OrderType { get; set; }
        public string TruckSizeCode { get; set; }
        public string DeliveryMethodCode { get; set; }
        public FailOrdersPlant Plant { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryStateCode { get; set; }
        public string DeliveryCountryCode { get; set; }
        public int? NumberOfSubmissionAttempts { get; set; }
        public List<FailSubmissionOrderItemsResponse> DmsOrderItems { get; set; }

    }
}
