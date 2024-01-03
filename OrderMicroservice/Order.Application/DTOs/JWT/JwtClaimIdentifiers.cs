using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Application.DTOs.JWT
{
    public class JwtClaimIdentifiers
    {
        public const string Role = "role", UserId = "userId", FirstName = "firstName", LastName = "lastName", UserName = "username";
        public const string ApiAccess = "api_access";
    }
}
