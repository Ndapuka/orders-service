

using Polly;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public interface IProductsPollyPolicies
{

    IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy(Func<Context, CancellationToken, Task<HttpResponseMessage>> fallbackAction);
    IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy(int maxParallelization, int maxQueuingActions);
    Func<Context, CancellationToken, Task<HttpResponseMessage>> GetDefaultFallbackAction();

}
