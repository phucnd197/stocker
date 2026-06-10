using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Stocker.Entities;

namespace Stocker.Database;

public class StockerDataContext : DbContext
{
  public DbSet<UserProfile> UserProfiles { get; set; }
  // 🛠️ FIX: Must be public and explicitly take the generic type parameter <StockerDataContext>
  public StockerDataContext(DbContextOptions<StockerDataContext> options)
      : base(options)
  {
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockerDataContext).Assembly);
    foreach (var type in modelBuilder.Model.GetEntityTypes())
    {
      if (!typeof(ISoftDeletable).IsAssignableFrom(type.ClrType))
      {
        continue;
      }

      var parameter = Expression.Parameter(type.ClrType, "e");
      var body = Expression.Equal(Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted)), Expression.Constant(false));
      var lambda = Expression.Lambda(body, parameter);

      modelBuilder.Entity(type.ClrType).HasQueryFilter(lambda);
    }
  }
}