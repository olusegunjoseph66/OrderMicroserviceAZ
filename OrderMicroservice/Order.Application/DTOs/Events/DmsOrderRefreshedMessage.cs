using Order.Application.ViewModels.Responses;
using Shared.ExternalServices.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Events
{
    public class DmsOrderRefreshedMessage : IntegrationBaseMessage
    {
        public int DmsOrderId { get; set; }
        public string OrderSapNumber { get; set; }
        public DateTime DateModifed { get; set; }
        public DateTime DateRefreshed { get; set; }
        public int ModifiedByUserId { get; set; }
        public int UserId { get; set; }
        public string CompanyCode { get; set; }
        public string CountryCode { get; set; }
        public DistributorSapAccountResponse DistributorSapAccount { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        public decimal? oldOrderSapNetValue { get; set; }
        public decimal? newOrderSapNetValue { get; set; }
        public OldOrderStatusResponse OldOrderStatus { get; set; }
        public NewOrderStatusResponse newOrderStatus { get; set; }
        public NewOrderStatusResponse OrderType { get; set; }
        public NewOrderStatusResponse OldDelivery { get; set; }
        public NewOrderStatusResponse newDelivery { get; set; }
        public TripResponse OldTrip { get; set; } = null;
        public TripResponse NewTrip { get; set; } = null;
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

        public List<OrderItemsResponse> DmsOrderItems { get; set; } = new List<OrderItemsResponse>();
    }

    public class TripResponse
    {
        public string TripSapNumber { get; set; }
        public NewOrderStatusResponse TripStatus { get; set; }
        public DateTime DispatchDate { get; set; }
        public int OdometerStart { get; set; }
        public int OdometerEnd { get; set; }
    }

    public class PlantResponse
    {
        public int PlantId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
    }
}
