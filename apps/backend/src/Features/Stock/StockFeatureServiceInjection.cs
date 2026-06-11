
namespace Stocker.Features.Stock.StockRanking;

public static class StockFeatureServiceInjection
{
  public static IServiceCollection AddStockFeatureDependencies(this IServiceCollection services)
  {
    services.AddScoped<StockRankingService>();
    return services;
  }
}