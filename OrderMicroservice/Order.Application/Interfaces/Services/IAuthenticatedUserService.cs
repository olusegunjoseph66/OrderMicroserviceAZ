namespace Order.Application.Interfaces.Services
{
    public interface IAuthenticatedUserService
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public long ClientId { get; set; }
        public string Role { get; set; }
    }
}
