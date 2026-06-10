namespace Stocker.Features.Stock.StockRanking;

public class StockRankingService
{
  private readonly TradingViewClient _dataFetcher;
  private readonly RankingCalculator _calculator;

  public StockRankingService(
      TradingViewClient dataFetcher,
      RankingCalculator calculator)
  {
    _dataFetcher = dataFetcher;
    _calculator = calculator;
  }

  public async Task<RankingResult> RankStocksAsync(RankingRequest request, CancellationToken ct = default)
  {
    // 1. Fetch data from TradingView
    var (peData, roaData) = await _dataFetcher.FetchAllStockDataAsync(ct);

    // 2. Calculate rankings
    var rankedCompanies = _calculator.CalculateRankings(peData, roaData);

    // 3. Filter by market cap
    var (valid, missingCap) = FilterByMarketCap(request.MinimumMarketcap, rankedCompanies);

    valid.Sort((a, b) => a.CombinedRank.CompareTo(b.CombinedRank));

    valid = valid.Take(request.NumberOfStocks).ToList();

    return new RankingResult
    {
      TotalRanked = valid.Count,
      TotalMissingCap = missingCap.Count,
      RankedStocks = valid,
      MissingCapStocks = missingCap
    };
  }

  private static (List<RankedCompany> Valid, List<RankedCompany> MissingCap) FilterByMarketCap(
      decimal? minimumMarketCap,
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
        if (minimumMarketCap is null || marketCap > minimumMarketCap)
        {
          valid.Add(company);
        }
      }
    }

    return (valid, missingCap);
  }
}
