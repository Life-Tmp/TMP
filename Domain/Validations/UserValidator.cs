using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
    public class UserValidator : AbstractValidator<User>
    {
        public UserValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Id is required.");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("FirstName is required.")
                .MaximumLength(100).WithMessage("FirstName must not exceed 100 characters.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("LastName is required.")
                .MaximumLength(100).WithMessage("LastName must not exceed 100 characters.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.") //valido per @
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");


            RuleFor(x => x.ProfilePicture)
                .MaximumLength(500).WithMessage("ProfilePicture URL must not exceed 500 characters.");

            RuleFor(x => x.Birthdate)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Birthdate cannot be in the future.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("PhoneNumber must be in E.164 format.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber)); // Validate only if PhoneNumber is provided

            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("CreatedAt is required.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("CreatedAt cannot be in the future.");

            RuleFor(x => x.UpdatedAt)
                .NotEmpty().WithMessage("UpdatedAt is required.")
                .GreaterThanOrEqualTo(x => x.CreatedAt).WithMessage("UpdatedAt must be greater than or equal to CreatedAt.");


            RuleFor(x => x.Comments)
                .NotNull().WithMessage("Comments collection cannot be null.");

            RuleFor(x => x.Notifications)
                .NotNull().WithMessage("Notifications collection cannot be null.");

            RuleFor(x => x.TaskDurations)
                .NotNull().WithMessage("TaskDurations collection cannot be null.");

            RuleFor(x => x.AssignedTasks)
                .NotNull().WithMessage("AssignedTasks collection cannot be null.");

            RuleFor(x => x.ProjectUsers)
                .NotNull().WithMessage("ProjectUsers collection cannot be null.");

            RuleFor(x => x.TeamMembers)
                .NotNull().WithMessage("TeamMembers collection cannot be null.");
        }
    }
}
