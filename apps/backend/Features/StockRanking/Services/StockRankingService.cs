using Stocker.Features.StockRanking.Models;

namespace Stocker.Features.StockRanking.Services;

public class StockRankingService
{
  private readonly TradingViewDataFetcher _dataFetcher;
  private readonly RankingCalculator _calculator;

  public StockRankingService(
      TradingViewDataFetcher dataFetcher,
      RankingCalculator calculator)
  {
    _dataFetcher = dataFetcher;
    _calculator = calculator;
  }

  public async Task<RankingResult> RankStocksAsync(CancellationToken ct = default)
  {
    // 1. Fetch data from TradingView
    var (peData, roaData) = await _dataFetcher.FetchAllStockDataAsync(ct);

    // 2. Calculate rankings
    var rankedCompanies = _calculator.CalculateRankings(peData, roaData);

    // 3. Filter by market cap
    var (valid, missingCap) = FilterByMarketCap(rankedCompanies);

    valid.Sort((a, b) => a.CombinedRank.CompareTo(b.CombinedRank));

    return new RankingResult
    {
      TotalRanked = valid.Count,
      TotalMissingCap = missingCap.Count,
      RankedStocks = valid,
      MissingCapStocks = missingCap
    };
  }

  private static (List<RankedCompany> Valid, List<RankedCompany> MissingCap) FilterByMarketCap(
      Dictionary<string, RankedCompany> rankedCompanies)
  {
    var valid = new List<RankedCompany>();
    var missingCap = new List<RankedCompany>();

    foreach (var company in rankedCompanies.Values)
    {
      var marketCap = company.Data.MarketCapBasic;

      if (marketCap == null)
      {
        missingCap.Add(company);
      }
      else
      {
        valid.Add(company);
      }
    }

    return (valid, missingCap);
  }
}


public class RankingResult
{
  public int TotalRanked { get; set; }
  public int TotalMissingCap { get; set; }
  public List<RankedCompany> RankedStocks { get; set; } = new();
  public List<RankedCompany> MissingCapStocks { get; set; } = new();
}
