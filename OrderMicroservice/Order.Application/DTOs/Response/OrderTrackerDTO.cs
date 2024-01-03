using Shared.ExternalServices.DTOs.APIModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Appplition.DTOs
{
    public class OrderTrackerDTO
    {
        public int dmsOrderId { get; set; }
        public string orderSapNumber { get; set; }
        public string parentOrderSapNumber { get; set; }
        public bool isATC { get; set; }
        public string overalStatusMessage { get; set; }
        public DateTime? lastUpdated { get; set; }
        public Status orderStatus { get; set; }
        public Status deliveryStatus { get; set; }
        //public Status tripStatus { get; set; }
        public SapDeliveryDto delivery { get; set; } = null;
        public SapTripDto sapTrip { get; set; } = null;
    }

    public class SapDeliveryDto
    {
        public string Id { get; set; }
        public DateTime? deliveryDate { get; set; }
        public DateTime? pickUpDate { get; set; }
        public DateTime? loadingDate { get; set; }
        public DateTime? transportDate { get; set; }
        public DateTime? plannedGoodsMovementDate { get; set; }
        public string WayBillNumber { get; set; }
    }

    public class SapTripDto
    {
        public string Id { get; set; }
        public string DeliveryId { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DispatchDate { get; set; }
        public string TruckLocation { get; set; }
        public Status TripStatus { get; set; }
    }
}
