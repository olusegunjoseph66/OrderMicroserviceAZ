using Order.Application.DTOs.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.ViewModels.Responses
{
    public record ScheduleATCDeliveryResponse(OtpResponseDto Otp);
    
}
