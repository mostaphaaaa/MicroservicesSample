namespace OrderService.RabbitMQ
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }

        public decimal TotalPrice { get; set; }
    }
}
