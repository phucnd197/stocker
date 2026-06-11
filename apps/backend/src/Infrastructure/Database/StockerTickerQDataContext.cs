using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using TickerQ.EntityFrameworkCore.DbContextFactory;

namespace Stocker.Infrastructure.Database;

public class StockerTickerQDataContext : TickerQDbContext
{
  public StockerTickerQDataContext(DbContextOptions<TickerQDbContext> options) : base(options)
  {
  }
}

public class StockerTickerQDbContextFactory : IDesignTimeDbContextFactory<StockerTickerQDataContext>
{
  public StockerTickerQDataContext CreateDbContext(string[] args)
  {
    var optionsBuilder = new DbContextOptionsBuilder<TickerQDbContext>();
    optionsBuilder.UseSqlServer("Server=localhost;Database=StockerTickerQ;User Id=sa;Password=123456a@A;TrustServerCertificate=True;");

    return new StockerTickerQDataContext(optionsBuilder.Options);
  }
}