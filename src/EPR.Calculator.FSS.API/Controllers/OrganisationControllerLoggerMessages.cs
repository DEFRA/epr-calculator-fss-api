namespace EPR.Calculator.FSS.API.Controllers
{
    public static partial class OrganisationControllerLoggerMessages
    {
        [LoggerMessage(Level = LogLevel.Error, Message = "{ErrorMessage}")]
        public static partial void LogErrorMessage(this ILogger logger, string errorMessage, Exception exception);
    }
}