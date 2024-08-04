using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using TMPApplication.Interfaces;
using TMPDomain.HelperModels;

namespace TMPInfrastructure.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailNotification(EmailMessage emailInfo, string templatePath)
        {
            _logger.LogInformation("Sending email notification to: {EmailAddress}", emailInfo.EmailAddress);
            var client = new MailjetClient(_configuration["Mailjet:ApiKey"], _configuration["Mailjet:ApiSecret"]);

            try
            {
                string template = await GetEmailTemplateAsync(templatePath);
                string emailBody = PopulateTemplate(emailInfo, template);

                var emailRequest = new MailjetRequest
                {
                    Resource = Send.Resource
                }
                .Property(Send.FromEmail, _configuration["Mailjet:FromEmail"])
                .Property(Send.FromName, _configuration["Mailjet:FromName"])
                .Property(Send.Subject, emailInfo.Subject)
                .Property(Send.HtmlPart, emailBody)
                .Property(Send.Recipients, new JArray
                {
                    new JObject
                    {
                        {"Email", emailInfo.EmailAddress},
                    }
                });

                var response = await client.PostAsync(emailRequest);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email notification sent to: {EmailAddress}", emailInfo.EmailAddress);
                }
                else
                {
                    _logger.LogWarning("Failed to send email notification to: {EmailAddress}. StatusCode: {StatusCode}, ErrorInfo: {ErrorInfo}",
                        emailInfo.EmailAddress, response.StatusCode, response.GetErrorMessage());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification to: {EmailAddress}", emailInfo.EmailAddress);
                throw;
            }
        }

        public async Task SendEmail(string email, string subject, string content)
        {
            _logger.LogInformation("Sending email to: {EmailAddress}", email);
            var client = new MailjetClient(_configuration["Mailjet:ApiKey"], _configuration["Mailjet:ApiSecret"]);

            try
            {
                var emailRequest = new MailjetRequest
                {
                    Resource = Send.Resource
                }
                .Property(Send.FromEmail, _configuration["Mailjet:FromEmail"])
                .Property(Send.FromName, _configuration["Mailjet:FromName"])
                .Property(Send.Subject, subject)
                .Property(Send.HtmlPart, content)
                .Property(Send.Recipients, new JArray
                {
                    new JObject
                    {
                        {"Email", email},
                    }
                });

                var response = await client.PostAsync(emailRequest);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent to: {EmailAddress}", email);
                }
                else
                {
                    _logger.LogWarning("Failed to send email to: {EmailAddress}. StatusCode: {StatusCode}, ErrorInfo: {ErrorInfo}",
                        email, response.StatusCode, response.GetErrorMessage());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to: {EmailAddress}", email);
                throw;
            }
        }

        private async Task<string> GetEmailTemplateAsync(string emailPath)
        {
            _logger.LogInformation("Fetching email template from: {TemplatePath}", emailPath);
            try
            {
                using (StreamReader reader = new StreamReader(emailPath))
                {
                    return await reader.ReadToEndAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching email template from: {TemplatePath}", emailPath);
                throw;
            }
        }

        private string PopulateTemplate(EmailMessage emailInfo, string template)
        {
            _logger.LogInformation("Populating email template for: {EmailAddress}", emailInfo.EmailAddress);
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
