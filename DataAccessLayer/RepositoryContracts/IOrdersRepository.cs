
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;

 public interface IOrdersRepository 
{
    /// <summary>
    /// Retrivies all orders asynchronomously.
    /// </summary>
    /// <returns>Return all Orders all orders from the orders collection</returns>
    Task<IEnumerable<Order>> GetOrders();
    /// <summary>
    /// Retriveis all orders based on the specifieed condition asynchronomously.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>return a collection of mathing orders</returns>
    Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order>filter);
    /// <summary>
    /// Retrieves a single order based on the specified condition asynchronously.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>Return matching order</returns>

    Task <Order?> GetOrderByCondition(FilterDefinition<Order> filter);

    /// <summary>
    /// retrieves a new Order into collection asynchronously.
    /// </summary>
    /// <param name="order"></param>
    /// <returns>Return the added orded obejct or null if unsuccessful</returns>

    Task<Order?> AddOrder(Order order);
    /// <summary>
    /// Update an existing order asynchronously.
    /// </summary>
    /// <param name="order"></param>
    /// <returns>return the update order object or null if not found</returns>
    Task<Order?> UpdateOrder(Order order);

    /// <summary>
    /// Deletes an existing order asynchronously.
    /// </summary>
    /// <param name="orderID">The order ID based on which we need to delete the order</param>
    /// <returns>return true if the deletion is successful </returns>
    Task<bool> DeleteOrder(Guid orderID);


}
    
   

