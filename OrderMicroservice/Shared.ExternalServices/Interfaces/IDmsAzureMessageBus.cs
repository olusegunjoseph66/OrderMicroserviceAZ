using Shared.ExternalServices.DTOs;

namespace Shared.ExternalServices.Interfaces
{
    public interface IDmsAzureMessageBus
    {
        Task PublishMessage(IntegrationBaseMessage message, string topicName);
    }
}
