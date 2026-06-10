using Stocker.Features.Stock.StockRanking;

namespace Stocker.Features;

public static class ServiceDependencyInjections
{
  public static IServiceCollection AddFeatureDependencies(this IServiceCollection services)
  {
    services.AddStockRankingDependencies();
    return services;
  }
}