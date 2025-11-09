
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersPollyPolicies : IUsersPollyPolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;
    public UsersPollyPolicies(ILogger<UsersMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    

    public IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount)
    {
        AsyncRetryPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
             .WaitAndRetryAsync(
            retryCount: retryCount, 
            sleepDurationProvider: retryAttempt 
            => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),//Delay betwen retries
             onRetry: (outcome, timespan, retryAttempt, context) =>
             {
                 _logger.LogInformation($"Retry {retryAttempt} after {timespan.TotalSeconds} seconds");
             });
     
        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(int handledEventsAllowedBeforeBreaking, TimeSpan durationOfBreak)
    {
        AsyncCircuitBreakerPolicy<HttpResponseMessage> policy = Policy.HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
             .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: handledEventsAllowedBeforeBreaking, 
             durationOfBreak: durationOfBreak,
             onBreak: (outcome, timespan ) =>
             {
             _logger.LogInformation($"Circuit breaker opened for {timespan.TotalMinutes} minutes due to consecutives 3 failure. The subsequente requests will be bloqued");
             },
             onReset: () => { _logger.LogInformation($"Circuit breaker closed. The subsequent requersts will be allowed");
             });

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(TimeSpan timeout)
    {
        AsyncTimeoutPolicy<HttpResponseMessage> policy = Policy.TimeoutAsync<HttpResponseMessage>(timeout: TimeSpan.FromSeconds(10),
            timeoutStrategy: TimeoutStrategy.Pessimistic,
            onTimeoutAsync: async (context, timespan, task, exception) =>
            { 
            _logger.LogWarning($"Request timed out after {timespan.TotalSeconds} seconds. The request will be cancelled.");
        });

        return policy;
    }

    
}
    


