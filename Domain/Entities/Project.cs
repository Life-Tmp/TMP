namespace TMPDomain.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string CreatedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User CreatedByUser { get; set; }
        public ICollection<Task> Tasks { get; set; }
        public ICollection<Column> Columns { get; set; }
        public ICollection<ProjectUser> ProjectUsers { get; set; } = new List<ProjectUser>();
        public ICollection<ProjectTeam> ProjectTeams { get; set; } = new List<ProjectTeam>();
    }

    public class Column
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Project> Projects { get; set; }
    }
}
