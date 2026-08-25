using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ProductService.RabbitMQ;

public class RabbitMQPublisher : BackgroundService
{
    private IConnection? _connection;
    private IChannel? _channel;

    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IConfiguration _configuration;

    public RabbitMQPublisher(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_connection is null || !_connection.IsOpen)
                {
                    await ConnectAsync(stoppingToken);
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"RabbitMQ connection failed: {ex.Message}");

                await Task.Delay(
                    TimeSpan.FromSeconds(5),
                    stoppingToken);
            }
        }
    }

    private async Task ConnectAsync(
        CancellationToken cancellationToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:Host"],
            Port = int.Parse(_configuration["RabbitMQ:Port"]!),
            UserName = _configuration["RabbitMQ:UserName"],
            Password = _configuration["RabbitMQ:Password"]
        };

        _connection =
            await factory.CreateConnectionAsync(
                cancellationToken);

        _channel =
            await _connection.CreateChannelAsync(
                cancellationToken: cancellationToken);

        await _channel.ExchangeDeclareAsync(
            exchange: "product.exchange",
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        Console.WriteLine(
            "Connected to RabbitMQ.");
    }

    public async Task PublishAsync(
        object message,
        string routingKey)
    {
        if (_channel is null || !_channel.IsOpen)
        {
            throw new InvalidOperationException(
                "RabbitMQ is not connected.");
        }

        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        await _semaphore.WaitAsync();

        try
        {
            await _channel.BasicPublishAsync(
                exchange: "product.exchange",
                routingKey: routingKey,
                body: body);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (_channel is not null)
            await _channel.CloseAsync(cancellationToken);

        if (_connection is not null)
            await _connection.CloseAsync(cancellationToken);

        _semaphore.Dispose();

        await base.StopAsync(cancellationToken);
    }
}