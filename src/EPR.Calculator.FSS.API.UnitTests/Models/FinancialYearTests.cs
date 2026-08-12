using System.ComponentModel;
using EPR.Calculator.FSS.API.Models;

namespace EPR.Calculator.FSS.API.UnitTests.Models;

[TestClass]
public class FinancialYearTests
{
    private static readonly TypeConverter Converter =
        TypeDescriptor.GetConverter(typeof(FinancialYear));

    [TestMethod]
    public void ToRelativeYear_ReturnsExpectedRelativeYear() =>
        Assert.AreEqual(new RelativeYear(2025), new FinancialYear("2025-26").ToRelativeYear());

    [TestMethod]
    public void ToString_ReturnsOriginalValue() =>
        Assert.AreEqual("2025-26", new FinancialYear("2025-26").ToString());

    [TestMethod]
    public void TypeConverter_CanConvertFromString() =>
        Assert.IsTrue(Converter.CanConvertFrom(typeof(string)));

    [TestMethod]
    public void TypeConverter_ConvertsValidFinancialYear()
    {
        var result = (FinancialYear)Converter.ConvertFrom("2025-26")!;
        Assert.AreEqual("2025-26", result.Value);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("2025")]
    [DataRow("2025/26")]
    [DataRow("2025-27")]
    [DataRow("2000-01")]
    [DataRow("abcd-ef")]
    public void TypeConverter_ThrowsFormatException_ForInvalidFinancialYear(string value) =>
        Assert.ThrowsExactly<FormatException>(() => Converter.ConvertFrom(value));
}
