namespace EPR.Calculator.FSS.API.Models;

public class ApiError
{
    public required string Error { get; set; }

    public required string Message { get; set; }

    public required string Description { get; set; }

    public required int StatusCode { get; set; }

    public required string ErrorCode { get; set; }
}
