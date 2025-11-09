namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductDeletionConsumer
{
    Task InitAsync();
    Task ConsumerAsync();
    ValueTask DisposeAsync();
}