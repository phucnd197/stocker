using Microsoft.Extensions.Caching.Distributed;
using Stocker.Core.Clients;
using Stocker.Infrastructure.Models;
using TickerQ.Utilities.Base;

namespace Stocker.Infrastructure.Jobs;

public class StockDataRefreshJob
{
    private readonly ITradingViewClient _tradingViewClient;

    public StockDataRefreshJob(ITradingViewClient tradingViewClient)
    {
        _tradingViewClient = tradingViewClient;
    }

    [TickerFunction("daily-stock-refresh", cronExpression: "0 0 0 * * *")]
    public async Task RefreshData()
    {
        await _tradingViewClient.FetchAllStockDataAsync(true, CancellationToken.None);
    }
}
