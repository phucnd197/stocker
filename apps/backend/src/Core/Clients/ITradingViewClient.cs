using Stocker.Features.Stock.StockRanking;

namespace Stocker.Core.Clients;

public interface ITradingViewClient
{
  Task<CompanyData[]> FetchAllStockDataAsync(bool refresh, CancellationToken ct);
}
