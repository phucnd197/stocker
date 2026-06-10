namespace Stocker.Features.Stock.StockRanking;

public static class StockRankingServiceInjection
{
  public static IServiceCollection AddStockRankingDependencies(this IServiceCollection services)
  {
    // Register feature services
    services.AddHttpClient<TradingViewClient>();
    services.AddScoped<RankingCalculator>();
    services.AddScoped<StockRankingService>();
    return services;
  }
}