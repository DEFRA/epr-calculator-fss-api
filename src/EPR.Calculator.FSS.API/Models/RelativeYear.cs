using System.Globalization;

namespace EPR.Calculator.FSS.API.Models;

public readonly record struct RelativeYear(int Value)
{
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
