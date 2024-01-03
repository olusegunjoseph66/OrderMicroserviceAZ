using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class TruckSizeDeliveryMethodMapping
    {
        public short Id { get; set; }
        public string TruckSizeCode { get; set; } = null!;
        public string DeliveryMethodCode { get; set; } = null!;
        public string PlantTypeCode { get; set; } = null!;
    }
}
