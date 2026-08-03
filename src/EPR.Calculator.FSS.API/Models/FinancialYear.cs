using System.ComponentModel;
using System.Globalization;

// ReSharper disable once CheckNamespace - Avoids namespace/classname duplication weirdness
namespace EPR.Calculator.FSS.API.Models;

[TypeConverter(typeof(FinancialYearTypeConverter))]
public readonly record struct FinancialYear(string Value)
{
    public RelativeYear ToRelativeYear() =>
        new(int.Parse(Value[..4], CultureInfo.InvariantCulture));

    public override string ToString() => Value;
}

internal sealed class FinancialYearTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is not string s)
            return base.ConvertFrom(context, culture, value);

        if (TryParse(s))
            return new FinancialYear(s);

        throw new FormatException("Financial year must be in the format YYYY-YY.");
    }

    private static bool TryParse(string value) =>
        value.Length == 7 &&
        value[4] == '-' &&
        int.TryParse(value[..4], out var start) &&
        int.TryParse(value[5..], out var end) &&
        start > 2000 &&
        end == (start + 1) % 100;
}
