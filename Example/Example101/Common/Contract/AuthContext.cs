using System.Security.Claims;
using System.Security.Principal;

namespace Example101.Common.Contract
{
    public class AuthContext
    {
        public required ClaimsPrincipal Principal {get; init;}
    }
}