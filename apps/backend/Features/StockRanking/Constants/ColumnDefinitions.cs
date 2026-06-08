namespace Stocker.Features.StockRanking.Constants;

public static class ColumnDefinitions
{
  public static readonly string[] PeColumns =
  [
      "name", "description", "close", "change", "volume",
        "relative_volume_10d_calc", "market_cap_basic",
        "fundamental_currency_code", "price_earnings_ttm",
        "earnings_per_share_diluted_ttm",
        "earnings_per_share_diluted_yoy_growth_ttm",
        "dividends_yield_current", "sector.tr", "market", "sector"
  ];

  public static readonly string[] RoaColumns =
  [
      "name", "description", "gross_margin_ttm", "operating_margin_ttm",
        "pre_tax_margin_ttm", "net_margin_ttm", "free_cash_flow_margin_ttm",
        "return_on_assets_fq", "return_on_equity_fq",
        "return_on_invested_capital_fq", "research_and_dev_ratio_ttm"
  ];
}
