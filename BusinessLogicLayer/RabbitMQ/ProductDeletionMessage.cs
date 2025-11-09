namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

record class ProductDeletionMessage(Guid ProductID, String? ProductName);

