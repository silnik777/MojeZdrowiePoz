using Microsoft.EntityFrameworkCore;
using MZ.Application.Abstractions;
using MZ.Application.Dtos;
using MZ.Application.Services;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

/// <summary>Kwalifikacja do programu oraz wystawienie skierowania z doborem pakietu badań.</summary>
public class QualificationService
{
    private readonly MzDbContext _db;
    private readonly ScoringService _scoring;
    private readonly PackageSelectionService _packages;
    private readonly EnrollmentService _enrollments;
    private readonly IAuditService _audit;
    private readonly TimeProvider _time;

    public QualificationService(MzDbContext db, ScoringService scoring, PackageSelectionService packages,
        EnrollmentService enrollments, IAuditService audit, TimeProvider time)
    {
        _db = db;
        _scoring = scoring;
        _packages = packages;
        _enrollments = enrollments;
        _audit = audit;
        _time = time;
    }

    private DateOnly Dzis => DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

    /// <summary>Zapisuje decyzję kwalifikacyjną; czynniki ryzyka wyliczane z ankiety. Zmienia status przypadku.</summary>
    public async Task KwalifikujAsync(Guid enrollmentId, DecyzjaKwalifikacji decyzja, string? uzasadnienie, CancellationToken ct = default)
    {
        var e = await _db.Enrollments
            .Include(x => x.Ankieta!).ThenInclude(a => a.Odpowiedzi)
            .Include(x => x.Kwalifikacja)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId, ct)
            ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        var czynniki = await WyliczCzynnikiRyzykaAsync(e, ct);

        var kw = e.Kwalifikacja ?? new Qualification { EnrollmentId = e.Id };
        kw.Decyzja = decyzja;
        kw.DataDecyzji = Dzis;
        kw.Uzasadnienie = uzasadnienie;
        kw.CzynnikiRyzyka = czynniki.ToList();
        if (e.Kwalifikacja is null) _db.Qualifications.Add(kw);

        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Utworzenie, nameof(Qualification), kw.Id.ToString(), e.PatientId, ct: ct);

        var nowy = decyzja == DecyzjaKwalifikacji.Zakwalifikowany
            ? PipelineStatus.ZakwalifikowanyDoProgramu
            : PipelineStatus.Wykluczony;
        await _enrollments.ZmienStatusAsync(e.Id, nowy, "Kwalifikacja", ct);
    }

    /// <summary>Dobiera pakiety wg wieku/płci/ryzyka i tworzy skierowanie z migawką zleconych badań.</summary>
    public async Task<Referral> WystawSkierowanieAsync(Guid enrollmentId, Guid? laboratoriumId, CancellationToken ct = default)
    {
        var e = await _db.Enrollments
            .Include(x => x.Patient)
            .Include(x => x.Kwalifikacja)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId, ct)
            ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        if (e.Skierowanie is not null || await _db.Referrals.AnyAsync(r => r.EnrollmentId == e.Id, ct))
            throw new InvalidOperationException("Skierowanie już istnieje.");

        var pakiety = await _db.TestPackages
            .Include(p => p.Pozycje).ThenInclude(pi => pi.TestDefinition)
            .Where(p => p.Aktywny).ToListAsync(ct);

        var czynniki = e.Kwalifikacja?.CzynnikiRyzyka ?? new List<string>();
        var wiek = e.Patient.Wiek(Dzis);
        var dobrane = _packages.DobierzPakiety(pakiety, wiek, e.Patient.Plec, czynniki, Dzis);
        var badania = _packages.Badania(dobrane);

        if (badania.Count == 0)
            throw new InvalidOperationException("Brak pasujących pakietów/badań dla pacjenta.");

        var skierowanie = new Referral
        {
            EnrollmentId = e.Id,
            DataWystawienia = Dzis,
            LaboratoriumId = laboratoriumId,
            Zlecone = badania.Select(b => new OrderedTest
            {
                TestDefinitionId = b.Id, Wymagane = true, DataZlecenia = Dzis
            }).ToList()
        };
        e.PrzypisanyPakietId = dobrane.FirstOrDefault(p => p.Typ == TypPakietu.Podstawowy)?.Id ?? dobrane.FirstOrDefault()?.Id;
        _db.Referrals.Add(skierowanie);

        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Utworzenie, nameof(Referral), skierowanie.Id.ToString(), e.PatientId, ct: ct);

        await _enrollments.ZmienStatusAsync(e.Id, PipelineStatus.SkierowanieWystawione, "Wystawienie skierowania", ct);
        return skierowanie;
    }

    private async Task<IReadOnlyCollection<string>> WyliczCzynnikiRyzykaAsync(ProgramEnrollment e, CancellationToken ct)
    {
        if (e.Ankieta is null) return Array.Empty<string>();
        var zestaw = await _db.ScoringRuleSets.Include(z => z.Reguly)
            .FirstOrDefaultAsync(z => z.Id == e.Ankieta.ScoringRuleSetId, ct);
        if (zestaw is null) return Array.Empty<string>();
        return _scoring.Oblicz(e.Ankieta, zestaw).CzynnikiRyzyka;
    }
}
