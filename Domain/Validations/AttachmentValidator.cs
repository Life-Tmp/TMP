using FluentValidation;
using TMPDomain.Entities;

namespace TMPDomain.Entities
{
    public class AttachmentValidator : AbstractValidator<Attachment>
    {
        public AttachmentValidator()
        {
            RuleFor(attachment => attachment.FileName)
                .NotEmpty().WithMessage("File name is required.")
                .Length(1, 255).WithMessage("File name must be between 1 and 255 characters.");

            RuleFor(attachment => attachment.FilePath)
                .NotEmpty().WithMessage("File path is required.");

            RuleFor(attachment => attachment.FileSize)
                .GreaterThan(0).WithMessage("File size must be greater than zero.");

            RuleFor(attachment => attachment.FileType)
                .NotEmpty().WithMessage("File type is required.");

            RuleFor(attachment => attachment.TaskId)
                .GreaterThan(0).WithMessage("Task ID must be greater than zero.");

            RuleFor(attachment => attachment.UploadDate)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Upload date cannot be in the future.");
        }
    }
}