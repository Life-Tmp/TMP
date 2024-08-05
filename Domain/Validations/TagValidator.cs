using FluentValidation;
using System.Collections.Generic;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class TagValidator : AbstractValidator<Tag>
    {
        public TagValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        }
    }
}
