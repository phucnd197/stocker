using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Stocker.Features.StockRanking.Models;

public record TradingViewRequest
{
  public required string[] Columns { get; init; }
  public required int[] Range { get; init; }
  public required string Preset { get; init; }
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
