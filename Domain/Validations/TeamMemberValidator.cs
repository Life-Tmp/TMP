using FluentValidation;
using TMPDomain.Enumerations;
using System;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class TeamMemberValidator : AbstractValidator<TeamMember>
    {
        public TeamMemberValidator()
        {
            RuleFor(x => x.TeamId)
                .GreaterThan(0).WithMessage("TeamId must be greater than 0.");

            RuleFor(x => x.Team)
                .NotNull().WithMessage("Team cannot be null.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.User)
                .NotNull().WithMessage("User cannot be null.");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Role must be a valid enum value.");
        }
    }
}
