using System.Net.Http.Json;
using System.Text.Json;
using Stocker.Features.StockRanking.Constants;
using Stocker.Features.StockRanking.Models;

namespace Stocker.Features.StockRanking.Services;

public class TradingViewDataFetcher
{
  private readonly HttpClient _httpClient;
  private const string BaseUrl = "https://scanner.tradingview.com/vietnam/scan";

  public TradingViewDataFetcher(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<(TradingViewResponse PeData, TradingViewResponse RoaData)> FetchAllStockDataAsync(CancellationToken ct)
  {
    var totalCount = await GetTotalCountAsync(ct);

    var peTask = GetStocksByPeAsync(totalCount, ct);
    var roaTask = GetStocksByRoaAsync(totalCount, ct);

    await Task.WhenAll(peTask, roaTask);

    return (await peTask, await roaTask);
  }

  private async Task<int> GetTotalCountAsync(CancellationToken ct)
  {
    var request = new TradingViewRequest
    {
      Columns = [],
      Range = [1, 2],
      Preset = "all_stocks",
    };

    var response = await PostAsync(request, ct);
    return response.TotalCount;
  }

  private async Task<TradingViewResponse> GetStocksByPeAsync(int maxStocks, CancellationToken ct)
  {
    var request = new TradingViewRequest
    {
      Columns = ColumnDefinitions.PeColumns,
      Range = [0, maxStocks],
      Sort = new SortOption("price_earnings_ttm", "asc"),
      Preset = "all_stocks"
    };

    return await PostAsync(request, ct);
  }

  private async Task<TradingViewResponse> GetStocksByRoaAsync(int maxStocks, CancellationToken ct)
  {
    var request = new TradingViewRequest
    {
      Columns = ColumnDefinitions.RoaColumns,
      Range = [0, maxStocks],
      Sort = new SortOption("return_on_assets_fq", "desc"),
      Preset = "all_stocks"
    };

    return await PostAsync(request, ct);
  }

  private async Task<TradingViewResponse> PostAsync(TradingViewRequest request, CancellationToken ct)
  {
    var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}?label-product=markets-screener", request, ct);

    response.EnsureSuccessStatusCode();

    var responseBody = await response.Content.ReadFromJsonAsync<TradingViewResponse>(ct);
    return responseBody ?? new TradingViewResponse { TotalCount = 0, Data = [] };
  }
}
