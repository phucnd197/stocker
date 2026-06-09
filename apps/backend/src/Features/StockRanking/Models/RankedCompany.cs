namespace Stocker.Features.StockRanking.Models;

public class RankedCompany
{
  public required string Name { get; set; }
  public required CompanyData Data { get; set; }
  public int CombinedRank { get; set; }
  public int PeRank { get; set; }
  public int RoaRank { get; set; }

  public Stock ToStock()
  {
    return new Stock
    {
      Name = Name,
      CombinedRank = CombinedRank,
      PeRank = PeRank,
      RoaRank = RoaRank,
      MarketCap = Data.MarketCapBasic,
      Price = Data.Close,
      Change = Data.Change,
      Volume = Data.Volume,
      PeRatio = Data.PriceEarningsTtm,
      Roa = Data.ReturnOnAssetsFq,
      Eps = Data.EarningsPerShareDilutedTtm,
      DividendsYield = Data.DividendsYieldCurrent,
      Description = Data.Description,
      Sector = Data.Sector,
      Market = Data.Market,
      StockExchange = Data.StockExchange
    };
  }
}
