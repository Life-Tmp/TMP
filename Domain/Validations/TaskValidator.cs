using FluentValidation;
using TMPDomain.Enumerations;
using System;
using System.Collections.Generic;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class TaskValidator : AbstractValidator<Entities.Task>
    {
        public TaskValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");

            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.");

            RuleFor(x => x.Priority)
                .IsInEnum().WithMessage("Priority must be a valid enum value.");

            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status must be a valid enum value.");

            RuleFor(x => x.DueDate)
                .GreaterThan(DateTime.Now).WithMessage("DueDate must be in the future.");

            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("CreatedAt is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("CreatedAt cannot be in the future.");

            RuleFor(x => x.UpdatedAt)
                .NotEmpty().WithMessage("UpdatedAt is required.")
                .GreaterThanOrEqualTo(x => x.CreatedAt)
                .WithMessage("UpdatedAt must be greater than or equal to CreatedAt.");

            RuleFor(x => x.ProjectId)
                .GreaterThan(0).WithMessage("ProjectId must be greater than 0.");

            RuleFor(x => x.Project)
                .NotNull().WithMessage("Project cannot be null.");

            RuleFor(x => x.Comments)
                .NotNull().WithMessage("Comments collection cannot be null.");

            RuleFor(x => x.Attachments)
                .NotNull().WithMessage("Attachments collection cannot be null.");

            RuleFor(x => x.TaskDurations)
                .NotNull().WithMessage("TaskDurations collection cannot be null.");

            RuleFor(x => x.Reminders)
                .NotNull().WithMessage("Reminders collection cannot be null.");

            RuleFor(x => x.Subtasks)
                .NotNull().WithMessage("Subtasks collection cannot be null.");

            RuleFor(x => x.AssignedUsers)
                .NotNull().WithMessage("AssignedUsers collection cannot be null.");

            RuleFor(x => x.Tags)
                .NotNull().WithMessage("Tags collection cannot be null.");
        }
    }
}
