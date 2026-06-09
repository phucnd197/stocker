using FastEndpoints;
using FluentValidation;
using Stocker.Features.StockRanking.Models;

namespace Stocker.Features.StockRanking.Validators;


public class RankingRequestValidator : Validator<RankingRequest>
{
  public RankingRequestValidator()
  {
    RuleFor(x => x.MinimumMarketcap).GreaterThan(0).When(x => x is not null);
    RuleFor(x => x.NumberOfStocks).GreaterThan(0);
  }
}