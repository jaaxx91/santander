using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Bulkhead;
using Polly.Extensions.Http;
using Polly.RateLimit;
using System.Net;

namespace HackerNewsAPI.SDK
{
    public static class NewsApiConfiguration
    {
        // handler lifetime
        private const int DefaultHandlerLifetime = 2;

        //timeout
        private const int DefaultTimeoutInSec = 30;
        private const bool DefaultTimeoutPolicyOn = true;

        //rate limiting
        private const bool DefaultRateLimitingOn = true;
        private const int DefaultRateLimitingNumberOfExecutions = 5;
        private const int DefaultRateLimitingTimeSpanInSec = 1;
        private const int DefaultRateLimitingMaxBurst = 10;

        //bulkhead
        private const bool DefaultBulkHeadPolicyOn = true;
        private const int DefaultBulkHeadPolicyMaxParallelization = 10;
        private const int DefaultBulkHeadPolicyMaxQueuingActions = 20;

        // circuit breaker
        private const bool DefaultCircuitBreakerPolicyOn = true;
        private const int DefaultCircuitBreakerPolicyEventsAllowedBeforeBreaking = 5;
        private const int DefaultCircuitBreakerPolicyDurationOfBreakInSec = 30;

        //retry
        private const bool DefaultRetryPolicyOn = true;
        private const int DefaultRetryCount = 5;
        private const int DefaultRetryBackOffBase = 2;
        private const bool DefaultRetryJitterOn = true;

        public static void AddNewsApi(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (!int.TryParse(configuration["NewsApi:HandlerLifetimeInMin"], out int handlerLifetimeInMin))
            {
                handlerLifetimeInMin = DefaultHandlerLifetime;
            }

            if (!bool.TryParse(configuration["NewsApi:TimeoutPolicyOn"], out bool timeoutPolicyOn))
            {
                timeoutPolicyOn = DefaultTimeoutPolicyOn;
            }

            if (!int.TryParse(configuration["NewsApi:TimeoutInSec"], out int timeoutInSec))
            {
                timeoutInSec = DefaultTimeoutInSec;
            }

            if (!bool.TryParse(configuration["NewsApi:RateLimitingOn"], out bool rateLimitingOn))
            {
                rateLimitingOn = DefaultRateLimitingOn;
            }

            if (!int.TryParse(configuration["NewsApi:RateLimitingNumberOfExecutions"], out int rateLimitingNumberOfExecutions))
            {
                rateLimitingNumberOfExecutions = DefaultRateLimitingNumberOfExecutions;
            }

            if (!int.TryParse(configuration["NewsApi:RateLimitingTimeSpanInSec"], out int rateLimitingTimeSpanInSec))
            {
                rateLimitingTimeSpanInSec = DefaultRateLimitingTimeSpanInSec;
            }

            if (!int.TryParse(configuration["NewsApi:RateLimitingMaxBurst"], out int rateLimitingMaxBurst))
            {
                rateLimitingMaxBurst = DefaultRateLimitingMaxBurst;
            }

            if (!bool.TryParse(configuration["NewsApi:BulkHeadPolicyOn"], out bool bulkHeadPolicyOn))
            {
                bulkHeadPolicyOn = DefaultBulkHeadPolicyOn;
            }

            if (!int.TryParse(configuration["NewsApi:BulkHeadPolicyMaxParallelization"], out int bulkHeadPolicyMaxParallelization))
            {
                bulkHeadPolicyMaxParallelization = DefaultBulkHeadPolicyMaxParallelization;
            }

            if (!int.TryParse(configuration["NewsApi:BulkHeadPolicyMaxQueuingActions"], out int bulkHeadPolicyMaxQueuingActions))
            {
                bulkHeadPolicyMaxQueuingActions = DefaultBulkHeadPolicyMaxQueuingActions;
            }

            if (!bool.TryParse(configuration["NewsApi:CircuitBreakerPolicyOn"], out bool circuitBreakerPolicyOn))
            {
                circuitBreakerPolicyOn = DefaultCircuitBreakerPolicyOn;
            }

            if (!int.TryParse(configuration["NewsApi:CircuitBreakerPolicyEventsAllowedBeforeBreaking"], out int circuitBreakerPolicyEventsAllowedBeforeBreaking))
            {
                circuitBreakerPolicyEventsAllowedBeforeBreaking = DefaultCircuitBreakerPolicyEventsAllowedBeforeBreaking;
            }

            if (!int.TryParse(configuration["NewsApi:CircuitBreakerPolicyDurationOfBreakInSec"], out int circuitBreakerPolicyDurationOfBreakInSec))
            {
                circuitBreakerPolicyDurationOfBreakInSec = DefaultCircuitBreakerPolicyDurationOfBreakInSec;
            }

            if (!bool.TryParse(configuration["NewsApi:RetryPolicyOn"], out bool retryPolicyOn))
            {
                retryPolicyOn = DefaultRetryPolicyOn;
            }

            if (!int.TryParse(configuration["NewsApi:RetryCount"], out int retryCount))
            {
                retryCount = DefaultRetryCount;
            }

            if (!int.TryParse(configuration["NewsApi:RetryBackOffBase"], out int retryBackOffBase))
            {
                retryBackOffBase = DefaultRetryBackOffBase;
            }

            if (!bool.TryParse(configuration["NewsApi:RetryJitterOn"], out bool retryJitterOn))
            {
                retryJitterOn = DefaultRetryJitterOn;
            }

            var httpClientBuilder = services.AddHttpClient<INewsApi, NewsApi>(client =>
            {
                client.BaseAddress = new Uri(configuration["NewsApi:BaseUrl"]);
            })
            .SetHandlerLifetime(TimeSpan.FromMinutes(handlerLifetimeInMin));

            var policies = new List<IAsyncPolicy<HttpResponseMessage>>();

            if (timeoutPolicyOn)
            {
                policies.Add(GetTimeOutPolicy(timeoutInSec));
            }

            if (rateLimitingOn)
            {
                policies.Add(GetRateLimitingPolicy(
                    rateLimitingNumberOfExecutions,
                    rateLimitingTimeSpanInSec,
                    rateLimitingMaxBurst));
            }

            if (bulkHeadPolicyOn)
            {
                policies.Add(GetBulkHeadPolicy(
                    bulkHeadPolicyMaxParallelization,
                    bulkHeadPolicyMaxQueuingActions));
            }

            if (circuitBreakerPolicyOn)
            {
                policies.Add(GetCircuitBreakerPolicy(
                    circuitBreakerPolicyEventsAllowedBeforeBreaking,
                    circuitBreakerPolicyDurationOfBreakInSec));
            }

            if (retryPolicyOn)
            {
                policies.Add(GetRetryPolicy(
                    retryCount,
                    retryBackOffBase,
                retryJitterOn));
            }

            var wrappedPolicy = policies.Aggregate((current, next) => Policy.WrapAsync(next, current));

            httpClientBuilder.AddPolicyHandler(wrappedPolicy);

            httpClientBuilder.AddDefaultLogger();
        }

