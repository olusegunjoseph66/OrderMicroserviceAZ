using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class DmsOrderItem
    {
        public int Id { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateModified { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string? OrderItemSapNumber { get; set; }
        public decimal Quantity { get; set; }
        public string SalesUnitOfMeasureCode { get; set; } = null!;
        public int ProductId { get; set; }
        public decimal? SapPricePerUnit { get; set; }
        public decimal? SapNetValue { get; set; }
        public decimal? SapDeliveryQuality { get; set; }

        public virtual DmsOrder Order { get; set; } = null!;
        public virtual Product Product { get; set; } = null!;
    }
}
