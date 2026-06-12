using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Stocker.Core.Domain;

namespace Stocker.Infrastructure.Database.Interceptors;

public class SoftDeleteInterceptors : SaveChangesInterceptor
{
  public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
  {
    if (eventData.Context is null)
    {
      return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    SoftDeleteEntries(eventData);

    return base.SavingChangesAsync(eventData, result, cancellationToken);
  }

  public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
  {
    if (eventData.Context is null)
    {
      return base.SavingChanges(eventData, result);
    }

    SoftDeleteEntries(eventData);

    return base.SavingChanges(eventData, result);
  }

  private static void SoftDeleteEntries(DbContextEventData eventData)
  {
    var entries = eventData.Context.ChangeTracker.Entries<ISoftDeletable>().Where(x => x.State == EntityState.Deleted);

    foreach (var softDeletable in entries)
    {
      softDeletable.State = EntityState.Modified;
      softDeletable.Entity.IsDeleted = true;
      softDeletable.Entity.DeletedAt = DateTime.UtcNow;
    }
  }
}