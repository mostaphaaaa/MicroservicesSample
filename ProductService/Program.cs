
using ProductService.Data;
using Microsoft.EntityFrameworkCore;
using ProductService.RabbitMQ;
using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddDbContext<ProductDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDb"));
});

//RabbitMQ
builder.Services.AddSingleton<RabbitMQPublisher>();
builder.Services.AddHostedService(sp=>sp.GetRequiredService<RabbitMQPublisher>());

//Redis
var redisConnection = builder.Configuration["Redis:ConnectionString"];
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnection!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


app.Run();
