using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stocker.Features.Stock;

namespace Stocker.Infrastructure.Database.Configurations;

public class PortfolioStockConfiguration : IEntityTypeConfiguration<PortfolioStock>
{
    public void Configure(EntityTypeBuilder<PortfolioStock> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.PortfolioId, x.Company }).IsUnique();
        builder.Property(x => x.Company).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BuyPrice).HasPrecision(18, 4).IsRequired();
        builder.HasOne(x => x.Portfolio).WithMany(x => x.StockSelections).HasForeignKey(x => x.PortfolioId);
        builder.HasQueryFilter(x => !x.Portfolio.IsDeleted);
    }
}