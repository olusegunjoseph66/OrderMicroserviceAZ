using Order.Application.Interfaces.Messaging;

namespace Order.API.Extensions
{
    public static class ApplicaionBuilderExtension
    {
        public static IAzureServiceBusConsumer ServiceBusConsumer { get; set; }
        public static IServiceScopeFactory _scopeFactory { get; set; }
        public static IApplicationBuilder UseAzureServiceBusConsume(this IApplicationBuilder app)
        {
            _scopeFactory = app.ApplicationServices.GetService<IServiceScopeFactory>();
            using var scope = _scopeFactory.CreateScope();
            ServiceBusConsumer = scope.ServiceProvider.GetRequiredService<IAzureServiceBusConsumer>();
            var hostApplicationLife = app.ApplicationServices.GetService<IHostApplicationLifetime>();

            hostApplicationLife.ApplicationStarted.Register(OnStart);
            //hostApplicationLife.ApplicationStarted.Register(OnStop);

            return app;
        }

        private static void OnStop()
        {
            ServiceBusConsumer.StopProductMsg();
            ServiceBusConsumer.StopAccountMsg();
            ServiceBusConsumer.StartOrderMsg();
        }

        private static void OnStart()
        {
            ServiceBusConsumer.StartAccountMsg();
            ServiceBusConsumer.StartProductMsg();
            ServiceBusConsumer.StartOrderMsg();
        }
    }
}
