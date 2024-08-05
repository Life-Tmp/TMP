using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPDomain.Entities;

namespace TMPDomain.Validations
{
        public class ContactFormValidator : AbstractValidator<ContactForm>
        {
            public ContactFormValidator()
            {
                RuleFor(x => x.FirstName)
                    .NotEmpty().WithMessage("First name is required.");

                RuleFor(x => x.LastName)
                    .NotEmpty().WithMessage("Last name is required.");

                RuleFor(x => x.Email)
                    .NotEmpty().WithMessage("Email is required.")
                    .EmailAddress().WithMessage("Invalid email address format.");

                RuleFor(x => x.Message)
                    .NotEmpty().WithMessage("Message is required.");
            }
        }

}
