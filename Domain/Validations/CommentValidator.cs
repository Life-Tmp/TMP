using FluentValidation;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class CommentValidator : AbstractValidator<Comment>
    {
        public CommentValidator()
        {
            RuleFor(comment => comment.Content)
                .NotEmpty().WithMessage("Content is required.")
                .MaximumLength(1000).WithMessage("Content cannot exceed 1000 characters.");

            RuleFor(comment => comment.TaskId)
                .NotNull().WithMessage("Task ID cant be null")
                .GreaterThan(0).WithMessage("Task ID must be greater than zero.");

            RuleFor(comment => comment.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(comment => comment.CreatedAt)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Created date cannot be in the future.");
        }
    }
}
