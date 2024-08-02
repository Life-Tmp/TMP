using FluentValidation;
using System;
using System.Collections.Generic;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class TeamValidator : AbstractValidator<Team>
    {
        public TeamValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("CreatedAt is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("CreatedAt cannot be in the future.");

            RuleFor(x => x.UpdatedAt)
                .NotEmpty().WithMessage("UpdatedAt is required.")
                .GreaterThanOrEqualTo(x => x.CreatedAt).WithMessage("UpdatedAt must be greater than or equal to CreatedAt.");

            RuleFor(x => x.TeamMembers)
                .NotNull().WithMessage("TeamMembers collection cannot be null.");

            RuleFor(x => x.ProjectTeams)
                .NotNull().WithMessage("ProjectTeams collection cannot be null.");
        }
    }
}
