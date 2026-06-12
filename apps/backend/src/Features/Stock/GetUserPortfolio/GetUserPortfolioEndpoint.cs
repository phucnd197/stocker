using System.Security.Claims;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Stocker.Infrastructure.Database;

namespace Stocker.Features.Stock.GetUserPortfolio;

public record GetUserPortfolioRequest(Guid Id);
public record GetUserPortfolioResponse(Guid Id, string Name, string? Description, IList<StockSelection> Selections);
public record StockSelection(string Company, decimal BuyPrice);

public class GetUserPorfolioEndpoint : Endpoint<GetUserPortfolioRequest, GetUserPortfolioResponse>
{
    private readonly StockerDataContext _context;

    public GetUserPorfolioEndpoint(StockerDataContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("/api/stock/portfolio/{id:guid}");
    }

    public override async Task HandleAsync(GetUserPortfolioRequest req, CancellationToken ct)
    {
        var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(user))
        {
            AddError("User infor not found");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        if (req.Id == Guid.Empty)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var data = await _context.Portfolios.Include(x => x.StockSelections).FirstOrDefaultAsync(x => x.Id == req.Id, cancellationToken: ct);
        if (data is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        if (data.UserId != user)
        {
            await Send.ForbiddenAsync(ct);
            return;
        }

        await Send.OkAsync(new GetUserPortfolioResponse(
            data.Id,
            data.Name,
            data.Description,
            data.StockSelections.Select(x => new StockSelection(x.Company, x.BuyPrice)).ToList())
        );
    }
}