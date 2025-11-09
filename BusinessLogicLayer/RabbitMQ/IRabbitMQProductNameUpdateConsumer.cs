using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public interface IRabbitMQProductNameUpdateConsumer
{
    Task InitAsync();
    Task ConsumerAsync();
    ValueTask DisposeAsync();
}
