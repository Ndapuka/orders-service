using DnsClient.Internal;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;
using Polly.Timeout;
using System.Net.Http.Json;
using System.Text.Json;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;

public class UsersMicroserviceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UsersMicroserviceClient> _logger;
    private readonly IDistributedCache _distributedCache;

    public UsersMicroserviceClient(HttpClient httpClient, ILogger<UsersMicroserviceClient> logger, IDistributedCache distributedCache)
    {
        _httpClient = httpClient;
        _logger = logger;
        _distributedCache = distributedCache;
        
    }

    public async Task<UserDTO?> GetUserByUserID(Guid userID)
    {
        string cacheKey = $"users_{userID}";
        string? cachedUser = await _distributedCache.GetStringAsync(cacheKey);

        if (cachedUser != null) 
        {
            UserDTO? userFromCache = JsonSerializer.Deserialize<UserDTO?>(cachedUser);
            return userFromCache;
        }
        
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/gateway/users/{userID}");
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    UserDTO? userFromFallback = await response.Content.ReadFromJsonAsync<UserDTO>();
                    if (userFromFallback == null)
                    {
                        // Handle the fallback user data, maybe log it or throw an exception
                        throw new NotImplementedException("Fallback user data received from Users Microservice.");
                        
                    }
                    return userFromFallback;
                    
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null; // User not found
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // Handle bad request, maybe log it or throw an exception
                    //throw new HttpRequestException("Bad request to the Users Microservice.", null, System.Net.HttpStatusCode.BadRequest);
                    return new UserDTO(
                        PersonName: "Temporary Unavailable", 
                        Email: "Temporary Unavailable", 
                        UserID: Guid.Empty,
                        Gender: "Temporary Unavailable");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    // Handle internal server error, maybe log it or throw an exception
                    throw new HttpRequestException("Internal server error in the Users Microservice.");
                }
                else
                {
                    // Log the error or throw an exception as needed
                    response.EnsureSuccessStatusCode(); // This will throw an exception for non-success status codes

                }
            }
            UserDTO? user = await response.Content.ReadFromJsonAsync<UserDTO>();
            if (user == null)
            {
                throw new InvalidOperationException("User data is null");
            }

            string userJson = JsonSerializer.Serialize(user);
            DistributedCacheEntryOptions options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(DateTimeOffset.UtcNow.AddSeconds
               (20)).SetAbsoluteExpiration(TimeSpan.FromSeconds(10));
            string cacheKeyToWrite = $"users_{user.UserID}";
            _distributedCache.SetString(cacheKeyToWrite, userJson, options);

            return user; // Return the user data if the request was successful
        }
        catch(BrokenCircuitException ex) 
        {
            _logger.LogError(ex, "Request failed because of circuit breaker is in Open state. Returning dummy data.");

            return new UserDTO(
                PersonName: "Temporary Unavailable (circuitBreaker)", 
                Email: "Temporary Unavailable (circuitBreaker)", 
                UserID: Guid.Empty, 
                Gender: "Temporary Unavailable (circuitBreaker)");
        }
        catch (TimeoutRejectedException ex)
        {
            _logger.LogError(ex, "Timeout occorred while fetching user data. Return dummy data");

            return new UserDTO(
                PersonName: "Temporary Unavailable (timeout)",
                Email: "Temporary Unavailable (timeout)",
                UserID: Guid.Empty,
                Gender: "Temporary Unavailable (timeout)");
        }

    }
}
