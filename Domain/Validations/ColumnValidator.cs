using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPDomain.Entities;

public class ColumnValidator : AbstractValidator<Column>
{
    public ColumnValidator()
    {
        RuleFor(column => column.Name)
            .NotEmpty().WithMessage("Column name is required.")
            .MaximumLength(20).WithMessage("Column name cannot exceed 20 characters.");
    }
}
