using Microsoft.EntityFrameworkCore;
namespace FreakyFashion.Data; // Adjust if folder differs
public class AppDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Category> Categories { get; set; }
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}