using Order.Application.Interfaces.Services;

namespace Order.Infrastructure.Services
{
    public class BaseService
    {
        internal readonly IAuthenticatedUserService _authenticatedUserService;
        public BaseService(IAuthenticatedUserService authenticatedUserService)
        {
            _authenticatedUserService = authenticatedUserService;
        }

        public int LoggedInUserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int LoggedInUser()
        {
            if (_authenticatedUserService.UserId == 0) throw new UnauthorizedAccessException($"Access Denied.");
            return _authenticatedUserService.UserId;
        }

        public void GetUserId()
        {
            if (_authenticatedUserService.UserId == 0) throw new UnauthorizedAccessException($"Access Denied. Kindly login to continue to this request.");
            LoggedInUserId = _authenticatedUserService.UserId;
        }

        public void GetEmail()
        {
            if (string.IsNullOrEmpty(_authenticatedUserService.Email)) throw new UnauthorizedAccessException($"Access Denied. Kindly login to continue to this request.");
            Email = _authenticatedUserService.Email;
        }
        public void GetPhone()
        {
            if (string.IsNullOrEmpty(_authenticatedUserService.PhoneNumber)) throw new UnauthorizedAccessException($"Access Denied. Kindly login to continue to this request.");
            PhoneNumber = _authenticatedUserService.PhoneNumber;
        }

    }
}
