using Microsoft.EntityFrameworkCore;
using MZ.Application.Abstractions;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Domain.Services;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

/// <summary>Operacje na przypadkach udziału w programie (pipeline).</summary>
public class EnrollmentService
{
    private readonly MzDbContext _db;
    private readonly IAuditService _audit;
    private readonly TimeProvider _time;

    public EnrollmentService(MzDbContext db, IAuditService audit, TimeProvider time)
    {
        _db = db;
        _audit = audit;
        _time = time;
    }

    private DateOnly Dzis => DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

    /// <summary>Tworzy nowy przypadek dla pacjenta (status początkowy: NowaAnkieta).</summary>
    public async Task<ProgramEnrollment> UtworzAsync(Guid patientId, CancellationToken ct = default)
    {
        var e = new ProgramEnrollment
        {
            PatientId = patientId,
            Status = PipelineStatus.NowaAnkieta,
            DataRozpoczecia = Dzis
        };
        _db.Enrollments.Add(e);
        _db.PipelineHistory.Add(new PipelineHistory
        {
            Enrollment = e, StatusZ = null, StatusNa = e.Status, DataPrzejscia = _time.GetUtcNow(),
            Komentarz = "Utworzenie przypadku"
        });
        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Utworzenie, nameof(ProgramEnrollment), e.Id.ToString(), patientId, ct: ct);
        return e;
    }

    public Task<List<ProgramEnrollment>> ListaAsync(CancellationToken ct = default) =>
        _db.Enrollments.AsNoTracking()
            .Include(e => e.Patient)
            .OrderBy(e => e.Status).ThenBy(e => e.Patient.Nazwisko)
            .Take(500).ToListAsync(ct);

    public Task<ProgramEnrollment?> PobierzZeSzczegolamiAsync(Guid id, CancellationToken ct = default) =>
        _db.Enrollments
            .Include(e => e.Patient)
            .Include(e => e.Ankieta!).ThenInclude(a => a.Odpowiedzi)
            .Include(e => e.Kwalifikacja!)
            .Include(e => e.Skierowanie!).ThenInclude(s => s.Zlecone).ThenInclude(z => z.TestDefinition)
            .Include(e => e.Skierowanie!).ThenInclude(s => s.Laboratorium)
            .Include(e => e.Wyniki).ThenInclude(w => w.TestDefinition)
            .Include(e => e.Terminy).ThenInclude(t => t.Laboratorium)
            .Include(e => e.Historia)
            .FirstOrDefaultAsync(e => e.Id == id, ct);

    /// <summary>Zmienia status przypadku z walidacją dozwolonych przejść i zapisem historii.</summary>
    public async Task ZmienStatusAsync(Guid id, PipelineStatus nowy, string? komentarz = null, CancellationToken ct = default)
    {
        var e = await _db.Enrollments.FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        if (e.Status == nowy) return;
        if (!PipelineStateMachine.CzyMoznaPrzejsc(e.Status, nowy))
            throw new InvalidOperationException($"Niedozwolone przejście: {e.Status} → {nowy}.");

        var poprzedni = e.Status;
        e.Status = nowy;
        _db.PipelineHistory.Add(new PipelineHistory
        {
            EnrollmentId = e.Id, StatusZ = poprzedni, StatusNa = nowy,
            DataPrzejscia = _time.GetUtcNow(), Komentarz = komentarz
        });
        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Modyfikacja, nameof(ProgramEnrollment), e.Id.ToString(),
            e.PatientId, szczegolyJson: $"{{\"z\":\"{poprzedni}\",\"na\":\"{nowy}\"}}", ct: ct);
    }

    /// <summary>
    /// Ustawia status wyników (komplet/częściowe) przy imporcie. Dopuszcza wejście z etapów
    /// poprzedzających wyniki (gdy wyniki przyszły zanim oznaczono umówienie/realizację badań).
    /// </summary>
    public async Task OznaczWynikiAsync(Guid id, bool kompletne, string? komentarz = null, CancellationToken ct = default)
    {
        var e = await _db.Enrollments.FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        var docelowy = kompletne ? PipelineStatus.WynikiKomplet : PipelineStatus.WynikiCzesciowe;

        var dozwolone = e.Status is PipelineStatus.SkierowanieWystawione or PipelineStatus.BadaniaUmowione
            or PipelineStatus.BadaniaWToku or PipelineStatus.WynikiCzesciowe or PipelineStatus.WynikiKomplet;
        if (!dozwolone || e.Status == docelowy) return;

        var poprzedni = e.Status;
        e.Status = docelowy;
        _db.PipelineHistory.Add(new PipelineHistory
        {
            EnrollmentId = e.Id, StatusZ = poprzedni, StatusNa = docelowy,
            DataPrzejscia = _time.GetUtcNow(), Komentarz = komentarz ?? "Import wyników"
        });
        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Modyfikacja, nameof(ProgramEnrollment), e.Id.ToString(),
            e.PatientId, szczegolyJson: $"{{\"z\":\"{poprzedni}\",\"na\":\"{docelowy}\"}}", ct: ct);
    }
}
