using Order.Application.Interfaces.Services;
using System.Security.Claims;

namespace Order.API.ClientServices
{
    public class AuthenticatedUserService : IAuthenticatedUserService
    {
        public AuthenticatedUserService(IHttpContextAccessor httpContextAccessor)
        {
            _ = int.TryParse(httpContextAccessor.HttpContext?.User?.FindFirstValue("userId"), out int userKey);
            Email = httpContextAccessor.HttpContext?.User?.FindFirstValue("emailAddress") ?? string.Empty;
            PhoneNumber = httpContextAccessor.HttpContext?.User?.FindFirstValue("phoneNumber") ?? string.Empty;
            UserId = userKey;
        }

        public int UserId { get; set; }
        public string Email { get ; set ; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public long ClientId { get; set; }
        public string Role { get; set ; }
    }
}
