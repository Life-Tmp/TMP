using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPApplication.Interfaces;
using TMPDomain.HelperModels;

namespace TMPInfrastructure.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        // private readonly string _templatePath;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;

        }


        public async Task SendEmailNotification(EmailMessage emailInfo, string templatePath)
        {
            var client = new MailjetClient(_configuration["Mailjet:ApiKey"], _configuration["Mailjet:ApiSecret"]);

            string template = await GetEmailTemplateAsync(templatePath);
            string emailBody = PopulateTemplate(emailInfo,template);

            var emailRequest = new MailjetRequest
            {
                Resource = Send.Resource
            }
            .Property(Send.FromEmail, _configuration["Mailjet:FromEmail"])
            .Property(Send.FromName, _configuration["Mailjet:FromName"])
            .Property(Send.Subject, emailInfo.Subject )
            .Property(Send.HtmlPart, emailBody)
            .Property(Send.Recipients, new JArray
            {
            new JObject
            {
                {"Email", emailInfo.EmailAddress},

            }
            });
            Console.WriteLine(emailInfo.EmailAddress);
            await client.PostAsync(emailRequest);
        }

        private async Task<string> GetEmailTemplateAsync(string emailPath)
        {
            using (StreamReader reader = new StreamReader(emailPath))
            {
                return await reader.ReadToEndAsync();
            }
        }
        private string PopulateTemplate(EmailMessage emailInfo, string template)
        {
            return template
                .Replace("{{UserFirstName}}", emailInfo.UserFirstName)
                .Replace("{{UserLastName}}", emailInfo.UserLastName)
                .Replace("{{NotificationMessage}}", emailInfo.NotificationMessage)
                .Replace("{{TaskTitle}}", emailInfo.TaskTitle)
                .Replace("{{TaskDescription}}", emailInfo.TaskDescription)
                .Replace("{{TaskDueDate}}", emailInfo.TaskDueDate.ToString());
                
        }
    }
}
