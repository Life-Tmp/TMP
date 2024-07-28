using TMPDomain.Enumerations;

namespace TMPDomain.Entities
{
    public class ProjectUser
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public MemberRole Role { get; set; }
    }
}
