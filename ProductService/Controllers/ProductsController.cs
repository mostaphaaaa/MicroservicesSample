using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using ProductService.Data;
using ProductService.Models;
using ProductService.RabbitMQ;
using StackExchange.Redis;
using System.Text.Json;

namespace ProductService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductDbContext _context;
        private readonly RabbitMQPublisher _publisher;

        private readonly StackExchange.Redis.IDatabase _redis;

        public ProductsController(ProductDbContext context, RabbitMQPublisher publisher, IConnectionMultiplexer redis)
        {
            _context = context;
            _publisher = publisher;
            _redis = redis.GetDatabase();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products.ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]

        public async Task<IActionResult> GetById(int id)
        {

            //await _redis.StringSetAsync("test", "hello");

            //var value = await _redis.StringGetAsync("test");

            //Console.WriteLine(value);
            //return Ok();

            var cacheKey=$"product:{id}";

            var cachedProduct=await _redis.StringGetAsync(cacheKey);
            if (cachedProduct.HasValue)
            {
                var deserializedProduct = JsonSerializer.Deserialize<Product>(cachedProduct.ToString());
                

                if (deserializedProduct != null)
                {
                    Console.WriteLine("CACHE HIT");
                    return Ok(deserializedProduct);
                }
            }

            Console.WriteLine("CACHE MISS");
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();


           var serializedProduct=  JsonSerializer.Serialize(product);

            await _redis.StringSetAsync(cacheKey, serializedProduct,TimeSpan.FromMinutes(5));

            return Ok(product);
        }


        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var productEvent = new ProductCreatedEvent
            {
                Id = product.Id,
                Title = product.Title,
                Price = product.Price,
            };

            await _publisher.PublishAsync(productEvent,"product.created");

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Product product)
        {
            if (id != product.Id)
                return BadRequest();

            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
                return NotFound();

            existingProduct.Title = product.Title;
            existingProduct.Price = product.Price;
            existingProduct.Stock = product.Stock;

            await _context.SaveChangesAsync();

            var cacheKey=$"product:{product.Id}";

            await _redis.KeyDeleteAsync(cacheKey);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            var cacheKey = $"product:{product.Id}";

            await _redis.KeyDeleteAsync(cacheKey);

            return NoContent();
        }

    }
}
