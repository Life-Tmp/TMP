using FluentValidation;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class ProjectTeamValidator : AbstractValidator<ProjectTeam>
    {
        public ProjectTeamValidator()
        {
            RuleFor(projectTeam => projectTeam.ProjectId)
                .GreaterThan(0).WithMessage("Project ID must be greater than zero.");

            RuleFor(projectTeam => projectTeam.TeamId)
                .GreaterThan(0).WithMessage("Team ID must be greater than zero.");
        }
    }
}
