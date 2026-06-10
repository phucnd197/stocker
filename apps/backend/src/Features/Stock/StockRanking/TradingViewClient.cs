using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Stocker.Features.Stock.StockRanking;

public record TradingViewRequest
{
  public required string[] Columns { get; init; }
  public required string Preset { get; init; }
  public int[]? Range { get; init; }
  public SortOption? Sort { get; init; }
  public Options Options { get; init; } = new();
}

public record SortOption(string SortBy, string SortOrder);

public record Options
{
  public string Lang { get; init; } = "en";
}

public record TradingViewResponse
{
  public int TotalCount { get; init; }

  public required StockDataPoint[] Data { get; init; }
}

public record StockDataPoint
{
  [JsonPropertyName("d")]
  public required JsonArray Data { get; init; }

  [JsonPropertyName("s")]
  public required string StockIdentifier { get; init; }
}

public class TradingViewClient
{
  private readonly HttpClient _httpClient;
  private const string BaseUrl = "https://scanner.tradingview.com/vietnam/scan";

  public TradingViewClient(HttpClient httpClient)
  {
    _httpClient = httpClient;
  }

  public async Task<(TradingViewResponse PeData, TradingViewResponse RoicData)> FetchAllStockDataAsync(CancellationToken ct)
  {
    var peTask = GetStocksByPeAsync(ct);
    var roicTask = GetStocksByRoicAsync(ct);

    await Task.WhenAll(peTask, roicTask);

    return (await peTask, await roicTask);
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

  private async Task<TradingViewResponse> GetStocksByRoicAsync(CancellationToken ct)
  {
    var request = new TradingViewRequest
    {
      Columns = ColumnDefinitions.RoicColumns,
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
