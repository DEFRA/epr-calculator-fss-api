namespace EPR.Calculator.FSS.API.Common.Models;

public class ApiError
{
    public string Error { get; set; }

    public string Message { get; set; }

    public string Description { get; set; }

    public int StatusCode { get; set; }

    public string ErrorCode { get; set; }
}