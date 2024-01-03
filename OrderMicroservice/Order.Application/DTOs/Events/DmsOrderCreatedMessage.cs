using Order.Application.ViewModels.Requests;
using Order.Application.ViewModels.Responses;
using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vm.ViewModels.Responses;

namespace Order.Application.DTOs.Events
{
    public class DmsOrderCreatedMessage : IntegrationBaseMessage
    {
        public int DmsOrderId { get; set; }
        public int CreatedByUserId { get; set; }
        public int UserId { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public DistributorSapAccountResponse DistributorSapAccount { get; set; }
        public decimal EstimatedNetValue { get; set; }
        public string SalesUnitOfMeasureCode { get; set; }
        public Status OrderStatus { get; set; }
        public Status OrderType { get; set; }
        public string ChannelCode { get; set; }
        public List<OrderItemsResponse> DmsOrderItems { get; set; }

         
    }
}
