namespace Shared.ExternalServices.DTOs
{
    public class IntegrationBaseMessage
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime DateCreated { get; set; }
    }
}
