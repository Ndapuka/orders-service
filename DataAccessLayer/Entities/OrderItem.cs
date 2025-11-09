using MongoDB.Bson.Serialization.Attributes;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.Entities;
public class OrderItem
{
    [BsonId] // This attribute indicates that this property is the primary key for the MongoDB document
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid _id { get; set; } = Guid.NewGuid(); // Automatically generate a new Guid for each order item
    [BsonRepresentation(MongoDB.Bson.BsonType.String)] // to represent as a string in MongoDB
    public Guid ProductID { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.Double)]
    public decimal UnitPrice { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.Int32)]

    public int Quantity { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.Double)]
    public decimal TotalPrice { get; set; }

}