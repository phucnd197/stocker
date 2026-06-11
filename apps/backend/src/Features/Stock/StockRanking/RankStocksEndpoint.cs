using FastEndpoints;
using FluentValidation;

namespace Stocker.Features.Stock.StockRanking;


public record RankingRequest(int NumberOfStocks, decimal? MinimumMarketCap, bool Refresh);

public class RankingResponse
{
  public int TotalRanked { get; set; }
  public int TotalMissingCap { get; set; }
  public List<CompanyData> RankedStocks { get; set; } = new();
  public List<CompanyData> MissingCapStocks { get; set; } = new();
}

public class RankingRequestValidator : Validator<RankingRequest>
{
  public RankingRequestValidator()
  {
    RuleFor(x => x.MinimumMarketCap).GreaterThan(0).When(x => x is not null);
    RuleFor(x => x.NumberOfStocks).GreaterThan(0);
  }
}

public class RankStocksEndpoint : Endpoint<RankingRequest, RankingResponse>
{
  private readonly StockRankingService _stockRankingService;

  public RankStocksEndpoint(StockRankingService stockRankingService)
  {
    _stockRankingService = stockRankingService;
  }

  public override void Configure()
  {
    Get("/api/stocks/ranking");
  }

  public override async Task HandleAsync(RankingRequest rq, CancellationToken ct)
  {
    var (ranked, missingCap) = await _stockRankingService.RankStocksAsync(rq, ct);

    var response = new RankingResponse
    {
      TotalRanked = ranked.Count,
      TotalMissingCap = missingCap.Count,
      RankedStocks = ranked,
      MissingCapStocks = missingCap,
    };

    await Send.OkAsync(response, ct);
  }
}
