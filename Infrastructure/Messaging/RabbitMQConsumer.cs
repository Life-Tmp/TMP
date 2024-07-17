using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPInfrastructure.Messaging
{
    public class RabbitMQConsumer
    {
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly NotificationHandler _notificationHandler;
        
        public RabbitMQConsumer(IModel channel, NotificationHandler notificationHandler, ILogger<RabbitMQConsumer> logger)
        {
            _logger = logger;
            _channel = channel;
           
            _notificationHandler = notificationHandler;
        }

        public void StartConsume(string queue)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await HandleMessageAsync(message, queue);

                _channel.BasicAck(ea.DeliveryTag, false);
            };
            _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
           
        }
        private async Task HandleMessageAsync(string message, string queueName)
        {
            switch (queueName)
            {
                case "notifications": //TODO: Use constants
                    await _notificationHandler.HandleMessageAsync(message);
                    _logger.LogInformation("Successfully handled the message in the queue: {queueName}", queueName);
                    break;
                // Add other case statements for different queues
                default:
                    Console.WriteLine($"Unhandled queue: {queueName}");
                    _logger.LogWarning($"Unhandled queue: {queueName}");
                    break;
            }
        }
    }
}