        private static IAsyncPolicy<HttpResponseMessage> GetTimeOutPolicy(int timeoutInSec)
            => Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(timeoutInSec));

        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
            int retryCount,
            int backOffBase,
            bool jitterOn)
        {
            var jitterer = new Random();

            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .Or<RateLimitRejectedException>()
                .Or<BulkheadRejectedException>()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.InternalServerError)
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: retryCount,
                    sleepDurationProvider: retryAttempt =>
                    {
                        var timespan = TimeSpan.FromSeconds(Math.Pow(backOffBase, retryAttempt));

                        if (jitterOn)
                        {
                            var jitter = TimeSpan.FromMilliseconds(jitterer.Next(0, 1000));
                            timespan += jitter;
                        }

                        return timespan;
                    });
        }

        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
            int handledEventsAllowedBeforeBreaking,
            int durationOfBreakInSec)
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == HttpStatusCode.InternalServerError)
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking,
                    durationOfBreak: TimeSpan.FromSeconds(durationOfBreakInSec)
                );
        }

        private static IAsyncPolicy<HttpResponseMessage> GetRateLimitingPolicy(
            int numberOfExecutions,
            int timeSpanInSec,
            int maxBurst)
        {
            return Policy.RateLimitAsync<HttpResponseMessage>(
                numberOfExecutions: numberOfExecutions,
                perTimeSpan: TimeSpan.FromSeconds(timeSpanInSec),
                maxBurst: maxBurst);
        }

        private static IAsyncPolicy<HttpResponseMessage> GetBulkHeadPolicy(
            int maxParallelization,
            int maxQueuingActions)
        {
            return Policy.BulkheadAsync<HttpResponseMessage>(
                maxParallelization: maxParallelization,
                maxQueuingActions: maxQueuingActions);
        }
    }
}
