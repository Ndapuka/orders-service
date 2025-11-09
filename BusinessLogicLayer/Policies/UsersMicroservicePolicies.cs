
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;
using System;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class UsersMicroservicePolicies : IUsersMicroservicePolicies
{
    private readonly ILogger<UsersMicroservicePolicies> _logger;
    private readonly IUsersPollyPolicies _pollyPolicies;

    public UsersMicroservicePolicies(ILogger<UsersMicroservicePolicies> logger, IUsersPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }

    


    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var timeoutPolicy = _pollyPolicies.GetTimeoutPolicy(TimeSpan.FromSeconds(5));
        var retryPolicy = _pollyPolicies.GetRetryPolicy(2);
        var circuitBreakerPolicy = _pollyPolicies.GetCircuitBreakerPolicy(3, TimeSpan.FromSeconds(10));

        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(timeoutPolicy, retryPolicy, circuitBreakerPolicy);

        return wrappedPolicy;
    }

}
    


