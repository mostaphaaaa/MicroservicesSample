namespace ProductService.RabbitMQ
{
    public class ProductCreatedEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
    }
}
