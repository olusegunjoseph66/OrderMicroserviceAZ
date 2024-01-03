using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class DmsOrderDto
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? SapDateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public int? ModifiedByUserId { get; set; }
        public int UserId { get; set; }
        public string OrderSapNumber { get; set; }
        public string ParentOrderSapNumber { get; set; }
        public bool IsAtc { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public int DistributorSapAccountId { get; set; }
        public byte OrderStatusId { get; set; }
        public byte OrderTypeId { get; set; }
        public int ShoppingCartId { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        public decimal? OrderSapNetValue { get; set; }
        public string TruckSizeCode { get; set; }
        public string DeliveryMethodCode { get; set; }
        public string PlantCode { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryStateCode { get; set; }
        public string DeliveryCountryCode { get; set; }
        public string DeliveryBlockCode { get; set; }
        public string ChannelCode { get; set; } = null!;
        public DateTime? DateRefreshed { get; set; }
        public DateTime? DateSubmittedOnDms { get; set; }
        public DateTime? DateSubmittedToSap { get; set; }
        public int? NumberOfSapsubmissionAttempts { get; set; }
        public decimal? SapVat { get; set; }
        public decimal? SapFreightCharges { get; set; }
        public int? DeliveryStatusId { get; set; }
        public string DeliverySapNumber { get; set; }
        public string SapTripNumber { get; set; }
        public int? PlantId { get; set; }
    }
}
