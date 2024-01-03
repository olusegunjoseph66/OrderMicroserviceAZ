
using Order.Application.DTOs.APIDataFormatters;
using Order.Application.DTOs.Features.Otp;
using Order.Application.DTOs.Request;

namespace Account.Application.Interfaces.Services
{
    public interface IOtpService
    {
        Task<OtpDto> GenerateOtp(string emailAddress, int? orderid, int? dmsOrderGroupId, bool isNewOtp = true, string phoneNumber = null, int? userId = 0, CancellationToken cancellationToken = default);
        Task<ApiResponse> ResendOtp(ResendOtpRequestDTO otp);
        Task<ApiResponse> ValidateOtp(ValidateOtpRequestDTO otp);

        //Task<ValidateOtpResponse> ValidateOtp(ValidateOtpRequest request, CancellationToken cancellationToken);

        //Task<OtpDto> ResendOtp(ResendOtpRequest request, CancellationToken cancellationToken);

        //Task<ValidateResetOtpResponse> ValidateResetOtp(ValidateOtpRequest request, CancellationToken cancellationToken);
    }
}