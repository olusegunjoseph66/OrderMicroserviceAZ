using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Response
{
    public record OtpResponseDto(long OtpId);
    public class OtpResponse
    {
        public OtpResponseVM Otp { get; set; }
    }
    public class OtpResponseVM
    {
        public int OtpId { get; set; }
    }
}
