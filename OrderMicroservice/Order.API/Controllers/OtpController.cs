using Account.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Request;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OtpController : BaseController
    {
        private readonly IOtpService _otpService;
        public OtpController(IOtpService otpService)
        {
            _otpService = otpService;
        }

        [HttpPost("validate")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        public async Task<IActionResult> ValidateOtp(ValidateOtpRequestDTO model) => Response(await _otpService.ValidateOtp(model).ConfigureAwait(false));

        [HttpPost("resend")]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiResponse))]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ApiResponse))]
        public async Task<IActionResult> ResendOtp(ResendOtpRequestDTO model) => Response(await _otpService.ResendOtp(model).ConfigureAwait(false));
    }
}
