namespace EPR.Calculator.FSS.API.Configs;

public class EprCalculatorApiSettings
{
    public const string SectionName = "EprCalculatorApi";

    public string BaseUrl { get; set; } = string.Empty;

    public string Scope { get; set; } = string.Empty;
}
