using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Features.Otp
{
    public class OtpDto{
        public long Id { get; set; }
        public string Code { get; set; }
        public DateTime DateExpiry { get; set; }

    }
}
