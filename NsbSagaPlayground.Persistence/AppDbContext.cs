using Microsoft.EntityFrameworkCore;
using NsbSagaPlayground.Shared.Domain;
using System.Reflection;

namespace NsbSagaPlayground.Persistence;

public class AppDbContext : DbContext
{
  private readonly string _connectionString;

  public DbSet<Order> Orders => Set<Order>(); 
  
  public AppDbContext(string connectionString)
  {
    _connectionString = connectionString;
  }
  
  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.UseSqlServer(_connectionString);
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
  }
}