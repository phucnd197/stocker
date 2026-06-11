namespace Stocker.Features.Stock.StockRanking;

public class StockRankingService
{
  private readonly ITradingViewClient _dataFetcher;

  public StockRankingService(
      ITradingViewClient dataFetcher)
  {
    _dataFetcher = dataFetcher;
  }

  public async Task<(List<CompanyData> rankedCompanies, List<CompanyData> missingCap)> RankStocksAsync(RankingRequest request, CancellationToken ct = default)
  {
    // 1. Fetch data from TradingView
    var stockData = await _dataFetcher.FetchAllStockDataAsync(request.Refresh, ct);

    // 2. Filter by market cap
    var (meetMinimumMarketcap, missingCap) = FilterByMarketCap(request.MinimumMarketCap, stockData);

    // 3. Calculate rankings
    var rankedCompanies = CalculateRankings(meetMinimumMarketcap);

    rankedCompanies.Sort((a, b) => a.CombinedRank.CompareTo(b.CombinedRank));

    return (rankedCompanies, missingCap);
  }

  private static (List<CompanyData> Valid, List<CompanyData> MissingCap) FilterByMarketCap(
      decimal? minimumMarketCap,
      CompanyData[] rankedCompanies)
  {
    var valid = new List<CompanyData>();
    var missingCap = new List<CompanyData>();

    foreach (var company in rankedCompanies)
    {
      var marketCap = company.MarketCapBasic;

      if (marketCap == null)
      {
        missingCap.Add(company);
      }
      else if (minimumMarketCap is null || marketCap > minimumMarketCap)
      {
        valid.Add(company);
      }
    }

    return (valid, missingCap);
  }

  public List<CompanyData> CalculateRankings(List<CompanyData> stockData)
  {
    var peList = stockData.OrderBy(x => x.PriceEarningsTtm).ToArray();
    var peRanks = new Dictionary<string, int>();
    for (var i = 0; i < peList.Length; i++)
    {
      peRanks.Add(peList[i].Identifier, i);
    }

    var roicList = stockData.OrderByDescending(x => x.ReturnOnInvestedCapitalFq).ToList();
    var roicRanks = new Dictionary<string, int>();
    for (var i = 0; i < peList.Length; i++)
    {
      roicRanks.Add(roicList[i].Identifier, i);
    }

    foreach (var stock in stockData)
    {
      var companyName = stock.Identifier;
      if (!roicRanks.ContainsKey(companyName))
        continue;

      var peRank = peRanks[companyName];
      var roicRank = roicRanks[companyName];

      stock.PeRank = peRank;
      stock.RoicRank = roicRank;
      stock.CombinedRank = peRank + roicRank;
    }

    return stockData;
  }
}
