using Stocker.Features.Stock.StockRanking;

namespace Stocker.Tests.Helpers.TestDataBuilders;

public class RankingRequestBuilder
{
  private int _numberOfStocks = 10;
  private decimal? _minimumMarketCap = null;

  public RankingRequestBuilder WithNumberOfStocks(int count)
  {
    _numberOfStocks = count;
    return this;
  }

  public RankingRequestBuilder WithMinimumMarketCap(decimal? cap)
  {
    _minimumMarketCap = cap;
    return this;
  }

  public RankingRequest Build()
  {
    return new RankingRequest(_numberOfStocks, _minimumMarketCap);
  }

  public static RankingRequestBuilder CreateDefault()
  {
    return new RankingRequestBuilder();
  }
}
