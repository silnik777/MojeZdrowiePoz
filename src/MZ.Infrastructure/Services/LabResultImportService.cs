using Microsoft.EntityFrameworkCore;
using MZ.Application.Abstractions;
using MZ.Application.Dtos;
using MZ.Application.Services;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

/// <summary>Importuje wyniki z laboratorium, dopasowuje do przypadków i przelicza kompletność badań.</summary>
public class LabResultImportService
{
    private static readonly PipelineStatus[] StatusyOczekujaceNaWyniki =
    {
        PipelineStatus.SkierowanieWystawione, PipelineStatus.BadaniaUmowione,
        PipelineStatus.BadaniaWToku, PipelineStatus.WynikiCzesciowe, PipelineStatus.WynikiKomplet
    };

    private readonly MzDbContext _db;
    private readonly ILabResultSource _zrodlo;
    private readonly CompletenessService _completeness;
    private readonly EnrollmentService _enrollments;
    private readonly IAuditService _audit;

    public LabResultImportService(MzDbContext db, ILabResultSource zrodlo, CompletenessService completeness,
        EnrollmentService enrollments, IAuditService audit)
    {
        _db = db;
        _zrodlo = zrodlo;
        _completeness = completeness;
        _enrollments = enrollments;
        _audit = audit;
    }

    public async Task<LabImportSummary> ImportujAsync(Stream plik, Guid laboratoriumId, CancellationToken ct = default)
    {
        var dokumenty = await _zrodlo.WczytajAsync(plik, ct);
        var ostrzezenia = new List<string>();
        int dopasowane = 0, zapisane = 0, niedopasowane = 0;

        var mapowania = await _db.LabTestMappings
            .Where(m => m.LaboratoriumId == laboratoriumId)
            .ToDictionaryAsync(m => m.KodLab, m => m.TestDefinitionId, StringComparer.OrdinalIgnoreCase, ct);
        var poKodzie = await _db.TestDefinitions
            .ToDictionaryAsync(d => d.Kod, d => d, StringComparer.OrdinalIgnoreCase, ct);
        var poId = poKodzie.Values.ToDictionary(d => d.Id);

        foreach (var dok in dokumenty)
        {
            if (string.IsNullOrWhiteSpace(dok.Pesel))
            {
                niedopasowane++;
                ostrzezenia.Add("Dokument bez PESEL — pominięto.");
                continue;
            }

            var e = await _db.Enrollments
                .Include(x => x.Skierowanie!).ThenInclude(s => s.Zlecone)
                .Include(x => x.Wyniki)
                .Where(x => x.Patient.Pesel == dok.Pesel && x.Skierowanie != null
                            && StatusyOczekujaceNaWyniki.Contains(x.Status))
                .OrderByDescending(x => x.Skierowanie!.DataWystawienia)
                .FirstOrDefaultAsync(ct);

            if (e is null)
            {
                niedopasowane++;
                ostrzezenia.Add($"Brak pasującego przypadku dla PESEL {dok.Pesel}.");
                continue;
            }

            dopasowane++;
            foreach (var w in dok.Wyniki)
            {
                Guid defId;
                if (mapowania.TryGetValue(w.KodLab, out var zMap)) defId = zMap;
                else if (poKodzie.TryGetValue(w.KodLab, out var def)) defId = def.Id;
                else
                {
                    ostrzezenia.Add($"Nieznany kod badania '{w.KodLab}' (PESEL {dok.Pesel}) — pominięto.");
                    continue;
                }

                var istniejacy = e.Wyniki.FirstOrDefault(r => r.TestDefinitionId == defId);
                var wynik = istniejacy ?? new TestResult { EnrollmentId = e.Id, TestDefinitionId = defId };

                wynik.WartoscLiczbowa = w.WartoscLiczbowa;
                wynik.WartoscTekstowa = w.WartoscTekstowa;
                wynik.Jednostka = w.Jednostka ?? (poId.TryGetValue(defId, out var d1) ? d1.Jednostka : null);
                wynik.DataWyniku = w.DataWyniku ?? wynik.DataWyniku;
                wynik.DataPobrania = w.DataPobrania ?? wynik.DataPobrania;
                wynik.Zrodlo = ZrodloWyniku.Hl7;
                wynik.LaboratoriumId = laboratoriumId;
                wynik.SurowyRekord = w.SurowyRekord;
                wynik.OrderedTestId ??= e.Skierowanie!.Zlecone.FirstOrDefault(z => z.TestDefinitionId == defId)?.Id;

                if (poId.TryGetValue(defId, out var def2))
                    wynik.OcenaNormy = NormEvaluator.Ocen(w.WartoscLiczbowa, def2.NormaMin, def2.NormaMax);

                if (istniejacy is null) { _db.TestResults.Add(wynik); e.Wyniki.Add(wynik); }
                zapisane++;
            }

            await _db.SaveChangesAsync(ct);
            await PrzeliczKompletnoscAsync(e, ct);
            await _audit.ZapiszAsync(AkcjaAudytu.Modyfikacja, nameof(TestResult), e.Id.ToString(), e.PatientId, ct: ct);
        }

        return new LabImportSummary(dokumenty.Count, dopasowane, zapisane, niedopasowane, ostrzezenia);
    }

    /// <summary>Przelicza kompletność badań przypadku, aktualizuje flagę braków i status pipeline.</summary>
    public async Task PrzeliczKompletnoscAsync(ProgramEnrollment e, CancellationToken ct = default)
    {
        var zlecone = e.Skierowanie?.Zlecone ?? (ICollection<OrderedTest>)Array.Empty<OrderedTest>();
        var wynik = _completeness.Sprawdz(zlecone, e.Wyniki);

        e.MaBrakiBadan = !wynik.Kompletne && zlecone.Count > 0;
        await _db.SaveChangesAsync(ct);

        if (zlecone.Count > 0)
            await _enrollments.OznaczWynikiAsync(e.Id, wynik.Kompletne, ct: ct);
    }
}
