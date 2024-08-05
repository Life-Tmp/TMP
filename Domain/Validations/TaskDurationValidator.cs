using FluentValidation;
using System;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class TaskDurationValidator : AbstractValidator<TaskDuration>
    {
        public TaskDurationValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required.");

            RuleFor(x => x.TaskId)
                .GreaterThan(0).WithMessage("TaskId must be greater than 0.");
        }
    }
}
