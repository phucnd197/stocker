namespace Stocker.Features.Stock.StockRanking;

public interface ITradingViewClient
{
  Task<CompanyData[]> FetchAllStockDataAsync(bool refresh, CancellationToken ct);
}
