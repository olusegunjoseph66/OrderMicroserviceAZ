using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Shared.ExternalServices.Configurations;
using Shared.ExternalServices.DTOs;
using Shared.ExternalServices.Interfaces;
using System.Text;

namespace Shared.ExternalServices.APIServices
{
    public class QueueMessagingService : IQueueMessagingService
    {
        private readonly MessagingServiceSetting _messagingSetting;
        private readonly ServiceBusClient _client;
        //private ServiceBusSender _clientSender;

        public QueueMessagingService(IOptions<MessagingServiceSetting> messagingSetting)
        {
            _messagingSetting = messagingSetting.Value;
            _client = new ServiceBusClient(_messagingSetting.ConnectionString);
        }
        public async Task PublishTopicMessage(dynamic message, string subscriberName)
        {
            message.Id = Guid.NewGuid();
            var jsonMessage = JsonConvert.SerializeObject(message);
            var busMessage = new Message(Encoding.UTF8.GetBytes(jsonMessage))
            {
                PartitionKey = Guid.NewGuid().ToString(),
                Label = subscriberName
            };
            ISenderClient topicClient = new TopicClient(_messagingSetting.ConnectionString, _messagingSetting.TopicName);
            await topicClient.SendAsync(busMessage);
            Console.WriteLine($"Sent message to {topicClient.Path}");
            await topicClient.CloseAsync();
        }
        public ServiceBusProcessor ConsumeMessage(string topicName, string subscriptionName)
        {
            return  _client.CreateProcessor(topicName, subscriptionName);
        }

        //public async Task PublishMessage(dynamic request, string topicName)
        //{
        //    string messagePayload = JsonConvert.SerializeObject(request);

        //    _clientSender = _client.CreateSender(topicName);
        //    ServiceBusMessage message = new(messagePayload);
        //    await _clientSender.SendMessageAsync(message).ConfigureAwait(false);
        //}

        //public async Task PublishMessage(IntegrationBaseMessage message, string topicName)
        //{
        //    ISenderClient topicClient = new TopicClient(Settings.SERVICE_BUS_CONNECTION_STRING, topicName);

        //    var jsonMessage = JsonConvert.SerializeObject(message);
        //    var serviceBusMessage = new Message(Encoding.UTF8.GetBytes(jsonMessage))
        //    {
        //        CorrelationId = Guid.NewGuid().ToString()
        //    };

        //    await topicClient.SendAsync(serviceBusMessage);
        //    Console.WriteLine($"Sent message to {topicClient.Path}");
        //    await topicClient.CloseAsync();

        //}
    }
}
