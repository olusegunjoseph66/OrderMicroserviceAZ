using Order.Application.ViewModels.Responses;
using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class DmsOrderUpdatedMessage : IntegrationBaseMessage
    {
        public int DmsOrderId { get; set; }
        public DateTime? DateModified { get; set; }
        public int ModifiedByUserId { get; set; }
        public int UserId { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public string OrderSapNumber { get; set; }
        public DistributorSapAccountResponse DistributorSapAccount { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        public OldOrderStatusResponse OldOrderStatus { get; set; }
        public NewOrderStatusResponse NewOrderStatus { get; set; }
        public NewOrderStatusResponse OrderType { get; set; }
        public string OldtruckSizeCode { get; set; }
        public string NewtruckSizeCode { get; set; }
        public string OldDeliveryMethodCode { get; set; }
        public string NewDeliveryMethodCode { get; set; }
        public PlantResponse Oldplant { get; set; }
        public PlantResponse Newplant { get; set; }
        public DateTime? OldDeliveryDate { get; set; }
        public DateTime? NewDeliveryDate { get; set; }
        public string OldDeliveryAddress { get; set; }
        public string NewDeliveryAddress { get; set; }
        public string OldDeliveryCity { get; set; }
        public string NewDeliveryCity { get; set; }
        public string OldDeliveryStateCode { get; set; }
        public string NewDeliveryStateCode { get; set; }
        public string OldDeliveryCountryCode { get; set; }
        public string NewDeliveryCountryCode { get; set; }

        //public int ShoppingCartId { get; set; }
        //public string OldShoppingCartStatus { get; set; }
        //public string NewShoppingCartStatus { get; set; }
        public List<OrderItemsResponse> DmsOrderItems { get; set; } = default;
    }
}
