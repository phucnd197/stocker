using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using Stocker.Features.StockRanking.Models;
using Stocker.Features.StockRanking.Services;
using Stocker.Features.StockRanking.Validators;

namespace Stocker.Features.StockRanking.Endpoints;

public class RankStocksEndpoint : Endpoint<RankingRequest, RankingResponse>
{
  private readonly StockRankingService _stockRankingService;

  public RankStocksEndpoint(StockRankingService stockRankingService)
  {
    _stockRankingService = stockRankingService;
  }

  public override void Configure()
  {
    Post("/api/stocks/ranking");
    AllowAnonymous();
  }

  public override async Task HandleAsync(RankingRequest request, CancellationToken ct)
  {
    var result = await _stockRankingService.RankStocksAsync(request, ct);

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
