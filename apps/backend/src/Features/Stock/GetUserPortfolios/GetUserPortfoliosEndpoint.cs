using System.Security.Claims;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Stocker.Infrastructure.Database;

namespace Stocker.Features.Stock.GetUserPortfolios;

public record PortfoliosResponse(IList<PortfolioData> Portfolios);
public class PortfolioData
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class GetUserPorfolios : EndpointWithoutRequest<PortfoliosResponse>
{
    private readonly StockerDataContext _context;

    public GetUserPorfolios(StockerDataContext context)
    {
        _context = context;
    }

    public override void Configure()
    {
        Get("/api/stock/portfolio");
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var user = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(user))
        {
            AddError("User infor not found");
            await Send.ErrorsAsync(cancellation: ct);
            return;
        }

        var data = await _context.Portfolios
            .Where(x => x.UserId == user)
            .Select(x => new PortfolioData
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description
            })
            .ToListAsync(ct);
        await Send.OkAsync(new PortfoliosResponse(data));
    }
}