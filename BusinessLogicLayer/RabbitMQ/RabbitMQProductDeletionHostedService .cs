using Microsoft.Extensions.Hosting;
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;
public class RabbitMQProductDeletionHostedService : IHostedService
{
    private readonly IRabbitMQProductDeletionConsumer _productDeletionConsume;
    public RabbitMQProductDeletionHostedService(IRabbitMQProductDeletionConsumer productDeletionConsume)
    {
        _productDeletionConsume = productDeletionConsume;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _productDeletionConsume.InitAsync();
        await _productDeletionConsume.ConsumerAsync();
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _productDeletionConsume.DisposeAsync();
    }
}
