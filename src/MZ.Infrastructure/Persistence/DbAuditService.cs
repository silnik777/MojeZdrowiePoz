using MZ.Application.Abstractions;
using MZ.Domain.Entities;
using MZ.Domain.Enums;

namespace MZ.Infrastructure.Persistence;

/// <summary>Zapisuje wpisy logu audytu RODO do bazy (append-only).</summary>
public class DbAuditService : IAuditService
{
    private readonly MzDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _time;

    public DbAuditService(MzDbContext db, ICurrentUser currentUser, TimeProvider time)
    {
        _db = db;
        _currentUser = currentUser;
        _time = time;
    }

    public async Task ZapiszAsync(AkcjaAudytu akcja, string encja, string? encjaId = null,
        Guid? patientId = null, string? szczegolyJson = null, CancellationToken ct = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            DataCzas = _time.GetUtcNow(),
            NazwaLogowania = _currentUser.NazwaLogowania,
            Akcja = akcja,
            Encja = encja,
            EncjaId = encjaId,
            PatientId = patientId,
            SzczegolyJson = szczegolyJson
        });
        await _db.SaveChangesAsync(ct);
    }
}
