using Stocker.Features.Stock.StockRanking;

namespace Stocker.Tests.Helpers.TestDataBuilders;

public class CompanyDataBuilder
{
  private readonly CompanyData _data = new()
  {
    Name = "TestCompany"
  };

  public CompanyDataBuilder WithName(string name)
  {
    _data.Name = name;
    return this;
  }

  public CompanyDataBuilder WithDescription(string? description)
  {
    _data.Description = description;
    return this;
  }

  public CompanyDataBuilder WithStockExchange(string? stockExchange)
  {
    _data.StockExchange = stockExchange;
    return this;
  }

  // Market data
  public CompanyDataBuilder WithMarketCap(decimal? marketCap)
  {
    _data.MarketCapBasic = marketCap;
    return this;
  }

  public CompanyDataBuilder WithPrice(decimal? price)
  {
    _data.Close = price;
    return this;
  }

  public CompanyDataBuilder WithChange(decimal? change)
  {
    _data.Change = change;
    return this;
  }

  public CompanyDataBuilder WithVolume(long? volume)
  {
    _data.Volume = volume;
    return this;
  }

  // PE metrics
  public CompanyDataBuilder WithPeRatio(decimal? peRatio)
  {
    _data.PriceEarningsTtm = peRatio;
    return this;
  }

  public CompanyDataBuilder WithEps(decimal? eps)
  {
    _data.EarningsPerShareDilutedTtm = eps;
    return this;
  }

  public CompanyDataBuilder WithDividendsYield(decimal? dividendsYield)
  {
    _data.DividendsYieldCurrent = dividendsYield;
    return this;
  }

  // Profitability metrics
  public CompanyDataBuilder WithRoic(decimal? roic)
  {
    _data.ReturnOnInvestedCapitalFq = roic;
    return this;
  }

  public CompanyDataBuilder WithGrossMargin(decimal? grossMargin)
  {
    _data.GrossMarginTtm = grossMargin;
    return this;
  }

  public CompanyDataBuilder WithOperatingMargin(decimal? operatingMargin)
  {
    _data.OperatingMarginTtm = operatingMargin;
    return this;
  }

  public CompanyDataBuilder WithNetMargin(decimal? netMargin)
  {
    _data.NetMarginTtm = netMargin;
    return this;
  }

  // Sector/Market info
  public CompanyDataBuilder WithSector(string? sector)
  {
    _data.Sector = sector;
    return this;
  }

  public CompanyDataBuilder WithMarket(string? market)
  {
    _data.Market = market;
    return this;
  }

  // Rankings
  public CompanyDataBuilder WithCombinedRank(int combinedRank)
  {
    _data.CombinedRank = combinedRank;
    return this;
  }

  public CompanyDataBuilder WithPeRank(int peRank)
  {
    _data.PeRank = peRank;
    return this;
  }

  public CompanyDataBuilder WithRoicRank(int roicRank)
  {
    _data.RoicRank = roicRank;
    return this;
  }

  public CompanyData Build()
  {
    return _data;
  }

  public static CompanyDataBuilder CreateDefault()
  {
    return new CompanyDataBuilder();
  }
}
