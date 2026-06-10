using FastEndpoints;
using FluentValidation;

namespace Stocker.Features.Stock.StockRanking;


public record RankingRequest(decimal? MinimumMarketcap, int NumberOfStocks);

public class RankingResponse
{
  public int TotalRanked { get; set; }
  public int TotalMissingCap { get; set; }
  public List<Stock> RankedStocks { get; set; } = new();
  public List<Stock> MissingCapStocks { get; set; } = new();
}

public class RankingRequestValidator : Validator<RankingRequest>
{
  public RankingRequestValidator()
  {
    RuleFor(x => x.MinimumMarketcap).GreaterThan(0).When(x => x is not null);
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

  public override async Task HandleAsync(RankingRequest request, CancellationToken ct)
  {
    var result = await _stockRankingService.RankStocksAsync(request, ct);

    var response = new RankingResponse
    {
      TotalRanked = result.TotalRanked,
      TotalMissingCap = result.TotalMissingCap,
      RankedStocks = result.RankedStocks.ConvertAll(s => s.ToStock()),
      MissingCapStocks = result.MissingCapStocks.ConvertAll(s => s.ToStock()),
    };

    await Send.OkAsync(response, ct);
  }
}
