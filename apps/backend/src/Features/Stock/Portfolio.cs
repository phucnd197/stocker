using Stocker.Core.Domain;

namespace Stocker.Features.Stock;

public class Portfolio : ISoftDeletable
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public virtual IList<PortfolioStock> StockSelections { get; set; }
}