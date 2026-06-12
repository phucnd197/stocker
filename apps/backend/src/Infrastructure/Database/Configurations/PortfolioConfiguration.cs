using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Namotion.Reflection;
using Stocker.Features.Stock;

namespace Stocker.Infrastructure.Database.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired(false);
        builder.Property(x => x.UserId).HasMaxLength(200).IsRequired();
        builder.HasMany(x => x.StockSelections).WithOne(x => x.Portfolio).HasForeignKey(x => x.PortfolioId);
    }
}