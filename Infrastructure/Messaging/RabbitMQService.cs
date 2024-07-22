using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TMPInfrastructure.Messaging
{
    public class RabbitMQService
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConfiguration _configuration;
        private IConnection _connection;
        private IModel _channel;
        private readonly ILogger<RabbitMQService> _logger;

        public RabbitMQService(IConfiguration configuration,ILogger<RabbitMQService> logger)
        {
            _logger = logger;
            _configuration = configuration;
            _connectionFactory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Hostname"],
                UserName = _configuration["RabbitMQ:Username"],
                Password = _configuration["RabbitMQ:Password"],
                VirtualHost = _configuration["RabbitMQ:VirtualHost"],
                DispatchConsumersAsync = true //For async consumer (if needed)
            };
            Connect(); 
        }
        private void Connect() 
        {
            try
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel(); //Check what exactly is a Model
                _channel.QueueDeclare(queue: "general-notification", durable: false, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(queue: "task-notification", durable: false, exclusive: false, autoDelete: false, arguments: null);
                _channel.QueueDeclare(queue: "reminder", durable: false, exclusive: false, autoDelete: false, arguments: null);
                _logger.LogInformation($"Successfully connected to Channel:  {_channel.ChannelNumber}");
                
            }
            catch(Exception e)
            {
                _logger.LogError(e,"Couldnt't create a connection to RabbitMQ");
                throw new Exception("Couldnt't create a connection to RabbitMQ",e); //TODO: Fix this
            }
           
        }
        public void DeclareQueue(string queueName)
        {
            _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public IModel GetChannel()
        {
            if (_channel == null)
                throw new InvalidOperationException("RabbitMQ channel is not initialized");

            return _channel;
        }


        public void PublishMessage(object message, string queueName)
        {
            if (_channel == null)
            {
                _logger.LogWarning("RabbitMQ channel is not initialied");
                throw new InvalidOperationException("RabbitMQ channel is not initialized.");
            }
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore  //INFO: To ignore reference loop, when we try to serialize
                                                                      
            };
            string messageSerialized = JsonConvert.SerializeObject(message,settings);
            var body = Encoding.UTF8.GetBytes(messageSerialized);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true; // Make message persistent so it is saved in disk in case of
                                          // a forced restart of RabbitMQ server

            _channel.BasicPublish(exchange: "",
                                  routingKey: queueName,
                                  basicProperties: properties,
                                  body: body);

            Console.WriteLine($" Sent {message}");  //THis is just for testing
            _logger.LogInformation($"The message was sent successfully to queue: {queueName}");
        }

    }
}
