using Stocker.Features.Stock.StockRanking;

namespace Stocker.Tests.Helpers.TestDataBuilders;

public class RankingRequestBuilder
{
  private int _numberOfStocks = 10;
  private decimal? _minimumMarketCap = null;
  private bool _refresh = false;

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

  public RankingRequestBuilder WithRefresh(bool refresh)
  {
    _refresh = refresh;
    return this;
  }

  public RankingRequest Build()
  {
    return new RankingRequest(_numberOfStocks, _minimumMarketCap, _refresh);
  }

  public static RankingRequestBuilder CreateDefault()
  {
    return new RankingRequestBuilder();
  }
}
