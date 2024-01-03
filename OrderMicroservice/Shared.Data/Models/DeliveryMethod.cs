using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class DeliveryMethod
    {
        public byte Id { get; set; }
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
