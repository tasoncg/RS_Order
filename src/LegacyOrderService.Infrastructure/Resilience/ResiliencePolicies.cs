using System;
using System.Net.Http;
using Polly;
using Polly.Extensions.Http;

namespace LegacyOrderService.Infrastructure.Resilience
{
    public static class ResiliencePolicies
    {
        /// <summary>
        /// Full resilience strategy including:
        /// - Retry with exponential backoff and jitter
        /// - Timeout
        /// - Circuit breaker
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetResiliencePolicy()
        {
            var jitter = new Random();

            // Retry with exponential backoff + jitter
            var retry = HttpPolicyExtensions
                .HandleTransientHttpError() // handles 5xx + 408 + HttpRequestException
                .OrResult(msg => (int)msg.StatusCode == 429) // Too Many Requests (rate limit)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromMilliseconds(200 * Math.Pow(2, attempt)) +
                        TimeSpan.FromMilliseconds(jitter.Next(0, 100))
                );

            // Timeout after 5 seconds
            var timeout = Policy
                .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));

            // Circuit breaker: opens after 2 failures, stays open 10 sec
            var circuitBreaker = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 2,
                    durationOfBreak: TimeSpan.FromSeconds(10)
                );

            // Wrap all policies together
            return Policy.WrapAsync(retry, timeout, circuitBreaker);
        }
    }
}
