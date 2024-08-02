using FluentValidation;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class NotificationValidator : AbstractValidator<Notification>
    {
        public NotificationValidator()
        {
            RuleFor(notification => notification.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(notification => notification.Subject)
                .NotEmpty().WithMessage("Subject is required.")
                .MaximumLength(255).WithMessage("Subject cannot exceed 255 characters.");

            RuleFor(notification => notification.Message)
                .NotEmpty().WithMessage("Message is required.");

            RuleFor(notification => notification.NotificationType)
                .NotEmpty().WithMessage("Notification Type is required.");

            RuleFor(notification => notification.CreatedAt)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Created date cannot be in the future.");

            RuleFor(notification => notification.IsRead)
                .NotNull().WithMessage("IsRead status is required.");
        }
    }
}
