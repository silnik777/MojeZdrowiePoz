using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MZ.Application.Abstractions;
using MZ.Domain.Common;

namespace MZ.Infrastructure.Persistence;

/// <summary>
/// Automatycznie wypełnia znaczniki audytowe (kto/kiedy utworzył/zmodyfikował) na encjach
/// dziedziczących po BaseEntity. Szczegółowy log dostępu RODO realizuje IAuditService.
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _time;

    public AuditSaveChangesInterceptor(ICurrentUser currentUser, TimeProvider time)
    {
        _currentUser = currentUser;
        _time = time;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData, InterceptionResult<int> result)
    {
        Wypelnij(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken ct = default)
    {
        Wypelnij(eventData.Context);
        return base.SavingChangesAsync(eventData, result, ct);
    }

    private void Wypelnij(DbContext? ctx)
    {
        if (ctx is null) return;
        var teraz = _time.GetUtcNow();
        var user = _currentUser.NazwaLogowania;

        foreach (var entry in ctx.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.UtworzonoUtc = teraz;
                entry.Entity.UtworzonoPrzez = user;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.ZmodyfikowanoUtc = teraz;
                entry.Entity.ZmodyfikowanoPrzez = user;
            }
        }
    }
}
