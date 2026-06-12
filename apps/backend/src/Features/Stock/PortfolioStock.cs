namespace Stocker.Features.Stock;

public class PortfolioStock
{
    public Guid Id { get; set; }
    public Guid PortfolioId { get; set; }
    public string Company { get; set; }
    public decimal BuyPrice { get; set; }

    public virtual Portfolio Portfolio { get; set; }
}