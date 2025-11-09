using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductDeletionConsumer : IRabbitMQProductDeletionConsumer, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;
    private IConnection _connection;
    private IChannel _channel;
    private readonly IDistributedCache _cache;
    public RabbitMQProductDeletionConsumer(IConfiguration configuration, ILogger<RabbitMQProductNameUpdateConsumer> logger, IDistributedCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
    }
    public async Task InitAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RABBITMQ_HOST"]!,
            UserName = _configuration["RABBITMQ_USERNAME"]!,
            Password = _configuration["RABBITMQ_PASSWORD"]!,
            Port = int.Parse(_configuration["RABBITMQ_PORT"]!)
        };

        
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();
       
    }
    public async Task ConsumerAsync()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            await InitAsync();
        }
        //string routingKey = "product.#"; //topic
        //string routingKey = "product.delete";
        var headers = new Dictionary<string, object?>
        {
            { "x-match", "all" },   //  "all" or "any"
            { "event", "product.delete" },
            { "RowCount", 1 }
        }; //Headers exchange

        var queueName = "orders.product.delete.queue";

        //create exchange
        string exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!;

        //declare exchange
        _logger.LogInformation("➡️ Criando exchange {Exchange}", exchangeName);

        await _channel.ExchangeDeclareAsync(exchangeName, type: ExchangeType.Headers, durable: true, autoDelete: false);

        _logger.LogInformation("Config exchange: {ExchangeName}", exchangeName);

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: headers); // para o type headers x-message-ttl | x-max-lenghth | x-dead-letter-exchange

        //Bind queue to exchange
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            //routingKey: string.Empty,//Fanout
            routingKey: string.Empty,
            arguments: headers);

        //Event
        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            _logger.LogInformation(" Mensagem recebida na fila {Queue}: {Message}", queueName, message);

            if (message != null)
            {
                ProductDeletionMessage? producteDeletionMessage = System.Text.Json.JsonSerializer.Deserialize<ProductDeletionMessage>(message);

                if (producteDeletionMessage != null)
                {
                    await HandleProductDeletion(producteDeletionMessage.ProductID);
                }
                // Acknowledge the message
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
            }
        };
        await _channel.BasicConsumeAsync(
                 queue: queueName,
                 autoAck: true,
                 consumer: consumer);
    }
    private async Task HandleProductDeletion(Guid productID)
    {
        //TO DO : DELETE PRODUCT in ORDERS DATABASE Redis cachi

        _logger.LogInformation($"Product deleted: {productID}");

        string cacheKeyToWrite = $"product_{productID}"; // Create a cache key based on the product ID

        await _cache.RemoveAsync(cacheKeyToWrite);
    }
    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
        if (_connection != null) await _connection.DisposeAsync();
    }
}

