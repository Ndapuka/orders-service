using Microsoft.Extensions.Hosting;
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateHostedService : IHostedService
{
    private readonly IRabbitMQProductNameUpdateConsumer _productNameUpdateConsume;
    public RabbitMQProductNameUpdateHostedService(IRabbitMQProductNameUpdateConsumer consumer)
    {
        _productNameUpdateConsume = consumer;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _productNameUpdateConsume.InitAsync();
            await _productNameUpdateConsume.ConsumerAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar o RabbitMQProductNameUpdateHostedService: {ex.Message}");

        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _productNameUpdateConsume.DisposeAsync();

    }
}
