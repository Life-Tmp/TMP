using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TMPDomain.Entities;
using TMP.Application.Interfaces;
using Persistence.EntityConfigurations;
using Task = TMPDomain.Entities.Task;

namespace TMP.Persistence
{
    public class DatabaseService : DbContext, IDatabaseService
    {
        private readonly IConfiguration _configuration;

        public DatabaseService(DbContextOptions<DatabaseService> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;

            Database.EnsureCreated();
        }

        public DbSet<Attachment> Attachments { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<TaskDuration> TaskDurations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }

        public void Save()
        {
            SaveChanges();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            new AttachmentConfiguration().Configure(builder.Entity<Attachment>());
            new TaskConfiguration().Configure(builder.Entity<Task>());
            new TaskDurationConfiguration().Configure(builder.Entity<TaskDuration>());
            new UserConfiguration().Configure(builder.Entity<User>());
            new TagConfiguration().Configure(builder.Entity<Tag>());
            new ProjectConfiguration().Configure(builder.Entity<Project>());
            new CommentConfiguration().Configure(builder.Entity<Comment>());
            new NotificationConfiguration().Configure(builder.Entity<Notification>());
            new ProjectUserConfiguration().Configure(builder.Entity<ProjectUser>());
        }
    }
}
