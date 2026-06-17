using Microsoft.EntityFrameworkCore;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

/// <summary>Pozycja listy „do kontaktu" (kontakt w terminie 30 dni od wpływu ankiety).</summary>
public record DoKontaktuPozycja(
    Guid EnrollmentId,
    string Pacjent,
    string? Telefon,
    bool ZgodaSms,
    PipelineStatus Status,
    DateOnly? TerminKontaktu,
    int? DniDoTerminu);

/// <summary>Listy robocze powiadomień (kontakt 30 dni).</summary>
public class NotificationService
{
    private static readonly PipelineStatus[] PrzedUmowieniem =
    {
        PipelineStatus.ZakwalifikowanyDoProgramu, PipelineStatus.SkierowanieWystawione
    };

    private readonly MzDbContext _db;
    private readonly TimeProvider _time;

    public NotificationService(MzDbContext db, TimeProvider time)
    {
        _db = db;
        _time = time;
    }

    /// <summary>Przypadki wymagające kontaktu z pacjentem (umówienia badań) z terminem 30-dniowym.</summary>
    public async Task<List<DoKontaktuPozycja>> DoKontaktuAsync(CancellationToken ct = default)
    {
        var dzis = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);
        var lista = await _db.Enrollments.AsNoTracking()
            .Include(e => e.Patient)
            .Where(e => e.TerminKontaktu != null && PrzedUmowieniem.Contains(e.Status))
            .OrderBy(e => e.TerminKontaktu)
            .ToListAsync(ct);

        return lista.Select(e => new DoKontaktuPozycja(
            e.Id, $"{e.Patient.Nazwisko} {e.Patient.Imie}", e.Patient.Telefon, e.Patient.ZgodaSms,
            e.Status, e.TerminKontaktu,
            e.TerminKontaktu is null ? null : e.TerminKontaktu.Value.DayNumber - dzis.DayNumber))
            .ToList();
    }
}
