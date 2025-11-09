using eCommerce.OrdersMicroservice.BusinessLogicLayer.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.DataAccessLayer.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;

public interface IOrdersService
{
    /// <summary>
    /// Retrivies the list of orders from the orders repository
    /// </summary>
    /// <returns>Returns list os OrdersResponse objescts</returns>
    Task<List<OrderResponse?>> GetOrders(Guid? userID = null);
    /// <summary>
    /// Retrivies the list of orders based on the specified condition from the orders repository
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>return matching orders as OrderReposnse object</returns>
    Task<List<OrderResponse?>> GetOrdersByCondition(FilterDefinition<Order>filter);
    /// <summary>
    /// Retrieves a single order based on the specified condition from the orders repository
    /// </summary>
    /// <param name="filter"></param>
    /// <returns>return matching order object as ordersResponse, or null if not found</returns>
    Task<OrderResponse?> GetOrderByCondition(FilterDefinition<Order> filter);
    /// <summary>
    /// Add (insert) order into the colletion using orders repository
    /// </summary>
    /// <param name="orderAddRequest">order to insert</param>
    /// <returns> return ordersResponse object that contains order details after inserting; or returns null if insertion is null </returns>
    Task<OrderResponse?>AddOrder(OrderAddRequest orderAddRequest);
    /// <summary>
    /// updates the existing order based on the OrderID using the orders repository
    /// </summary>
    /// <param name="orderUpdateRequest">order data to update</param>
    /// <returns>Returns order object after succefull updation; otherwise null</returns>
    Task<OrderResponse?>UpdateOrder(OrderUpdateRequest orderUpdateRequest);
    /// <summary>
    /// Deletes an existing order based on given order ID using the orders repository
    /// </summary>
    /// <param name="orderID">order id to search and delete</param>
    /// <returns>return true if deletion is successful; otherwise false</returns>
    Task<bool> DeleteOrder(Guid orderID);

}
