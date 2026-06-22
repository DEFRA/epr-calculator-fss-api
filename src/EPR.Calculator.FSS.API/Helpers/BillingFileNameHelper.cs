using EPR.Calculator.FSS.API.Constants;
using System.Globalization;
using System.Text;

namespace EPR.Calculator.FSS.API.Helpers;

public static class BillingFileNameHelper
{
    private static readonly CompositeFormat BillingFileName =
        CompositeFormat.Parse(BillingConstants.BillFileName);

    public static string Create(int calculatorRunId) =>
        string.Format(
            CultureInfo.CurrentCulture,
            BillingFileName,
            calculatorRunId);
}