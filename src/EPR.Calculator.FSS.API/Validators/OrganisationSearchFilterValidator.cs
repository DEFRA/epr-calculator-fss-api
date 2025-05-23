﻿namespace EPR.Calculator.FSS.API.Validators;

using EPR.Calculator.FSS.API.Common.Models;
using FluentValidation;
using System.Globalization;

public class OrganisationSearchFilterValidator : AbstractValidator<OrganisationSearchFilter>
{
    public OrganisationSearchFilterValidator()
    {
        // The date should be a valid date
        // E.g. "2021-01-30"
        RuleFor(x => x.CreatedOrModifiedAfter).Must(BeAValidDate).WithMessage("Please enter a valid date. E.g. 2025-05-20");
    }

    private static bool BeAValidDate(string createdOrModifiedAfter)
    {
        string[] formats = { "yyyy-MM-dd", "yyyy-MMM-dd" };
        var validDate = DateTime.TryParseExact(createdOrModifiedAfter, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        return validDate;
    }
}