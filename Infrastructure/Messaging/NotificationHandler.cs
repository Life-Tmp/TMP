using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPDomain.Entities;
using Task = System.Threading.Tasks.Task;

namespace TMPInfrastructure.Messaging
{
    
    public class NotificationHandler
    {
        private readonly ILogger<NotificationHandler> _logger;

        public NotificationHandler(ILogger<NotificationHandler> logger)
        {
            _logger = logger;
        }

        public async Task HandleMessageAsync(string message)
        {
            var notification = JsonConvert.DeserializeObject<Notification>(message);
            Console.WriteLine(notification.Message);
             // Just for testing, changing later
        }
    }

    
}
