using System.Security.Claims;
using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Stocker.Infrastructure.Database;

namespace Stocker.Features.Stock.DeletePortfolio;

public record DeletePortfolioRequest(Guid Id);

public class DeletePortfolioEndpoint : Endpoint<DeletePortfolioRequest>
{
    private readonly StockerDataContext _context;

    public DeletePortfolioEndpoint(StockerDataContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Delete("/api/stock/portfolio/{id:guid}");
    }

    public override async Task HandleAsync(DeletePortfolioRequest req, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            AddError("User info not found.");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }
        if (req.Id == Guid.Empty)
        {
            await Send.NotFoundAsync(ct);
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

        _context.Portfolios.Remove(entity);
        await _context.SaveChangesAsync(ct);
    }
}