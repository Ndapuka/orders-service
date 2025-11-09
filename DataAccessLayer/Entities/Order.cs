using MongoDB.Bson.Serialization.Attributes;

namespace eCommerce.OrdersMicroservice.DataAccessLayer.Entities;

public class Order
{
    [BsonId] // This attribute indicates that this property is the primary key for the MongoDB document 
    [BsonRepresentation(MongoDB.Bson.BsonType.String)] // to represent as a string in MongoDB
    public Guid _id { get; set; } = Guid.NewGuid(); // Automatically generate a new Guid for each order
    [BsonRepresentation(MongoDB.Bson.BsonType.String)] // to represent as a string in MongoDB
    public Guid OrderID { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid UserID { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public DateTime OrderDate { get; set; }
    [BsonRepresentation(MongoDB.Bson.BsonType.Decimal128)]
    [BsonElement("TotalBill")]
    public decimal Totalbill { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

}




