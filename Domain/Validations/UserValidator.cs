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
                .EmailAddress().WithMessage("Email must be a valid email address.")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters.");

            RuleFor(x => x.Birthdate)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Birthdate cannot be in the future.");

            RuleFor(x => x.PhoneNumber)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("PhoneNumber must be in E.164 format.")
                .When(x => !string.IsNullOrEmpty(x.PhoneNumber)); // Validate only if PhoneNumber is provided

            
        }
    }
}
