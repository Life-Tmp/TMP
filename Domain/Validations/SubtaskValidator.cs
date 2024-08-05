using FluentValidation;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class SubtaskValidator : AbstractValidator<Subtask>
    {
        public SubtaskValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(30).WithMessage("Title must not exceed 30 characters.");


            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("CreatedAt is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("CreatedAt cannot be in the future.");;

        }
    }
}
