using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Stocker.Core.Clients;
using Stocker.Features.Stock.StockRanking;
using Stocker.Infrastructure.Models;

namespace Stocker.Core.Clients;

record TradingViewRequest
{
  public required string[] Columns { get; init; }
  public required string Preset { get; init; }
  public int[]? Range { get; init; }
  public SortOption? Sort { get; init; }
  public Options Options { get; init; } = new();
}

record SortOption(string SortBy, string SortOrder);

record Options(string Lang = "en");

record TradingViewResponse
{
  public int TotalCount { get; init; }

  public required StockDataPoint[] Data { get; init; }
}

record StockDataPoint
{
  [JsonPropertyName("d")]
  public required JsonArray Data { get; init; }

  [JsonPropertyName("s")]
  public required string StockIdentifier { get; init; }
}

public interface ITradingViewClient
{
  Task<CompanyData[]> FetchAllStockDataAsync(bool refresh, CancellationToken ct);
}

public class TradingViewClient : ITradingViewClient
{
  private readonly HttpClient _httpClient;
  private readonly IDistributedCache _cache;
  private const string BaseUrl = "https://scanner.tradingview.com/america/scan";

  public TradingViewClient(HttpClient httpClient, IDistributedCache cache)
  {
    _httpClient = httpClient;
    _cache = cache;
  }

  public async Task<CompanyData[]> FetchAllStockDataAsync(bool refresh, CancellationToken ct)
  {
    if (!refresh)
    {
      var cachedString = await _cache.GetStringAsync(CacheKeys.TRADING_VIEW_SCREENER, ct);
      if (!string.IsNullOrEmpty(cachedString))
      {
        var cached = JsonSerializer.Deserialize<CompanyData[]>(cachedString);
        if (cached is { Length: > 0 })
        {
          return cached;
        }
      }
    }

    var request = new TradingViewRequest
    {
      Columns = Screener.Columns,
      Preset = "all_stocks"
    };

    var response = await PostAsync(request, ct);
    var stockData = StockDataPointTransformer.Tranform(response.Data);
    await _cache.SetStringAsync(CacheKeys.TRADING_VIEW_SCREENER, JsonSerializer.Serialize(stockData), new DistributedCacheEntryOptions
    {
      AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
    }, ct);

    return stockData;
  }

  private async Task<TradingViewResponse> PostAsync(TradingViewRequest request, CancellationToken ct)
  {
    var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}?label-product=markets-screener", request, ct);

    response.EnsureSuccessStatusCode();

    var responseBody = await response.Content.ReadFromJsonAsync<TradingViewResponse>(ct);
    return responseBody ?? new TradingViewResponse { TotalCount = 0, Data = [] };
  }
}

static class Screener
{
  public static readonly string[] Columns =
  [
      "name", "description", "close", "change", "volume",
        "relative_volume_10d_calc", "market_cap_basic",
        "fundamental_currency_code", "price_earnings_ttm",
        "earnings_per_share_diluted_ttm",
        "earnings_per_share_diluted_yoy_growth_ttm",
        "dividends_yield_current", "sector.tr", "market", "sector",

      "gross_margin_ttm", "operating_margin_ttm",
        "pre_tax_margin_ttm", "net_margin_ttm", "free_cash_flow_margin_ttm",
        "return_on_assets_fq", "return_on_equity_fq",
        "return_on_invested_capital_fq", "research_and_dev_ratio_ttm"
  ];
}

