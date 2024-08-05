using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TMPDomain.Entities;
using Task = TMPDomain.Entities.Task;

namespace TMP.Application.Interfaces
{
    public interface IDatabaseService
    {
        DbSet<Attachment> Attachments { get; set; }
        DbSet<Task> Tasks { get; set; }
        DbSet<TaskDuration> TaskDurations { get; set; }
        DbSet<User> Users { get; set; }
        DbSet<Tag> Tags { get; set; }
        DbSet<Project> Projects { get; set; }
        DbSet<Comment> Comments { get; set; }
        DbSet<Notification> Notifications { get; set; }

        DbSet<ContactForm> ContactForms { get; set; }

        void Save();
    }
}
