using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Stocker.Entities;

namespace Stocker.Database.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
  public void Configure(EntityTypeBuilder<UserProfile> builder)
  {
    builder.HasKey(x => x.Id);
    builder.Property(x => x.Address).HasMaxLength(500);
    builder.Property(x => x.Image).HasMaxLength(200);
    builder.Property(x => x.Phone).HasMaxLength(20);
    builder.Property(x => x.Nickname).HasMaxLength(100);
  }
}