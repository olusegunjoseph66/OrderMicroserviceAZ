using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class DmsOrder
    {
        public DmsOrder()
        {
            DmsOrderItems = new HashSet<DmsOrderItem>();
            DmsOrdersChangeLogs = new HashSet<DmsOrdersChangeLog>();
            Otps = new HashSet<Otp>();
        }

        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? SapDateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public int? ModifiedByUserId { get; set; }
        public int UserId { get; set; }
        public string? OrderSapNumber { get; set; }
        public string? ParentOrderSapNumber { get; set; }
        public bool IsAtc { get; set; }
        public string CompanyCode { get; set; } = null!;
        public string CountryCode { get; set; } = null!;
        public int DistributorSapAccountId { get; set; }
        public byte OrderStatusId { get; set; }
        public byte OrderTypeId { get; set; }
        public int ShoppingCartId { get; set; }
        public decimal? EstimatedNetValue { get; set; }
        public decimal? OrderSapNetValue { get; set; }
        public string? TruckSizeCode { get; set; }
        public string? DeliveryMethodCode { get; set; }
        public string? PlantCode { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? DeliveryAddress { get; set; }
        public string? DeliveryCity { get; set; }
        public string? DeliveryStateCode { get; set; }
        public string? DeliveryCountryCode { get; set; }
        public string? DeliveryBlockCode { get; set; }
        public string ChannelCode { get; set; } = null!;
        public DateTime? DateRefreshed { get; set; }
        public DateTime? DateSubmittedOnDms { get; set; }
        public DateTime? DateSubmittedToSap { get; set; }
        public int? NumberOfSapsubmissionAttempts { get; set; }
        public decimal? SapVat { get; set; }
        public decimal? SapFreightCharges { get; set; }
        public int? DeliveryStatusId { get; set; }
        public string? DeliverySapNumber { get; set; }
        public string? SapTripNumber { get; set; }
        public int? PlantId { get; set; }
        public string? DeliveryStatusCode { get; set; }
        public string? TripStatusCode { get; set; }
        public DateTime? TripDispatchDate { get; set; }
        public int? TripOdometerStart { get; set; }
        public int? TripOdometerEnd { get; set; }
        public int? NumberOfChildAtc { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public string? SapReference { get; set; }
        public int? DmsOrderGroupId { get; set; }
        public string? CustomerPaymentReference { get; set; }
        public DateTime? CustomerPaymentDate { get; set; }
        public string? WayBillNumber { get; set; }
        public virtual DeliveryStatus? DeliveryStatus { get; set; }
        public virtual DistributorSapAccount DistributorSapAccount { get; set; } = null!;
        public virtual DmsOrderGroup? DmsOrderGroup { get; set; }
        public virtual OrderStatus OrderStatus { get; set; } = null!;
        public virtual OrderType OrderType { get; set; } = null!;
        public virtual Plant? Plant { get; set; }
        public virtual ShoppingCart ShoppingCart { get; set; } = null!;
        public virtual ICollection<DmsOrderItem> DmsOrderItems { get; set; }
        public virtual ICollection<DmsOrdersChangeLog> DmsOrdersChangeLogs { get; set; }
        public virtual ICollection<Otp> Otps { get; set; }
    }
}
