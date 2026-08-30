using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.DTOs;
using OrderService.Models;
using OrderService.RabbitMQ;
namespace OrderService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly HttpClient _httpClient;

        private readonly RabbitMQPublishers _publisher;

        public OrdersController(OrderDbContext context,IHttpClientFactory httpClientFactory, RabbitMQPublishers publisher)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("ProductService");
            _publisher = publisher;
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orders=await _context.Orders
                .Include(x=>x.OrderItems).ToListAsync();

            var result = orders.Select(order => new OrderDto
            {
                Id = order.Id,
                UserId = order.UserId,
                CreatedAt = order.CreatedAt,
                TotalPrice = order.TotalPrice,

                OrderItems = order.OrderItems.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice

                }).ToList()
            }).ToList();

            return Ok(result);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var orders=await _context.Orders
                .Include(x=>x.OrderItems)
                .FirstOrDefaultAsync(x=>x.Id==id);

            if (orders == null)
                return NotFound();

            var result = new OrderDto
            {
                Id = orders.Id,
                UserId = orders.UserId,
                CreatedAt = orders.CreatedAt,
                TotalPrice = orders.TotalPrice,
                OrderItems = orders.OrderItems.Select(item => new OrderItemDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                }).ToList()
            };
                
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Order order)
        {
            foreach(var item in order.OrderItems)
            {
                var response = await _httpClient.GetAsync($"api/products/{item.ProductId}");

                if(!response.IsSuccessStatusCode)
                {
                    return BadRequest($"Product {item.ProductId} not found");
                }

            }


            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            var orderEvent = new OrderCreatedEvent()
            {
                OrderId=order.Id,
                UserId=order.UserId,
                TotalPrice=order.TotalPrice,
            };

            await _publisher.PublishAsync(orderEvent,"order.created");

            return CreatedAtAction(nameof(GetAll),new {id=order.Id});
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order =await  _context.Orders
                .SingleOrDefaultAsync(o => o.Id==id);
            if (order == null)
                return NotFound();
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
