using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPDomain.HelperModels;

namespace TMPApplication.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailNotification(EmailMessage emailInfo, string template);
        Task SendEmail(string email, string subject, string content);
    }
}
