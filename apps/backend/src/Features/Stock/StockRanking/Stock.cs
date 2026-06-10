namespace Stocker.Features.Stock.StockRanking;

public class Stock
{
  public required string Name { get; set; }
  public int CombinedRank { get; set; }
  public int PeRank { get; set; }
  public int RoicRank { get; set; }

  // Market data
  public decimal? MarketCap { get; set; }
  public decimal? Price { get; set; }
  public decimal? Change { get; set; }
  public long? Volume { get; set; }

  // Valuation metrics
  public decimal? PeRatio { get; set; }
  public decimal? Eps { get; set; }
  public decimal? Roic { get; set; }
  public decimal? DividendsYield { get; set; }

  // Company info
  public string? Description { get; set; }
  public string? Sector { get; set; }
  public string? Market { get; set; }
  public string? StockExchange { get; set; }
}