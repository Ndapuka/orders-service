
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;
using Polly.Wrap;


namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductsMicroservicePolicies : IProductsMicroservicePolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger;
    private readonly IProductsPollyPolicies _pollyPolicies;
    public ProductsMicroservicePolicies(ILogger<ProductsMicroservicePolicies> logger, IProductsPollyPolicies pollyPolicies)
    {
        _logger = logger;
        _pollyPolicies = pollyPolicies;
    }

    public IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy()
    {
        var fallbackAction = _pollyPolicies.GetDefaultFallbackAction();
        var bulkheadPolicy = _pollyPolicies.GetBulkheadIsolationPolicy( 2, 40);
        
        var fallbackPolicy = _pollyPolicies.GetFallbackPolicy(fallbackAction);

       
        AsyncPolicyWrap<HttpResponseMessage> wrappedPolicy = Policy.WrapAsync(fallbackPolicy, bulkheadPolicy);

        return wrappedPolicy;
    }
}
