using System;
using EPR.Calculator.FSS.API.Common.Models;
using FluentValidation;
using FluentValidation.Results;

namespace EPR.Calculator.FSS.API.Common.Validators;
public class OrganisationSearchFilterValidator : AbstractValidator<OrganisationSearchFilter>
{
    public OrganisationSearchFilterValidator()
    {
        // The date should be a valid date
        // E.g. "2021-01-30"
        RuleFor(x => x.CreatedOrModifiedAfter).Must(BeAValidDate).WithMessage("Please enter a valid date. E.g. 2025-05-20");
    }

    private static bool BeAValidDate(string postcode)
    {
        // custom date validating logic goes here
        return true;
    }
}