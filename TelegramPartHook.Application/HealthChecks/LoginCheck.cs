using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace TelegramPartHook.Application.HealthChecks
{
    public class LoginCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, 
                                                        CancellationToken cancellationToken = default)
            => Task.FromResult(HealthCheckResult.Healthy());
    }
}
