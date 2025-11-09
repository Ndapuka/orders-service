
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Polly;
using Polly.Bulkhead;
using Polly.Fallback;


namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;

public class ProductsPollyPolicies : IProductsPollyPolicies
{
    private readonly ILogger<ProductsMicroservicePolicies> _logger;
    public ProductsPollyPolicies(ILogger<ProductsMicroservicePolicies> logger)
    {
        _logger = logger;
    }

    public IAsyncPolicy<HttpResponseMessage> GetBulkheadIsolationPolicy(int maxParallelization, int maxQueuingActions)
    {
       

        AsyncBulkheadPolicy< HttpResponseMessage> policy = Policy.BulkheadAsync<HttpResponseMessage>(
                
                maxParallelization: maxParallelization, // Maximum number of concurrent requests
                maxQueuingActions: maxQueuingActions, // Maximum number of queued requests
                onBulkheadRejectedAsync: async context =>
                {
                    _logger.LogWarning("Bulkhead isolation triggered: Too many concurrent requests. Request rejected.");

                   throw new BulkheadRejectedException("Bulkhead queue is full");
                });
        //You can log available slots outside the delegate, e.g., periodically or before sendings a request
        _logger.LogInformation($"Bulkhead isolation policy initialized with maxParallelization = {maxParallelization}, maxQueuingActions={maxQueuingActions}, " + "availeble execution slots: {Executing}, Available queue slot: {Queued},",
            maxParallelization,
            maxQueuingActions,
            policy.BulkheadAvailableCount,
            policy.QueueAvailableCount);

        return policy;
    }

    public IAsyncPolicy<HttpResponseMessage> GetFallbackPolicy(Func<Context, CancellationToken, Task<HttpResponseMessage>> fallbackAction)
    {
        return Policy<HttpResponseMessage>
            .HandleResult(r=> !r.IsSuccessStatusCode)
            .FallbackAsync(fallbackAction,
            onFallbackAsync:(outcome, context) =>
            {
                _logger.LogWarning("Fallback triggered: The request failed, returning dummy data");
                return Task.CompletedTask;
            });
        //AsyncFallbackPolicy<HttpResponseMessage> policy = Policy
        //    .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
        //    .FallbackAsync(async (context) =>
        //{
        //    _logger.LogWarning("Fallback triggered: The resquest failede, return dummy data");

        //    ProductDTO product = new ProductDTO(
        //        ProductID: Guid.Empty, 
        //        ProductName: "Temporary Unvailable (fallback)", 
        //        Category: "Temporary Unvailable (fallback)", 
        //        UnitPrice: 0, QuantityInStock: 0
        //        );

        //    var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        //    {
        //        Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(product), System.Text.Encoding.UTF8, "application/json")
        //    };

        //    return response; // Return a dummy product as a fallback response


        //});
        //return policy;
    }
    public Func<Context, CancellationToken, Task<HttpResponseMessage>> GetDefaultFallbackAction()
    {
        return async (context, cancellationToken) =>
        {
            _logger.LogWarning("Fallback triggered: returning dummy product.");

            var product = new ProductDTO(
                ProductID: Guid.Empty,
                ProductName: "Temporary Unavailable (fallback)",
                Category: "Temporary Unavailable (fallback)",
                UnitPrice: 0,
                QuantityInStock: 0
            );

            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(product),
                    System.Text.Encoding.UTF8,
                    "application/json")
            };

            return response;
        };
    }


}
