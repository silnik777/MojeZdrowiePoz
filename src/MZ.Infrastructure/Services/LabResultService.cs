using Microsoft.EntityFrameworkCore;
using MZ.Application.Abstractions;
using MZ.Application.Services;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

/// <summary>Pozycja raportu braków badań (jeden przypadek z niewykonanymi badaniami).</summary>
public record BrakiPozycja(
    Guid EnrollmentId,
    string Pacjent,
    string? Laboratorium,
    PipelineStatus Status,
    IReadOnlyList<string> BrakujaceBadania);

/// <summary>Ręczne wprowadzanie wyników i raport kompletności (braki per laboratorium).</summary>
public class LabResultService
{
    private readonly MzDbContext _db;
    private readonly CompletenessService _completeness;
    private readonly LabResultImportService _import;
    private readonly IAuditService _audit;

    public LabResultService(MzDbContext db, CompletenessService completeness,
        LabResultImportService import, IAuditService audit)
    {
        _db = db;
        _completeness = completeness;
        _import = import;
        _audit = audit;
    }

    /// <summary>Dodaje/aktualizuje wynik wprowadzony ręcznie i przelicza kompletność.</summary>
    public async Task DodajRecznieAsync(Guid enrollmentId, Guid testDefinitionId,
        decimal? wartoscLiczbowa, string? wartoscTekstowa, DateOnly? dataWyniku, CancellationToken ct = default)
    {
        var e = await _db.Enrollments
            .Include(x => x.Skierowanie!).ThenInclude(s => s.Zlecone)
            .Include(x => x.Wyniki)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId, ct)
            ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        var def = await _db.TestDefinitions.FirstOrDefaultAsync(d => d.Id == testDefinitionId, ct)
                  ?? throw new InvalidOperationException("Nie znaleziono badania.");

        var wynik = e.Wyniki.FirstOrDefault(r => r.TestDefinitionId == testDefinitionId)
                    ?? new TestResult { EnrollmentId = e.Id, TestDefinitionId = testDefinitionId };

        wynik.WartoscLiczbowa = wartoscLiczbowa;
        wynik.WartoscTekstowa = wartoscTekstowa;
        wynik.Jednostka = def.Jednostka;
        wynik.DataWyniku = dataWyniku;
        wynik.Zrodlo = ZrodloWyniku.Reczny;
        wynik.LaboratoriumId = e.Skierowanie?.LaboratoriumId;
        wynik.OrderedTestId ??= e.Skierowanie?.Zlecone.FirstOrDefault(z => z.TestDefinitionId == testDefinitionId)?.Id;
        wynik.OcenaNormy = NormEvaluator.Ocen(wartoscLiczbowa, def.NormaMin, def.NormaMax);

        if (wynik.Id == default || !e.Wyniki.Contains(wynik)) { _db.TestResults.Add(wynik); e.Wyniki.Add(wynik); }

        await _db.SaveChangesAsync(ct);
        await _import.PrzeliczKompletnoscAsync(e, ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Utworzenie, nameof(TestResult), wynik.Id.ToString(), e.PatientId, ct: ct);
    }

    /// <summary>Raport przypadków z brakami badań (zlecone bez wyniku), z nazwami brakujących badań.</summary>
    public async Task<List<BrakiPozycja>> RaportBrakowAsync(Guid? laboratoriumId = null, CancellationToken ct = default)
    {
        var q = _db.Enrollments.AsNoTracking()
            .Include(e => e.Patient)
            .Include(e => e.Skierowanie!).ThenInclude(s => s.Zlecone).ThenInclude(z => z.TestDefinition)
            .Include(e => e.Skierowanie!).ThenInclude(s => s.Laboratorium)
            .Include(e => e.Wyniki)
            .Where(e => e.MaBrakiBadan && e.Skierowanie != null);

        if (laboratoriumId is not null)
            q = q.Where(e => e.Skierowanie!.LaboratoriumId == laboratoriumId);

        var lista = await q.ToListAsync(ct);

        return lista.Select(e =>
        {
            var wynik = _completeness.Sprawdz(e.Skierowanie!.Zlecone, e.Wyniki);
            var brakujace = e.Skierowanie!.Zlecone
                .Where(z => wynik.BrakujaceWymagane.Contains(z.TestDefinitionId))
                .Select(z => z.TestDefinition.Nazwa)
                .OrderBy(n => n)
                .ToList();
            return new BrakiPozycja(e.Id, $"{e.Patient.Nazwisko} {e.Patient.Imie}",
                e.Skierowanie!.Laboratorium?.Nazwa, e.Status, brakujace);
        })
        .Where(b => b.BrakujaceBadania.Count > 0)
        .OrderBy(b => b.Laboratorium).ThenBy(b => b.Pacjent)
        .ToList();
    }
}
