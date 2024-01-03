using System;
using System.Collections.Generic;

namespace Shared.Data.Models
{
    public partial class Otp
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string EmailAddress { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public byte OtpStatusId { get; set; }
        public int? DmsOrderId { get; set; }
        public short? NumberOfRetries { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateExpiry { get; set; }
        public int? DmsOrderGroupId { get; set; }

        public virtual DmsOrder? DmsOrder { get; set; }
        public virtual DmsOrderGroup? DmsOrderGroup { get; set; }
        public virtual OtpStatus OtpStatus { get; set; } = null!;
    }
}
