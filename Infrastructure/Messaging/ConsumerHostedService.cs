using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPInfrastructure.Messaging
{
    public class ConsumerHostedService : IHostedService
    {
        private readonly RabbitMQConsumer _consumer;

        public ConsumerHostedService(RabbitMQConsumer consumer)
        {
            _consumer = consumer;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _consumer.StartConsume("notification");
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
