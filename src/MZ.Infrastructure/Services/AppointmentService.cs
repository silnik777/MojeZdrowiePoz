using Microsoft.EntityFrameworkCore;
using MZ.Application.Abstractions;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

/// <summary>Terminarz badań i wizyt: umawianie z automatycznym przypomnieniem i postępem pipeline.</summary>
public class AppointmentService
{
    private readonly MzDbContext _db;
    private readonly EnrollmentService _enrollments;
    private readonly IAuditService _audit;
    private readonly TimeProvider _time;

    public AppointmentService(MzDbContext db, EnrollmentService enrollments, IAuditService audit, TimeProvider time)
    {
        _db = db;
        _enrollments = enrollments;
        _audit = audit;
        _time = time;
    }

    /// <summary>Umawia termin; tworzy przypomnienie (domyślnie 24h przed) i przesuwa pipeline dla pobrania badań.</summary>
    public async Task<Appointment> UmowAsync(Guid enrollmentId, TypTerminu typ, DateTimeOffset dataGodzina,
        Guid? laboratoriumId = null, int godzinPrzed = 24, CancellationToken ct = default)
    {
        var e = await _db.Enrollments.FirstOrDefaultAsync(x => x.Id == enrollmentId, ct)
                ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        var termin = new Appointment
        {
            EnrollmentId = e.Id, Typ = typ, DataGodzina = dataGodzina,
            Status = StatusTerminu.Zaplanowany, LaboratoriumId = laboratoriumId
        };
        _db.Appointments.Add(termin);

        var dataPrzypomnienia = dataGodzina.AddHours(-Math.Abs(godzinPrzed));
        _db.Reminders.Add(new Reminder
        {
            Appointment = termin, EnrollmentId = e.Id,
            Typ = TypPowiadomienia.PrzypomnienieOTerminie,
            DataZaplanowana = dataPrzypomnienia
        });

        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Utworzenie, nameof(Appointment), termin.Id.ToString(), e.PatientId, ct: ct);

        // Umówienie pobrania badań przesuwa przypadek do "Badania umówione".
        if (typ == TypTerminu.PobranieBadan && e.Status == PipelineStatus.SkierowanieWystawione)
            await _enrollments.ZmienStatusAsync(e.Id, PipelineStatus.BadaniaUmowione, "Umówienie pobrania badań", ct);
        else if (typ == TypTerminu.WizytaPodsumowujaca && e.Status is PipelineStatus.WynikiKomplet or PipelineStatus.WynikiCzesciowe)
            await _enrollments.ZmienStatusAsync(e.Id, PipelineStatus.WizytaPodsumowujacaUmowiona, "Umówienie wizyty podsumowującej", ct);

        return termin;
    }

    public async Task<List<Appointment>> NadchodzaceAsync(int dniDoPrzodu = 30, CancellationToken ct = default)
    {
        var teraz = _time.GetUtcNow();
        var granica = teraz.AddDays(dniDoPrzodu);

        // Filtr po dacie w pamięci — porównania DateTimeOffset nie są tłumaczone przez SQLite (dev).
        var zaplanowane = await _db.Appointments.AsNoTracking()
            .Include(a => a.Enrollment).ThenInclude(e => e.Patient)
            .Include(a => a.Laboratorium)
            .Where(a => a.Status == StatusTerminu.Zaplanowany)
            .ToListAsync(ct);

        return zaplanowane
            .Where(a => a.DataGodzina >= teraz && a.DataGodzina <= granica)
            .OrderBy(a => a.DataGodzina)
            .ToList();
    }

    public async Task ZmienStatusAsync(Guid appointmentId, StatusTerminu status, CancellationToken ct = default)
    {
        var a = await _db.Appointments.FirstOrDefaultAsync(x => x.Id == appointmentId, ct)
                ?? throw new InvalidOperationException("Nie znaleziono terminu.");
        a.Status = status;
        await _db.SaveChangesAsync(ct);
    }
}
