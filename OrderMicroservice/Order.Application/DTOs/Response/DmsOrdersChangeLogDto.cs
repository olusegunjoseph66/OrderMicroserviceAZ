using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class DmsOrdersChangeLogDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string ChangeType { get; set; } = null!;
        public DateTime OldDateCreated { get; set; }
        public int OldCreatedByUserId { get; set; }
        public DateTime? OldDateModified { get; set; }
        public int? OldModifiedByUserId { get; set; }
        public int OldUserId { get; set; }
        public string OldOrderSapNumber { get; set; } = null!;
        public string OldCompanyCode { get; set; } = null!;
        public string OldCountryCode { get; set; } = null!;
        public int OldDistributorSapAccountId { get; set; }
        public int OldOrderStatusId { get; set; }
        public int OldOrderTypeId { get; set; }
        public int OldShoppingCartId { get; set; }
        public decimal? OldEstimatedNetValue { get; set; }
        public decimal? OldOrderSapNetValue { get; set; }
        public string OldTruckSizeCode { get; set; }
        public string OldDeliveryMethodCode { get; set; }
        public string OldPlantCode { get; set; }
        public DateTime? OldDeliveryDate { get; set; }
        public string OldDeliveryAddress { get; set; }
        public string OldDeliveryCity { get; set; }
        public string OldDeliveryStateCode { get; set; }
        public string OldDeliveryCountryCode { get; set; }
        public string OldChannelCode { get; set; } = null!;
        public DateTime? OldDateRefreshed { get; set; }
        public DateTime? OldDateSubmittedOnDms { get; set; }
        public DateTime? OldDateSubmittedToSap { get; set; }
        public int? OldPlantId { get; set; }
        public int? OldNumberOfSapsubmissionAttempts { get; set; }
        public DateTime NewDateCreated { get; set; }
        public int NewCreatedByUserId { get; set; }
        public DateTime? NewDateModified { get; set; }
        public int? NewModifiedByUserId { get; set; }
        public int NewUserId { get; set; }
        public string NewOrderSapNumber { get; set; } = null!;
        public string NewCompanyCode { get; set; } = null!;
        public string NewCountryCode { get; set; } = null!;
        public int NewDistributorSapAccountId { get; set; }
        public int NewOrderStatusId { get; set; }
        public int NewOrderTypeId { get; set; }
        public int NewShoppingCartId { get; set; }
        public decimal? NewEstimatedNetValue { get; set; }
        public decimal? NewOrderSapNetValue { get; set; }
        public string NewTruckSizeCode { get; set; }
        public string NewDeliveryMethodCode { get; set; }
        public string NewPlantCode { get; set; }
        public DateTime? NewDeliveryDate { get; set; }
        public string NewDeliveryAddress { get; set; }
        public string NewDeliveryCity { get; set; }
        public string NewDeliveryStateCode { get; set; }
        public string NewDeliveryCountryCode { get; set; }
        public string NewChannelCode { get; set; } = null!;
        public DateTime? NewDateRefreshed { get; set; }
        public DateTime? NewDateSubmittedOnDms { get; set; }
        public DateTime? NewDateSubmittedToSap { get; set; }
        public int? NewPlantId { get; set; }
        public int? NewNumberOfSapsubmissionAttempts { get; set; }
    }
}
