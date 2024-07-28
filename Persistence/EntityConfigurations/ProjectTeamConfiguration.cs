using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

public class ProjectTeamConfiguration : IEntityTypeConfiguration<ProjectTeam>
{
    public void Configure(EntityTypeBuilder<ProjectTeam> builder)
    {
        builder.HasKey(pt => new { pt.ProjectId, pt.TeamId });

        builder.HasOne(pt => pt.Project)
            .WithMany(p => p.ProjectTeams)
            .HasForeignKey(pt => pt.ProjectId);

        builder.HasOne(pt => pt.Team)
            .WithMany(t => t.ProjectTeams)
            .HasForeignKey(pt => pt.TeamId);
    }
}
