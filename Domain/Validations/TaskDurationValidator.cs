using FluentValidation;
using System;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class TaskDurationValidator : AbstractValidator<TaskDuration>
    {
        public TaskDurationValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Id must be greater than 0.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.TaskId)
                .GreaterThan(0).WithMessage("TaskId must be greater than 0.");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("StartTime is required.")
                .LessThanOrEqualTo(x => x.EndTime)
                .WithMessage("StartTime must be less than or equal to EndTime.");

            RuleFor(x => x.Task)
                .NotNull().WithMessage("Task cannot be null.");

            RuleFor(x => x.User)
                .NotNull().WithMessage("User cannot be null.");
        }
    }
}
