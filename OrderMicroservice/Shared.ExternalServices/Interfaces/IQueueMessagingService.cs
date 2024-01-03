using Azure.Messaging.ServiceBus;
using Shared.ExternalServices.DTOs;

namespace Shared.ExternalServices.Interfaces
{
    public interface IQueueMessagingService
    {
        Task PublishTopicMessage(dynamic message, string subscriberName);
        ServiceBusProcessor ConsumeMessage(string topicName, string subscriptionName);
    }
}
