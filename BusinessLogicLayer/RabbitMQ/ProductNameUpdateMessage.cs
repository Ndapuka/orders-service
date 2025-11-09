namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public record ProductNameUpdateMessage(Guid ProductID, String? NewName);
