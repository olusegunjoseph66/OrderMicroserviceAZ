using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.Request
{
    public class ResendOtpRequestDTO
    {
        //[Required]
        public int otpId { get; set; }
    }
    public class ValidateOtpRequestDTO
    {
        [Required]
        public int otpId { get; set; }
        [Required]
        public string otpCode { get; set; }
    }
}
