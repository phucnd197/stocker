using System.Security.Claims;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Stocker.Infrastructure.Database;

namespace Stocker.Features.Stock.UpdatePortfolio;

public record UpdatePortfolioRequest(Guid Id, string Name, string Description, UpdateStockData[] Selections);

public record UpdateStockData(string Company, decimal BuyPrice)
{
    public PortfolioStock ToEntity(Portfolio portfolio)
    {
        return new PortfolioStock
        {
            Company = Company,
            BuyPrice = BuyPrice,
            Portfolio = portfolio,
        };
    }
}

public class UpdatePortfolioRequestValidator : Validator<UpdatePortfolioRequest>
{
    public UpdatePortfolioRequestValidator()
    {
        RuleFor(x => x.Id).Must(x => x != Guid.Empty);
        RuleFor(x => x.Name).MaximumLength(200).NotEmpty();
        RuleFor(x => x.Description).MaximumLength(500).When(x => x is not null);
        RuleForEach(x => x.Selections).ChildRules(selection =>
        {
            selection.RuleFor(x => x.Company).NotEmpty().MaximumLength(200);
            selection.RuleFor(x => x.BuyPrice).GreaterThanOrEqualTo(0);
        });
    }
}

public class UpdatePortfolioEndpoint : Endpoint<UpdatePortfolioRequest>
{
    private readonly StockerDataContext _context;
    private readonly ILogger<UpdatePortfolioEndpoint> _logger;

    public UpdatePortfolioEndpoint(StockerDataContext context, ILogger<UpdatePortfolioEndpoint> logger)
    {
        _context = context;
        _logger = logger;
    }

    public override void Configure()
    {
        Put("/api/stock/portfolio/{id:guid}");
    }

    public override async Task HandleAsync(UpdatePortfolioRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            AddError("User info not found.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        var entity = await _context.Portfolios.FindAsync([req.Id], cancellationToken: ct);
        if (entity is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        if (entity.UserId != userId)
        {
            await Send.ForbiddenAsync(cancellation: ct);
            return;
        }

        using var transaction = await _context.Database.BeginTransactionAsync(ct);
        try
        {
            await _context.PortfolioStocks.Where(x => x.PortfolioId == entity.Id).ExecuteDeleteAsync(ct);

            entity.Name = req.Name;
            entity.Description = req.Description;
            entity.StockSelections = req.Selections.Select(x => x.ToEntity(entity)).ToList();

            await _context.SaveChangesAsync(ct);

            await transaction.CommitAsync(ct);

            await Send.NoContentAsync(cancellation: ct);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            _logger.LogError(ex, "Error when updating portfolio: {e}", ex.Message);
            ThrowError("Error when updating portfolio");
        }
    }
}