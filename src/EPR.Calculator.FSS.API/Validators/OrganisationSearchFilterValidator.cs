using System.Globalization;
using EPR.Calculator.FSS.API.Models;
using FluentValidation;

namespace EPR.Calculator.FSS.API.Validators;

public class OrganisationSearchFilterValidator : AbstractValidator<OrganisationSearchFilter>
{
    public OrganisationSearchFilterValidator()
    {
        RuleFor(x => x.CreatedOrModifiedAfter)
            .Must(BeAValidDate)
            .When(x => x.CreatedOrModifiedAfter != null)
            .WithMessage("Please enter a valid date. E.g. 2025-05-20");

        RuleFor(x => x.FinancialYear)
            .Must(BeValidFinancialYear)
            .When(x => x.FinancialYear != null)
            .WithMessage("Financial year must be like 2025-26");
    }

    private static bool BeAValidDate(string? value) =>
        DateTime.TryParseExact(
            value,
            ["yyyy-MM-dd", "yyyy-MMM-dd"],
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _);

    private static bool BeValidFinancialYear(string value) =>
        value.Length == 7 &&
        value[4] == '-' &&
        int.TryParse(value[..4], out var start) &&
        int.TryParse(value[5..], out var end) &&
        start > 2000 &&
        end + 2000 == start + 1;
}
