using System.Net.Http.Json;
using System.Text.Json;
using Stocker.Features.StockRanking.Constants;
using Stocker.Features.StockRanking.Models;

namespace Stocker.Features.StockRanking.Services;

public class TradingViewClient
{
  private readonly HttpClient _httpClient;
  private const string BaseUrl = "https://scanner.tradingview.com/vietnam/scan";

  public TradingViewClient(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<(TradingViewResponse PeData, TradingViewResponse RoaData)> FetchAllStockDataAsync(CancellationToken ct)
  {
    var peTask = GetStocksByPeAsync(ct);
    var roaTask = GetStocksByRoaAsync(ct);

    await Task.WhenAll(peTask, roaTask);

    return (await peTask, await roaTask);
  }

  private async Task<TradingViewResponse> GetStocksByPeAsync(CancellationToken ct)
  {
    var request = new TradingViewRequest
    {
      Columns = ColumnDefinitions.PeColumns,
      Sort = new SortOption("price_earnings_ttm", "asc"),
      Preset = "all_stocks"
    };

    return await PostAsync(request, ct);
  }

  private async Task<TradingViewResponse> GetStocksByRoaAsync(CancellationToken ct)
  {
    var request = new TradingViewRequest
    {
      Columns = ColumnDefinitions.RoaColumns,
      Sort = new SortOption("return_on_invested_capital_fq", "desc"),
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
