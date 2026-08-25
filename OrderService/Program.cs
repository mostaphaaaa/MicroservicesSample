using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<OrderDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDb"));
});


//Add HttpClient For Service Communication
builder.Services.AddHttpClient("ProductService", client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ProductService:BaseUrl"]!
    );
});

//RabbitMQ
builder.Services.AddSingleton<RabbitMQPublishers>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<RabbitMQPublishers>());

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
