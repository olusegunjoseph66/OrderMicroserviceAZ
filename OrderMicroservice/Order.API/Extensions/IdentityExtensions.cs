using Order.Application.DTOs.JWT;
using Order.Application.ViewModels;
using System.Security.Claims;
using System.Security.Principal;

namespace Order.API.Extensions
{
    public static class IdentityExtensions
    {
        public static UserDetailsViewModel GetSessionDetails(this IPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;

            UserDetailsViewModel getData = new();
            getData.UserID = Convert.ToInt32(identity.Claims.Single(c => c.Type == JwtClaimIdentifiers.UserId).Value);
            //getData.Role = identity.Claims.Single(c => c.Type == JwtClaimIdentifiers.Role).Value;
            getData.UserName = identity.Claims.Single(c => c.Type == JwtClaimIdentifiers.UserName).Value;
            getData.FirstName = identity.Claims.Single(c => c.Type == JwtClaimIdentifiers.FirstName).Value;
            getData.LastName = identity.Claims.Single(c => c.Type == JwtClaimIdentifiers.LastName).Value;
            
            return getData;
        }

        public static void ClearSessionDetails(this IPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity;
            foreach (var claim in identity.Claims)
            {
                identity.TryRemoveClaim(claim);
            }
        }
    }

}
