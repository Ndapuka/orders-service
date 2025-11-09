using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Validators;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrdersMicroservice.DataAccessLayer.BusinessLogicLayer.DTO;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.ServiceContracts;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Services;
using Microsoft.Extensions.Options;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.RabbitMQ;

namespace eCommerce.OrdersMicroservice.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        //To do: add business logic layer services into the IoC container
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();

        //services.AddAutoMapper(typeof(Mappers.OrderAddRequestToOrderMappingProfile).Assembly);
        services.AddAutoMapper(typeof
            (Mappers.OrderAddRequestToOrderMappingProfile).Assembly,
            typeof(Mappers.UserDTOToOrdersResponseMappingProfile).Assembly);
        services.AddScoped<IOrdersService, OrdersService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{configuration["REDIS_CACHE_HOST"]}:{configuration["REDIS_CACHE_PORT"]}";
        });
        services.AddSingleton<IRabbitMQProductNameUpdateConsumer, RabbitMQ.RabbitMQProductNameUpdateConsumer>();

        services.AddSingleton<IRabbitMQProductDeletionConsumer, RabbitMQ.RabbitMQProductDeletionConsumer>();
        services.AddHostedService<RabbitMQProductNameUpdateHostedService>();
        services.AddHostedService<RabbitMQProductDeletionHostedService>();

        return services;
    }
}
