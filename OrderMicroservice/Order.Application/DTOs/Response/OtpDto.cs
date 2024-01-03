using System;
using System.Collections.Generic;

namespace Order.Application.DTOs.Response
{
    public partial class OtpDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string PhoneNumber { get; set; }
        public byte OtpStatusId { get; set; }
        public int? DmsOrderId { get; set; }
        public short? NumberOfRetries { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateExpiry { get; set; }
    }
}
