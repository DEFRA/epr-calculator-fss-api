using System.ComponentModel.DataAnnotations;

namespace EPR.Calculator.FSS.API.Configs;

public class EprCommonDataApiSettings
{
    public const string SectionName = "EprCommonDataApi";

    [Required]
    [Url]
    public string BaseUrl { get; set; } = string.Empty;
}
