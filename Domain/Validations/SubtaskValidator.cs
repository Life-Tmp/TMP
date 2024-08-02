using FluentValidation;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class SubtaskValidator : AbstractValidator<Subtask>
    {
        public SubtaskValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");


            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("CreatedAt is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("CreatedAt cannot be in the future.");

            RuleFor(x => x.CompletedAt)
                .GreaterThanOrEqualTo(x => x.CreatedAt)
                .When(x => x.CompletedAt.HasValue)
                .WithMessage("CompletedAt must be greater than or equal to CreatedAt.");

            RuleFor(x => x.TaskId)
                .GreaterThan(0).WithMessage("TaskId must be greater than 0.");

            RuleFor(x => x.Task)
                .NotNull().WithMessage("Task cannot be null.");
        }
    }
}
