using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class DeliveryStatusDto
    {

        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
