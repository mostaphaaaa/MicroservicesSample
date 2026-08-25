namespace OrderService.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public decimal TotalPrice { get; set; }

        public List<OrderItemDto>? OrderItems { get; set; }
    }
}
