using FinanceAPI.Enums;
using System.Security.Claims;

namespace FinanceAPI.Helpers
{
    public class CurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int LoggedInUser()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userId = int.Parse(user?.FindFirst(ClaimTypes.NameIdentifier)?.Value);

            return userId;
        }

        public bool IsAdmin()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            var userRole = user.IsInRole(RoleType.Admin.ToString());

            return userRole;
        }
    }
}
