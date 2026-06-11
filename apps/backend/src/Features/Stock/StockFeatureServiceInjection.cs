
namespace Stocker.Features.Stock.StockRanking;

public static class StockFeatureServiceInjection
{
  public static IServiceCollection AddStockFeatureDependencies(this IServiceCollection services)
  {
    // Register feature services
    services.AddHttpClient<ITradingViewClient, TradingViewClient>(configureClient: client =>
    {
      client.BaseAddress = new Uri("https://scanner.tradingview.com/america/scan");
    }).AddStandardResilienceHandler();

    services.AddScoped<StockRankingService>();
    return services;
  }
}