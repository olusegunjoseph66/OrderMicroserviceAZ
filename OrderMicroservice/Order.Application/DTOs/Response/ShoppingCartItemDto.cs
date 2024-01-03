using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class ShoppingCartItemDto
    {
        public int Id { get; set; }
        public int ShoppingCartId { get; set; }
        public int ProductId { get; set; }
        public int DistributorSapAccountId { get; set; }
        public decimal Quantity { get; set; }
        public string UnitOfMeasureCode { get; set; } = null!;
        public DateTime DateCreated { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DateModified { get; set; }
        public int? ModifiedByUserId { get; set; }
        public string ChannelCode { get; set; } = null!;
    }
}
