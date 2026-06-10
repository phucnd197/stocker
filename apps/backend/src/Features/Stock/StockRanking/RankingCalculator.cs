using System.Text.Json;
using System.Text.Json.Nodes;

namespace Stocker.Features.Stock.StockRanking;


public class RankingCalculator
{
  private static readonly Dictionary<string, Action<CompanyData, JsonNode?>> PropertyMap = new Dictionary<string, Action<CompanyData, JsonNode?>>
  {
    // Basic info
    { "name", (data, value) => data.Name = GetStringValue(value) ?? string.Empty },
    { "description", (data, value) => data.Description = GetStringValue(value) },
    { "stock_exchange", (data, value) => data.StockExchange = GetStringValue(value) },

    // Market data
    { "close", (data, value) => data.Close = GetDecimalValue(value) },
    { "change", (data, value) => data.Change = GetDecimalValue(value) },
    { "volume", (data, value) => data.Volume = GetLongValue(value) },
    { "relative_volume_10d_calc", (data, value) => data.RelativeVolume10dCalc = GetDecimalValue(value) },
    { "market_cap_basic", (data, value) => data.MarketCapBasic = GetDecimalValue(value) },

    // Fundamental currency
    { "fundamental_currency_code", (data, value) => data.FundamentalCurrencyCode = GetStringValue(value) },

    // PE metrics
    { "price_earnings_ttm", (data, value) => data.PriceEarningsTtm = GetDecimalValue(value) },
    { "earnings_per_share_diluted_ttm", (data, value) => data.EarningsPerShareDilutedTtm = GetDecimalValue(value) },
    { "earnings_per_share_diluted_yoy_growth_ttm", (data, value) => data.EarningsPerShareDilutedYoyGrowthTtm = GetDecimalValue(value) },
    { "dividends_yield_current", (data, value) => data.DividendsYieldCurrent = GetDecimalValue(value) },

    // Sector/Market info
    { "sector.tr", (data, value) => data.SectorTr = GetStringValue(value) },
    { "market", (data, value) => data.Market = GetStringValue(value) },
    { "sector", (data, value) => data.Sector = GetStringValue(value) },

    // Profitability metrics (ROA)
    { "gross_margin_ttm", (data, value) => data.GrossMarginTtm = GetDecimalValue(value) },
    { "operating_margin_ttm", (data, value) => data.OperatingMarginTtm = GetDecimalValue(value) },
    { "pre_tax_margin_ttm", (data, value) => data.PreTaxMarginTtm = GetDecimalValue(value) },
    { "net_margin_ttm", (data, value) => data.NetMarginTtm = GetDecimalValue(value) },
    { "free_cash_flow_margin_ttm", (data, value) => data.FreeCashFlowMarginTtm = GetDecimalValue(value) },
    { "return_on_assets_fq", (data, value) => data.ReturnOnAssetsFq = GetDecimalValue(value) },
    { "return_on_equity_fq", (data, value) => data.ReturnOnEquityFq = GetDecimalValue(value) },
    { "return_on_invested_capital_fq", (data, value) => data.ReturnOnInvestedCapitalFq = GetDecimalValue(value) },
    { "research_and_dev_ratio_ttm", (data, value) => data.ResearchAndDevRatioTtm = GetDecimalValue(value) },
  };

  public Dictionary<string, RankedCompany> CalculateRankings(
      TradingViewResponse peData,
      TradingViewResponse roaData)
  {
    var peRank = ExtractRank(peData, ColumnDefinitions.PeColumns);
    var roaRank = ExtractRank(roaData, ColumnDefinitions.RoaColumns);

    return CombineRankings(peRank, roaRank);
  }

  private static Dictionary<string, CompanyRank> ExtractRank(TradingViewResponse response, string[] columns)
  {
    var ranking = new Dictionary<string, CompanyRank>();

    if (response?.Data is null)
      return ranking;

    for (int i = 0; i < response.Data.Length; i++)
    {
      var dataPoint = response.Data[i];
      var recordData = dataPoint.Data;

      var companyData = new CompanyData
      {
        Name = string.Empty,
        StockExchange = dataPoint.StockIdentifier.Split(':')[0]
      };

      // Map columns to properties using the dictionary
      for (int j = 0; j < columns.Length && j < recordData.Count; j++)
      {
        var columnName = columns[j];
        var value = recordData[j];

        if (PropertyMap.TryGetValue(columnName, out var setter))
        {
          setter(companyData, value);
        }
      }

      if (!string.IsNullOrEmpty(companyData.Name))
      {
        ranking[companyData.Name] = new CompanyRank(i, companyData);
      }
    }

    return ranking;
  }

