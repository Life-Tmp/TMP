using FluentValidation;
using TMPDomain.Entities;
namespace TMPDomain.Validations
{
	public class ReminderValidator : AbstractValidator<Reminder>
	{
		public ReminderValidator()
		{
			RuleFor(x => x.Id)
				.GreaterThan(0).WithMessage("Id must be greater than 0.");

			RuleFor(x => x.Description)
				.NotEmpty().WithMessage("Description is required.");

			RuleFor(x => x.CreatedByUserId)
				.NotEmpty().WithMessage("CreatedByUserId is required.");

			RuleFor(x => x.ReminderDateTime)
				.GreaterThan(DateTime.Now).WithMessage("ReminderDateTime must be in the future.");

			RuleFor(x => x.TaskId)
				.GreaterThan(0).WithMessage("TaskId must be greater than 0.");

			RuleFor(x => x.Task)
				.NotNull().WithMessage("Task cannot be null.");

			//RuleFor(x => x.JobId)
			//	.Matches(@"^[a-zA-Z0-9-]+$").WithMessage("JobId must be alphanumeric with optional hyphens."); // Modify this regex as needed
		}
	}
}
