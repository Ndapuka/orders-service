using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.BusinessLogicLayer.DTO;

public record OrderResponse(Guid OrderID, Guid UserID, decimal TotalBill, DateTime OrderDate, List<OrderItemResponse> OrderItems, string PersonName, string Email)
{
    public OrderResponse() : this(default, default, default, default, default, string.Empty, string.Empty)
    {
        // Default constructor for deserialization or initialization
    }
}
