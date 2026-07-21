using System.Globalization;
using System.Text;

namespace EPR.Calculator.FSS.API.Helpers;

public static class BillingFileNameHelper
{
    private static readonly CompositeFormat BillingFileName =
        CompositeFormat.Parse("{0}billing.json");

    public static string Create(int calculatorRunId) =>
        string.Format(
            CultureInfo.CurrentCulture,
            BillingFileName,
            calculatorRunId);
}
