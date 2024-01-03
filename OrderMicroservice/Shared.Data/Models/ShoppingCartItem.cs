using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class ShoppingCartItem
    {
        public int Id { get; set; }
        public int ShoppingCartId { get; set; }
        public int ProductId { get; set; }
        public int? DistributorSapAccountId { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasureCode { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DateModified { get; set; }
        public int? ModifiedByUserId { get; set; }
        public string ChannelCode { get; set; } = null!;
        public string? PlantCode { get; set; }
        public string? DeliveryMethodCode { get; set; }
        public decimal? SapEstimatedOrderValue { get; set; }
        public DateTime? DateOfOrderEstimate { get; set; }
        public string? DeliveryCountryCode { get; set; }
        public string? DeliveryStateCode { get; set; }

        public virtual DistributorSapAccount? DistributorSapAccount { get; set; }
        public virtual Product Product { get; set; } = null!;
        public virtual ShoppingCart ShoppingCart { get; set; } = null!;
    }
}
