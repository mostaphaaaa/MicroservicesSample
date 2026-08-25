using System.ComponentModel.DataAnnotations;

namespace ProductService.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
