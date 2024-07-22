using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPInfrastructure.Messaging
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ConsumerHostedService> _logger;

        public ConsumerHostedService(IServiceProvider serviceProvider, ILogger<ConsumerHostedService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var _consumer = scope.ServiceProvider.GetRequiredService<RabbitMQConsumer>();
                    _consumer.StartConsume("general-notification");
                    _consumer.StartConsume("task-notification");
                    _consumer.StartConsume("reminder");
                }
            });
            
            //TODO: Start consuming other queues as needed
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            //TODO: Implement graceful shutdown logic if necessary
            return Task.CompletedTask;
        }
    }
}
