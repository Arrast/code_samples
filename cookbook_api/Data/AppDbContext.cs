using cookbook_api.Models;
using Microsoft.EntityFrameworkCore;

namespace cookbook_api.Data;

public class AppDbContext : DbContext
{
    public DbSet<Recipe> Recipes { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
}