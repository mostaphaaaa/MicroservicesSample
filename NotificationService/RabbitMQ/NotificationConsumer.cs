using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace NotificationService.RabbitMQ
{
    public class NotificationConsumer : BackgroundService
    {
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly IConfiguration _configuration;

        public NotificationConsumer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory
            {
                HostName = _configuration["RabbitMQ:Host"],
                Port = int.Parse(_configuration["RabbitMQ:Port"]!),
                UserName = _configuration["RabbitMQ:UserName"],
                Password = _configuration["RabbitMQ:Password"]
            };

            _connection = await factory.CreateConnectionAsync(stoppingToken);
            _channel = await _connection.CreateChannelAsync(
                cancellationToken: stoppingToken);

            await _channel.QueueDeclareAsync(
                queue: "order.queue",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (sender, eventArgs) =>
            {
                var body = eventArgs.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                Console.WriteLine(
                    $"NotificationService received: {message}");

                await _channel.BasicAckAsync(
                    deliveryTag: eventArgs.DeliveryTag,
                    multiple: false,
                    cancellationToken: stoppingToken);
            };

            await _channel.BasicConsumeAsync(
                queue: "order.queue",
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

            Console.WriteLine("NotificationService is listening...");

            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_channel is not null)
                await _channel.CloseAsync(cancellationToken);

            if (_connection is not null)
                await _connection.CloseAsync(cancellationToken);

            await base.StopAsync(cancellationToken);
        }
    }
}