  private static Dictionary<string, RankedCompany> CombineRankings(
      Dictionary<string, CompanyRank> peRank,
      Dictionary<string, CompanyRank> roaRank)
  {
    var finalRank = new Dictionary<string, RankedCompany>();

    foreach (var companyName in peRank.Keys)
    {
      if (!roaRank.ContainsKey(companyName))
        continue;

      var peCompany = peRank[companyName];
      var roaCompany = roaRank[companyName];

      var combinedRank = peCompany.Rank + roaCompany.Rank;

      // Create merged company data from PE data as base
      var companyData = new CompanyData
      {
        // Basic info
        Name = peCompany.CompanyData.Name,
        Description = peCompany.CompanyData.Description,
        StockExchange = peCompany.CompanyData.StockExchange,

        // Market data (from PE)
        Close = peCompany.CompanyData.Close,
        Change = peCompany.CompanyData.Change,
        Volume = peCompany.CompanyData.Volume,
        RelativeVolume10dCalc = peCompany.CompanyData.RelativeVolume10dCalc,
        MarketCapBasic = peCompany.CompanyData.MarketCapBasic,
        FundamentalCurrencyCode = peCompany.CompanyData.FundamentalCurrencyCode,

        // PE metrics
        PriceEarningsTtm = peCompany.CompanyData.PriceEarningsTtm,
        EarningsPerShareDilutedTtm = peCompany.CompanyData.EarningsPerShareDilutedTtm,
        EarningsPerShareDilutedYoyGrowthTtm = peCompany.CompanyData.EarningsPerShareDilutedYoyGrowthTtm,
        DividendsYieldCurrent = peCompany.CompanyData.DividendsYieldCurrent,

        // Sector/Market info
        SectorTr = peCompany.CompanyData.SectorTr,
        Market = peCompany.CompanyData.Market,
        Sector = peCompany.CompanyData.Sector,

        // ROA metrics (from ROA data)
        GrossMarginTtm = roaCompany.CompanyData.GrossMarginTtm,
        OperatingMarginTtm = roaCompany.CompanyData.OperatingMarginTtm,
        PreTaxMarginTtm = roaCompany.CompanyData.PreTaxMarginTtm,
        NetMarginTtm = roaCompany.CompanyData.NetMarginTtm,
        FreeCashFlowMarginTtm = roaCompany.CompanyData.FreeCashFlowMarginTtm,
        ReturnOnAssetsFq = roaCompany.CompanyData.ReturnOnAssetsFq,
        ReturnOnEquityFq = roaCompany.CompanyData.ReturnOnEquityFq,
        ReturnOnInvestedCapitalFq = roaCompany.CompanyData.ReturnOnInvestedCapitalFq,
        ResearchAndDevRatioTtm = roaCompany.CompanyData.ResearchAndDevRatioTtm,

        // Rankings
        CombinedRank = combinedRank,
        PeRank = peCompany.Rank,
        RoaRank = roaCompany.Rank
      };

      finalRank[companyName] = new RankedCompany
      {
        Name = companyName,
        Data = companyData,
        CombinedRank = combinedRank,
        PeRank = peCompany.Rank,
        RoaRank = roaCompany.Rank
      };
    }

    return finalRank;
  }

  private static string? GetStringValue(JsonNode? node)
  {
    if (node is null || node.GetValueKind() == JsonValueKind.Null || node.GetValueKind() == JsonValueKind.Undefined)
      return null;

    if (node.GetValueKind() == JsonValueKind.String)
      return node.ToString();

    return node.ToString();
  }

  private static decimal? GetDecimalValue(JsonNode? node)
  {
    if (node is null || node.GetValueKind() == JsonValueKind.Null || node.GetValueKind() == JsonValueKind.Undefined)
      return null;

    try
    {
      if (node.GetValueKind() == JsonValueKind.Number)
        return node.AsValue().GetValue<decimal>();

      if (node.GetValueKind() == JsonValueKind.String)
        return decimal.TryParse(node.ToString(), out var result) ? result : null;

      return Convert.ToDecimal(node.ToString());
    }
    catch
    {
      return null;
    }
  }

  private static long? GetLongValue(JsonNode? node)
  {
    if (node is null || node.GetValueKind() == JsonValueKind.Null || node.GetValueKind() == JsonValueKind.Undefined)
      return null;

    try
    {
      if (node.GetValueKind() == JsonValueKind.Number)
        return node.AsValue().GetValue<long>();

      if (node.GetValueKind() == JsonValueKind.String)
        return long.TryParse(node.ToString(), out var result) ? result : null;

      return Convert.ToInt64(node.ToString());
    }
    catch
    {
      return null;
    }
  }
}
