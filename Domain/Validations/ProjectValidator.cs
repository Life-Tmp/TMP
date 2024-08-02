using FluentValidation;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class ProjectValidator : AbstractValidator<Project>
    {
        public ProjectValidator()
        {
            RuleFor(project => project.Name)
                .NotEmpty().WithMessage("Project name is required.")
                .MaximumLength(255).WithMessage("Project name cannot exceed 255 characters.");

            RuleFor(project => project.Description)
                .NotEmpty().WithMessage("Project description is required.")
                .MaximumLength(1000).WithMessage("Project description cannot exceed 1000 characters.");

            RuleFor(project => project.CreatedByUserId)
                .NotEmpty().WithMessage("Created by User ID is required.");

            RuleFor(project => project.CreatedAt)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Created date cannot be in the future.");

            RuleFor(project => project.UpdatedAt)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Updated date cannot be in the future.");

            RuleForEach(project => project.ProjectUsers)
                .SetValidator(new ProjectUserValidator());

            RuleForEach(project => project.ProjectTeams)
                .SetValidator(new ProjectTeamValidator());
        }
    }
}

public class ProjectUserValidator : AbstractValidator<ProjectUser>
{
    public ProjectUserValidator()
    {
        RuleFor(projectUser => projectUser.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(projectUser => projectUser.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");
    }
}

public class ProjectTeamValidator : AbstractValidator<ProjectTeam>
{
    public ProjectTeamValidator()
    {
        RuleFor(projectTeam => projectTeam.TeamId)
            .NotEmpty().WithMessage("Team ID is required.");

        RuleFor(projectTeam => projectTeam.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");
    }
}
