using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stocker.Core.Domain;

namespace Stocker.Infrastructure.Database.Interceptors;

public class SoftDeleteInterceptors : SaveChangesInterceptor
{
  public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
  {
    if (eventData.Context is null)
    {
      return base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    var entries = eventData.Context.ChangeTracker.Entries<ISoftDeletable>().Where(x => x.State == EntityState.Modified);

    foreach (var softDeletable in entries)
    {
      softDeletable.State = EntityState.Modified;
      softDeletable.Entity.IsDeleted = true;
      softDeletable.Entity.DeletedAt = DateTime.UtcNow;
    }

    return base.SavedChangesAsync(eventData, result, cancellationToken);
  }
}