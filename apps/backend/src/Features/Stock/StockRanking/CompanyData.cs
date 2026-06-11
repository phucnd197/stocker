namespace Stocker.Features.Stock.StockRanking;

public record CompanyRank(int Rank, CompanyData CompanyData);

public class CompanyData
{
  // Basic info
  public required string Name { get; set; }
  public required string Identifier { get; set; }
  public string? Description { get; set; }
  public string? StockExchange { get; set; }

  // Market data
  public decimal? Close { get; set; }
  public decimal? Change { get; set; }
  public long? Volume { get; set; }
  public decimal? RelativeVolume10dCalc { get; set; }
  public decimal? MarketCapBasic { get; set; }

  // Fundamental currency
  public string? FundamentalCurrencyCode { get; set; }

  // PE metrics
  public decimal? PriceEarningsTtm { get; set; }
  public decimal? EarningsPerShareDilutedTtm { get; set; }
  public decimal? EarningsPerShareDilutedYoyGrowthTtm { get; set; }
  public decimal? DividendsYieldCurrent { get; set; }

  // Sector/Market info
  public string? SectorTr { get; set; }
  public string? Market { get; set; }
  public string? Sector { get; set; }

  // Profitability metrics
  public decimal? GrossMarginTtm { get; set; }
  public decimal? OperatingMarginTtm { get; set; }
  public decimal? PreTaxMarginTtm { get; set; }
  public decimal? NetMarginTtm { get; set; }
  public decimal? FreeCashFlowMarginTtm { get; set; }
  public decimal? ReturnOnAssetsFq { get; set; }
  public decimal? ReturnOnEquityFq { get; set; }
  public decimal? ReturnOnInvestedCapitalFq { get; set; }
  public decimal? ResearchAndDevRatioTtm { get; set; }

  // Rankings (set during combination)
  public int CombinedRank { get; set; }
  public int PeRank { get; set; }
  public int RoicRank { get; set; }
}