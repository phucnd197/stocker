using System.Data;
using System.Security.Claims;
using FastEndpoints;
using FluentValidation;
using Stocker.Features.Stock;
using Stocker.Infrastructure.Database;

namespace Stocker.Features.Stock.CreatePortfolio;

public record CreatePortfolioRequest(string Name, string Description, CreateStockData[] Selections)
{
    public Portfolio ToEntity(string userId)
    {
        var portfolio = new Portfolio
        {
            Name = Name,
            Description = Description,
            UserId = userId,
        };
        portfolio.StockSelections = Selections.Select(x => new PortfolioStock
        {
            Company = x.Company,
            BuyPrice = x.BuyPrice,
            Portfolio = portfolio,
        }).ToList();
        return portfolio;
    }
}

public record CreateStockData(string Company, decimal BuyPrice);

public class CreatePortfolioRequestValidator : Validator<CreatePortfolioRequest>
{
    public CreatePortfolioRequestValidator()
    {
        RuleFor(x => x.Name).MaximumLength(200).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500);
        RuleForEach(x => x.Selections).ChildRules(selection =>
        {
            selection.RuleFor(x => x.Company).NotEmpty().MaximumLength(200);
            selection.RuleFor(x => x.BuyPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class CreatePortfolioEndpoint : Endpoint<CreatePortfolioRequest>
{
    private readonly StockerDataContext _context;

    public CreatePortfolioEndpoint(StockerDataContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Post("/api/stock/portfolio");
    }

    public override async Task HandleAsync(CreatePortfolioRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            AddError("User info not found.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var entity = req.ToEntity(userId);
        await _context.Portfolios.AddAsync(entity, ct);
        await _context.SaveChangesAsync(ct);
        await Send.NoContentAsync(cancellation: ct);
    }
}