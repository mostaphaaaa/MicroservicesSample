using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ProductService.Data;

public class ProductDbContextFactory : IDesignTimeDbContextFactory<ProductDbContext>
{
    public ProductDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(
            "ConnectionStrings__ProductDb");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__ProductDb environment variable is not set.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ProductDbContext>();

        optionsBuilder.UseSqlServer(connectionString);

        return new ProductDbContext(optionsBuilder.Options);
    }
}
