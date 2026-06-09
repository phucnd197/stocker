namespace Stocker.Features.StockRanking.Models;

public record RankingRequest(decimal? MinimumMarketcap, int NumberOfStocks);