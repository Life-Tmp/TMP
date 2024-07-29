using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TMPCommon.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPInfrastructure.Messaging
{
    public class RabbitMQConsumer
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IModel _channel;
        private readonly ILogger<RabbitMQConsumer> _logger;

        public RabbitMQConsumer(IModel channel, ILogger<RabbitMQConsumer> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _channel = channel;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public void StartConsume(string queue)
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                await ProcessMessageAsync(message, ea.DeliveryTag, queue);
            };
            _channel.BasicConsume(queue: queue, autoAck: false, consumer: consumer);
           
        }

       
        private async Task ProcessMessageAsync(string message, ulong deliveryTag, string queueName)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var messageHandler  = scope.ServiceProvider.GetRequiredService<MessageHandler>(); //For later

                try
                {
                    switch (queueName)
                    {

                        case NotificationType.GeneralNotifications:
                                await messageHandler.HandleNotificationAsync(message);                                
                            break;
                        case NotificationType.TaskNotifications:
                                await messageHandler.HandleTaskNotifications(message);
                            break;
                        case NotificationType.Reminders: //TODO: Use constants
                                await messageHandler.HandleReminderNotificationAsync(message);
                            break;
                        default:
                            Console.WriteLine($"Unhandled queue: {queueName}");
                            _logger.LogWarning($"Unhandled queue: {queueName}");
                            break;
                    }
                    _channel.BasicAck(deliveryTag, false);
                    _logger.LogInformation($"Message processed successfully from queue: {queueName}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", message);
                    _channel.BasicNack(deliveryTag, false, false); //TODO: Reject the message and move it to the dead-letter queue
                }
            }
                
                
        }
    }
}
