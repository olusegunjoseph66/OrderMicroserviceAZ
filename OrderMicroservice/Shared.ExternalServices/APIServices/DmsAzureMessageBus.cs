using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shared.ExternalServices.DTOs;
using Shared.ExternalServices.Interfaces;
using System.Text;

namespace Shared.ExternalServices.APIServices
{
    public class DmsAzureMessageBus : IDmsAzureMessageBus
    {
        private readonly IConfiguration _configuration;
        public DmsAzureMessageBus(IConfiguration _configuration)
        {
            this._configuration = _configuration;
        }
        public async Task PublishMessage(IntegrationBaseMessage message, string topicName)
        {
            ISenderClient topicClient = new TopicClient(_configuration["SERVICE_BUS_CONNECTION_STRING"], topicName);

            var jsonMessage = JsonConvert.SerializeObject(message);
            var serviceBusMessage = new Message(Encoding.UTF8.GetBytes(jsonMessage))
            {
                CorrelationId = Guid.NewGuid().ToString()
            };

            await topicClient.SendAsync(serviceBusMessage);
            Console.WriteLine($"Sent message to {topicClient.Path}");
            await topicClient.CloseAsync();

        }
    }
}
