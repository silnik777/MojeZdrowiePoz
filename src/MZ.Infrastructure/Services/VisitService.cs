using Microsoft.EntityFrameworkCore;
using MZ.Application.Abstractions;
using MZ.Application.Dtos;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

/// <summary>Wizyta podsumowująca, Indywidualny Plan Zdrowotny i przekazanie do rozliczenia.</summary>
public class VisitService
{
    private readonly MzDbContext _db;
    private readonly EnrollmentService _enrollments;
    private readonly IAuditService _audit;

    public VisitService(MzDbContext db, EnrollmentService enrollments, IAuditService audit)
    {
        _db = db;
        _enrollments = enrollments;
        _audit = audit;
    }

    /// <summary>Zapisuje wizytę (pomiary) i przesuwa przypadek do „Wizyta zrealizowana".</summary>
    public async Task<Visit> ZapiszWizyteAsync(Guid enrollmentId, VisitInput input, CancellationToken ct = default)
    {
        var e = await _db.Enrollments.Include(x => x.Wizyta)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId, ct)
            ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        var wizyta = e.Wizyta ?? new Visit { EnrollmentId = e.Id };
        wizyta.DataWizyty = input.DataWizyty;
        wizyta.CisnienieSkurczowe = input.CisnienieSkurczowe;
        wizyta.CisnienieRozkurczowe = input.CisnienieRozkurczowe;
        wizyta.Tetno = input.Tetno;
        wizyta.WzrostCm = input.WzrostCm;
        wizyta.WagaKg = input.WagaKg;
        wizyta.ObwodTaliiCm = input.ObwodTaliiCm;
        wizyta.PodsumowanieWynikow = input.PodsumowanieWynikow;
        wizyta.Zalecenia = input.Zalecenia;
        if (e.Wizyta is null) _db.Visits.Add(wizyta);

        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Modyfikacja, nameof(Visit), wizyta.Id.ToString(), e.PatientId, ct: ct);

        // Doprowadź status do „Wizyta zrealizowana" (przez umówienie, jeśli trzeba).
        if (e.Status is PipelineStatus.WynikiKomplet or PipelineStatus.WynikiCzesciowe)
            await _enrollments.ZmienStatusAsync(e.Id, PipelineStatus.WizytaPodsumowujacaUmowiona, "Wizyta podsumowująca", ct);
        await _enrollments.ZmienStatusAsync(e.Id, PipelineStatus.WizytaZrealizowana, "Realizacja wizyty", ct);

        return wizyta;
    }

    /// <summary>Tworzy/aktualizuje Indywidualny Plan Zdrowotny powiązany z wizytą.</summary>
    public async Task ZapiszIpzAsync(Guid enrollmentId, IpzInput input, CancellationToken ct = default)
    {
        var e = await _db.Enrollments
            .Include(x => x.Wizyta!).ThenInclude(v => v.Ipz)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId, ct)
            ?? throw new InvalidOperationException("Nie znaleziono przypadku.");
        if (e.Wizyta is null) throw new InvalidOperationException("Najpierw zapisz wizytę.");

        var ipz = e.Wizyta.Ipz ?? new IndividualHealthPlan
        {
            VisitId = e.Wizyta.Id, EnrollmentId = e.Id, DataUtworzenia = e.Wizyta.DataWizyty
        };
        ipz.Cele = input.Cele.ToList();
        ipz.Interwencje = input.Interwencje.ToList();
        ipz.ZaleceniaSzczepien = input.ZaleceniaSzczepien;
        ipz.DalszaDiagnostyka = input.DalszaDiagnostyka;
        ipz.DataPrzegladu = input.DataPrzegladu;
        if (e.Wizyta.Ipz is null) _db.HealthPlans.Add(ipz);

        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Modyfikacja, nameof(IndividualHealthPlan), ipz.Id.ToString(), e.PatientId, ct: ct);
    }

    /// <summary>Przekazuje zrealizowany przypadek do rozliczenia.</summary>
    public async Task PrzekazDoRozliczeniaAsync(Guid enrollmentId, CancellationToken ct = default) =>
        await _enrollments.ZmienStatusAsync(enrollmentId, PipelineStatus.DoRozliczenia, "Przekazanie do rozliczenia", ct);
}
