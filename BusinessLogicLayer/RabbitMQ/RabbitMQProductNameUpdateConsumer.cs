using eCommerce.OrdersMicroservice.BusinessLogicLayer.DTO;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
namespace eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

public class RabbitMQProductNameUpdateConsumer : IRabbitMQProductNameUpdateConsumer, IAsyncDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;
    private IConnection _connection;
    private IChannel _channel;
    private readonly IDistributedCache _cache;

    public RabbitMQProductNameUpdateConsumer(IConfiguration configuration, ILogger<RabbitMQProductNameUpdateConsumer> logger, IDistributedCache cache)
    {
        _configuration = configuration;
        _logger = logger;
        _cache = cache;

    }
    public async Task InitAsync()
    {
        Console.WriteLine($"RabbitMQ_Host:{_configuration["RABBITMQ_HOST"]}");
        Console.WriteLine($"RabbitMQ_UserName:{_configuration["RABBITMQ_USERNAME"]}");
        Console.WriteLine($"RabbitMQ_Psw:{_configuration["RABBITMQ_PASSWORD"]}");
        Console.WriteLine($"RabbitMQ_Port:{_configuration["RABBITMQ_PORT"]}");

        var factory = new ConnectionFactory
        {
            HostName = _configuration["RABBITMQ_HOST"]!,
            UserName = _configuration["RABBITMQ_USERNAME"]!,
            Password = _configuration["RABBITMQ_PASSWORD"]!,
            Port = int.Parse(_configuration["RABBITMQ_PORT"]!)
        };

        for (int attempt = 1; attempt <= 5; attempt++)
        {
            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                _logger.LogInformation("Conexão com RabbitMQ estabelecida com sucesso.");
                return;
            }
            catch (BrokerUnreachableException ex)
            {
                _logger.LogWarning(ex, $"Tentativa {attempt}/5 falhou. Repetindo em 5s...");
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
        _logger.LogError("Não foi possível conectar ao RabbitMQ após várias tentativas.");
    }
    

    public async Task ConsumerAsync()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            await InitAsync();
        }
        //string routingKey = "product.update.*"; // topic
        //string routingKey = "product.update.name";
        var headers = new Dictionary<string, object>
        {
            { "x-match", "all" }, //  "all" or "any"
            { "event", "product.update" },
            { "RowCount", 1 }
        }; //Headers exchange
        var queueName = "orders.product.update.name.queue";
        //create exchange
        string exchangeName = _configuration["RABBITMQ_PRODUCTS_EXCHANGE"]!;

        await _channel.ExchangeDeclareAsync(exchangeName, type: ExchangeType.Headers, durable: true, autoDelete: false);

        _logger.LogInformation("Config exchange: {ExchangeName}", exchangeName);

        await _channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: headers); // for method headers x-message-ttl | x-max-lenghth | x-dead-letter-exchange

        //Bind queue to exchange
        await _channel.QueueBindAsync(
            queue: queueName,
            exchange: exchangeName,
            routingKey: string.Empty);

        //Event
        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            _logger.LogInformation("📥 Mensagem recebida na fila {Queue}: {Message}", queueName, message);

            if (message != null)
            {
                ProductDTO? productDTO = System.Text.Json.JsonSerializer.Deserialize<ProductDTO>(message);

                if (productDTO != null)
                {
                    await HandleProductUpdation(productDTO);
                }
                // Acknowledge the message
                await _channel.BasicAckAsync(args.DeliveryTag, multiple: false);
            }
        };
        await _channel.BasicConsumeAsync(
                 queue: queueName,
                 autoAck: false,
                 consumer: consumer);
    }
    private async Task HandleProductUpdation(ProductDTO productDTO)
    {
        //TO DO : UPDATE PRODUCT in ORDERS DATABASE Redis cachi
        _logger.LogInformation($"Product name update: {productDTO.ProductID}, new product: {productDTO.ProductName}");
        string productJson = JsonSerializer.Serialize(productDTO);
        DistributedCacheEntryOptions options = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(20)).SetSlidingExpiration(TimeSpan.FromSeconds(10)); // Set cache expiration time to 10 seconds

        string cacheKeyToWrite = $"product_{productDTO.ProductID}"; // Create a cache key based on the product ID

        await _cache.SetStringAsync(cacheKeyToWrite, productJson, options);
    }
    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
        if (_connection != null) await _connection.DisposeAsync();
    }
}

