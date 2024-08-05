using FluentValidation;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class ProjectUserValidator : AbstractValidator<ProjectUser>
    {
        public ProjectUserValidator()
        {
            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("ProjectId must be greater than 0.");

            RuleFor(x => x.Project)
                .NotNull().WithMessage("Project cannot be null.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId cannot be empty.");

            RuleFor(x => x.User)
                .NotNull().WithMessage("User cannot be null.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Role must be a valid enum value.");
        }
    }
}
