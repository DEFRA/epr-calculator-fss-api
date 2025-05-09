using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics.CodeAnalysis;

namespace EPR.Calculator.FSS.API.HealthCheck
{
    [ExcludeFromCodeCoverage]
    public static class HealthCheckOptionsBuilder
    {
        public static HealthCheckOptions Build()
        {
            return new HealthCheckOptions
            {
                AllowCachingResponses = false,
                ResultStatusCodes =
                {
                    [HealthStatus.Healthy] = StatusCodes.Status200OK,
                },
            };
        }
    }
}
