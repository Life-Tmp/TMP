using TMPDomain.Entities;

public class ProjectTeam
{
    public int ProjectId { get; set; }
    public Project Project { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; }
}
