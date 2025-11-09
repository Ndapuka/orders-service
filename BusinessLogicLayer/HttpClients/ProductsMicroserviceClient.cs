using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.Bulkhead;
using System.Linq.Expressions;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class ProductsMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProductsMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public ProductsMicroserviceClient(HttpClient httpClient, ILogger<ProductsMicroserviceClient> logger, IDistributedCache distributedcache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedcache;
    }
    
    public async Task<ProductDTO?> GetProductByProductID(Guid productID)
    {
        try 
        {
            //Key: product123
            //Value: ProductDTO(ProductID: 123, ProductName: "Product Name", Category: "Category", UnitPrice: 100, QuantityInStock: 50);

            string cacheKey = $"product_{productID}";

            string? cachedPrdoduct = await _distributedCache.GetStringAsync(cacheKey);

            if(cachedPrdoduct != null) 
            {
                ProductDTO? productFromCache = JsonSerializer.Deserialize<ProductDTO>(cachedPrdoduct);
                return productFromCache;
            }

            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/products/search/product-id/{productID}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    ProductDTO? productFromFallback = await response.Content.ReadFromJsonAsync<ProductDTO>();
                    if (productFromFallback == null)
                    {
                        throw new NotImplementedException("Fallback policy was not implemented");
                    }

                    return productFromFallback;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null; // User not found
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Handle bad request, maybe log it or throw an exception
                    throw new HttpRequestException("Bad request to the Products Microservice.", null, System.Net.HttpStatusCode.BadRequest);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    // Handle internal server error, maybe log it or throw an exception
                    throw new HttpRequestException("Internal server error in the Product Microservice.");
                }
                else
                {
                    // Log the error or throw an exception as needed
                    response.EnsureSuccessStatusCode(); // This will throw an exception for non-success status codes

                }
            }

            ProductDTO? product = await response.Content.ReadFromJsonAsync<ProductDTO>();
            if (product == null)
            {
                throw new InvalidOperationException("Product data is null");
            }
            //product: product:{productID}
            //Value: ProductDTO(ProductID: 123, ProductName: "Product Name", Category: "Category", UnitPrice: 100, QuantityInStock: 50);

            string productJson = JsonSerializer.Serialize(product);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(20)).SetSlidingExpiration(TimeSpan.FromSeconds(10)); // Set cache expiration time to 10 seconds

            string cacheKeyToWrite = $"product_{product.ProductID}"; // Create a cache key based on the product ID

            _distributedCache.SetString(cacheKeyToWrite, productJson, options); // Store the product data in the distributed cache

            return product; // Return the product data if the request was successful
        }

        catch (BulkheadRejectedException ex)
        {
            _logger.LogError(ex, "Bulkhead isolation blocks the requerest since the requerest queue is full");

            return new ProductDTO(
                ProductID: Guid.Empty,
                ProductName: "Temporary Unvailable (bulkhead)",
                Category: "Temporary Unvailable (bulkhead)",
                UnitPrice: 0, 
                QuantityInStock: 0
                );
        }
    }
}





