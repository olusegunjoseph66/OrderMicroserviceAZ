using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class OtpStatusDto
    {
        public string Code { get; set; } = null!;
        public string Name { get; set; } = null!;
    }
}