static class StockDataPointTransformer
{
  private static readonly Dictionary<string, Action<CompanyData, JsonNode?>> PropertyMap = new Dictionary<string, Action<CompanyData, JsonNode?>>
  {
    // Basic info
    { "name", (data, value) => data.Name = GetStringValue(value) ?? string.Empty },
    { "description", (data, value) => data.Description = GetStringValue(value) },
    { "stock_exchange", (data, value) => data.StockExchange = GetStringValue(value) },

    // Market data
    { "close", (data, value) => data.Close = GetDecimalValue(value) },
    { "change", (data, value) => data.Change = GetDecimalValue(value) },
    { "volume", (data, value) => data.Volume = GetLongValue(value) },
    { "relative_volume_10d_calc", (data, value) => data.RelativeVolume10dCalc = GetDecimalValue(value) },
    { "market_cap_basic", (data, value) => data.MarketCapBasic = GetDecimalValue(value) },

    // Fundamental currency
    { "fundamental_currency_code", (data, value) => data.FundamentalCurrencyCode = GetStringValue(value) },

    // PE metrics
    { "price_earnings_ttm", (data, value) => data.PriceEarningsTtm = GetDecimalValue(value) },
    { "earnings_per_share_diluted_ttm", (data, value) => data.EarningsPerShareDilutedTtm = GetDecimalValue(value) },
    { "earnings_per_share_diluted_yoy_growth_ttm", (data, value) => data.EarningsPerShareDilutedYoyGrowthTtm = GetDecimalValue(value) },
    { "dividends_yield_current", (data, value) => data.DividendsYieldCurrent = GetDecimalValue(value) },

    // Sector/Market info
    { "sector.tr", (data, value) => data.SectorTr = GetStringValue(value) },
    { "market", (data, value) => data.Market = GetStringValue(value) },
    { "sector", (data, value) => data.Sector = GetStringValue(value) },

    // Profitability metrics 
    { "gross_margin_ttm", (data, value) => data.GrossMarginTtm = GetDecimalValue(value) },
    { "operating_margin_ttm", (data, value) => data.OperatingMarginTtm = GetDecimalValue(value) },
    { "pre_tax_margin_ttm", (data, value) => data.PreTaxMarginTtm = GetDecimalValue(value) },
    { "net_margin_ttm", (data, value) => data.NetMarginTtm = GetDecimalValue(value) },
    { "free_cash_flow_margin_ttm", (data, value) => data.FreeCashFlowMarginTtm = GetDecimalValue(value) },
    { "return_on_assets_fq", (data, value) => data.ReturnOnAssetsFq = GetDecimalValue(value) },
    { "return_on_equity_fq", (data, value) => data.ReturnOnEquityFq = GetDecimalValue(value) },
    { "return_on_invested_capital_fq", (data, value) => data.ReturnOnInvestedCapitalFq = GetDecimalValue(value) },
    { "research_and_dev_ratio_ttm", (data, value) => data.ResearchAndDevRatioTtm = GetDecimalValue(value) },
  };

  public static CompanyData[] Tranform(StockDataPoint[] stockData)
  {
    var actualData = new CompanyData[stockData.Length];
    for (int i = 0; i < stockData.Length; i++)
    {
      var dataPoint = stockData[i];
      var recordData = dataPoint.Data;

      var companyData = new CompanyData
      {
        Name = string.Empty,
        Identifier = dataPoint.StockIdentifier,
        StockExchange = dataPoint.StockIdentifier.Split(':')[0]
      };

      // Map columns to properties using the dictionary
      for (int j = 0; j < Screener.Columns.Length && j < recordData.Count; j++)
      {
        var columnName = Screener.Columns[j];
        var value = recordData[j];

        if (PropertyMap.TryGetValue(columnName, out var setter))
        {
          setter(companyData, value);
        }
      }
      actualData[i] = companyData;
    }

    return actualData;
  }

  private static string? GetStringValue(JsonNode? node)
  {
    if (node is null || node.GetValueKind() == JsonValueKind.Null || node.GetValueKind() == JsonValueKind.Undefined)
      return null;

    if (node.GetValueKind() == JsonValueKind.String)
      return node.ToString();

    return node.ToString();
  }

  private static decimal? GetDecimalValue(JsonNode? node)
  {
    if (node is null || node.GetValueKind() == JsonValueKind.Null || node.GetValueKind() == JsonValueKind.Undefined)
      return null;

    try
    {
      if (node.GetValueKind() == JsonValueKind.Number)
        return node.AsValue().GetValue<decimal>();

      if (node.GetValueKind() == JsonValueKind.String)
        return decimal.TryParse(node.ToString(), out var result) ? result : null;

      return Convert.ToDecimal(node.ToString());
    }
    catch
    {
      return null;
    }
  }

  private static long? GetLongValue(JsonNode? node)
  {
    if (node is null || node.GetValueKind() == JsonValueKind.Null || node.GetValueKind() == JsonValueKind.Undefined)
      return null;

    try
    {
      if (node.GetValueKind() == JsonValueKind.Number)
        return node.AsValue().GetValue<long>();

      if (node.GetValueKind() == JsonValueKind.String)
        return long.TryParse(node.ToString(), out var result) ? result : null;

      return Convert.ToInt64(node.ToString());
    }
    catch
    {
      return null;
    }
  }
}