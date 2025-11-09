using eCommerce.OrdersMicroservice.DataAccessLayer;

using FluentValidation.AspNetCore;
using eCommerce.OrdersMicroservice.API.Middleware;
using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.HttpClients;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
using Microsoft.Extensions.DependencyInjection;
using Polly;

var builder = WebApplication.CreateBuilder(args);

//Add DAL and BLL servives
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers();

//FluentValidation
builder.Services.AddFluentValidationAutoValidation();

//Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


// Cors
builder.Services.AddCors(options => {
    options.AddPolicy("CorsPolicy", builder => {
        builder
        .WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials()
        .SetIsOriginAllowed(origin => true); // for dev
    });
});

builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();
builder.Services.AddTransient<IUsersPollyPolicies, UsersPollyPolicies>();

builder.Services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();

builder.Services.AddTransient<IProductsPollyPolicies, ProductsPollyPolicies>();

// Add HttpClient for UsersMicroservice
builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
})
    
    .AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetCombinedPolicy()

);

//Add HttpClient for ProductsMicroservice
builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
})
    .AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IProductsMicroservicePolicies>().GetCombinedPolicy()

    );

var app = builder.Build();

app.UseExceptionHandlingMiddleware();

// Enable CORS
app.UseCors("CorsPolicy");

//Use Routing
app.UseRouting();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Use Authorization
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

//Endpoints
app.MapControllers();

app.Run();