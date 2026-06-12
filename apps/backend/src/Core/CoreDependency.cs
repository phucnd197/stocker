using Stocker.Core.Clients;

namespace Stocker.Core;

public static class CoreDependency
{
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        // Register feature services
        services.AddHttpClient<ITradingViewClient, TradingViewClient>(configureClient: client =>
        {
            client.BaseAddress = new Uri("https://scanner.tradingview.com/america/scan");
        }).AddStandardResilienceHandler();
        return services;
    }
}