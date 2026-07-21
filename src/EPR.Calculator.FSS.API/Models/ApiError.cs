namespace EPR.Calculator.FSS.API.Models;

public record ApiError
{
    public required string Error { get; init; }

    public required string Message { get; init; }

    public required string Description { get; init; }

    public required int StatusCode { get; init; }

    public required string ErrorCode { get; init; }
}
