using FluentValidation;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class NotificationValidator : AbstractValidator<Notification>
    {
        public NotificationValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Message)
                .NotEmpty().WithMessage("Message is required.");

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Subject is required.");

            RuleFor(x => x.NotificationType)
                .NotEmpty().WithMessage("Notification type is required.");
        }
    }
}
