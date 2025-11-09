using eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
using eCommerce.OrdersMicroservice.DataAccessLayer.RepositoryContracts;
using MongoDB.Driver;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.Repositories;
public class OrdersRepository : IOrdersRepository
{

    private readonly IMongoCollection<Order> _orders;
    private readonly string collectionName = "orders"; // to change the current collection name if needed
    public OrdersRepository(IMongoDatabase mongoDatabase)
    {
        // Initialize the repository with the MongoDB database instance
        _orders = mongoDatabase.GetCollection<Order>("orders");
    }
    public async Task<Order?> AddOrder(Order order)
    {
        order.OrderID = Guid.NewGuid(); // Generate a new OrderID for the order
        order._id=order.OrderID; //like same mongodb
        foreach (OrderItem orderItem in order.OrderItems) 
        {
            orderItem._id= Guid.NewGuid();
        }
        await _orders.InsertOneAsync(order); // Insert the order into the MongoDB collection
        return order;
    }

    public async Task<bool> DeleteOrder(Guid orderID)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, orderID);
        Order? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();

        if (existingOrder == null)
        {
            return false; // Order not found
        }
        DeleteResult deleteResult = await _orders.DeleteOneAsync(filter); // Delete the order from the collection
        return deleteResult.DeletedCount > 0; // Return true if the deletion was successful

    }

    public async Task<Order?> GetOrderByCondition(FilterDefinition<Order> filter)
    {
        return (await _orders.FindAsync(filter)).FirstOrDefault();
    }

    public async Task<IEnumerable<Order>> GetOrders()
    {
        return (await _orders.FindAsync(Builders<Order>.Filter.Empty)).ToList();
    }

    public async Task<IEnumerable<Order?>> GetOrdersByCondition(FilterDefinition<Order> filter)
    {
        return (await _orders.FindAsync(filter)).ToList();
    }

    public async Task<Order?> UpdateOrder(Order order)
    {
        FilterDefinition<Order> filter = Builders<Order>.Filter.Eq(temp => temp.OrderID, order.OrderID);
        Order? existingOrder = (await _orders.FindAsync(filter)).FirstOrDefault();

        if (existingOrder == null)
        {
            return null; // Order not found
        }
        order._id = existingOrder._id; // Ensure the _id remains the same for the existing order
        ReplaceOneResult replaceOneResult =  await _orders.ReplaceOneAsync(filter, order); // Update the existing order with the new data

        return order; // Return the updated order object
    }
    
}