using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class TruckSizeDeliveryMethodMappingDto
    {
        public short Id { get; set; }
        public string TruckSizeCode { get; set; } = null!;
        public string DeliveryMethodCode { get; set; } = null!;
        public string PlantTypeCode { get; set; } = null!;
    }
}
