using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace StudentDiary.Infrastructure.HttpPolicies
{
    public class PaymentsApiPolicy
    {
        private readonly ILogger<PaymentsApiPolicy> _logger;

        public PaymentsApiPolicy(ILogger<PaymentsApiPolicy> logger)
        {
            _logger = logger;
        }

        public IAsyncPolicy<HttpResponseMessage> GetPolicy()
        {
            var retryPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TaskCanceledException>() // Handle timeouts
                .RetryAsync(
                    retryCount: 2,
                    onRetry: (outcome, retryNumber, context) =>
                    {
                        _logger.LogWarning("Retry {RetryNumber} for request due to: {Exception}",
                            retryNumber, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    });

            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<TaskCanceledException>()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 3,
                    durationOfBreak: TimeSpan.FromSeconds(30),
                    onBreak: (outcome, timespan) =>
                    {
                        _logger.LogError("Circuit opened for {Duration}s due to: {Exception}",
                            timespan.TotalSeconds, outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                    },
                    onReset: () =>
                    {
                        _logger.LogInformation("Circuit closed, service recovered");
                    },
                    onHalfOpen: () =>
                    {
                        _logger.LogInformation("Circuit half-open, testing service...");
                    }
                );

            // Combine policies: retry -> circuit breaker
            return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
        }
    }
}
