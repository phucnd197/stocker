namespace Stocker.Features.Stock.StockRanking;

public static class StockFeatureServiceInjection
{
  public static IServiceCollection AddStockFeatureDependencies(this IServiceCollection services)
  {
    // Register feature services
    services.AddHttpClient<TradingViewClient>();
    services.AddScoped<RankingCalculator>();
    services.AddScoped<StockRankingService>();
    return services;
  }
}