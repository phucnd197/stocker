using FastEndpoints;
using Stocker.Features.StockRanking.Models;
using Stocker.Features.StockRanking.Services;

namespace Stocker.Features.StockRanking.Endpoints;

public class RankStocksEndpoint : EndpointWithoutRequest<RankingResponse>
{
  private readonly StockRankingService _stockRankingService;

  public RankStocksEndpoint(StockRankingService stockRankingService)
  {
    _stockRankingService = stockRankingService;
  }

  public override void Configure()
  {
    Get("/api/stocks/ranking");
    AllowAnonymous();
  }

  public override async Task HandleAsync(CancellationToken ct)
  {
    var result = await _stockRankingService.RankStocksAsync(ct);

    var response = new RankingResponse
    {
      TotalRanked = result.TotalRanked,
      TotalMissingCap = result.TotalMissingCap,
      RankedStocks = result.RankedStocks.ConvertAll(s => s.ToDto()),
      MissingCapStocks = result.MissingCapStocks.ConvertAll(s => s.ToDto()),
    };

    await Send.OkAsync(response, ct);
  }
}
