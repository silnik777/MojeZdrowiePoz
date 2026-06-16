using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MZ.Application.Abstractions;
using MZ.Application.Dtos;
using MZ.Application.Services;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

/// <summary>Zapis i obsługa ankiety KBZOD: wpis ręczny, auto-score, wgranie skanu + OCR.</summary>
public class QuestionnaireService
{
    private readonly MzDbContext _db;
    private readonly ScoringService _scoring;
    private readonly IOcrService _ocr;
    private readonly IAuditService _audit;
    private readonly IConfiguration _config;

    public QuestionnaireService(MzDbContext db, ScoringService scoring, IOcrService ocr,
        IAuditService audit, IConfiguration config)
    {
        _db = db;
        _scoring = scoring;
        _ocr = ocr;
        _audit = audit;
        _config = config;
    }

    /// <summary>Zapisuje odpowiedzi ankiety, wylicza score wg aktywnego zestawu reguł i ustawia termin kontaktu (30 dni).</summary>
    public async Task<QuestionnaireSaveResult> ZapiszAsync(Guid enrollmentId, QuestionnaireInput input, CancellationToken ct = default)
    {
        var e = await _db.Enrollments
            .Include(x => x.Ankieta!).ThenInclude(a => a.Odpowiedzi)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId, ct)
            ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        var ankieta = e.Ankieta ?? new Questionnaire { EnrollmentId = e.Id };
        ankieta.Zrodlo = input.Zrodlo;
        ankieta.DataWypelnienia = input.DataWypelnienia;
        ankieta.DataWplywu = input.DataWplywu;
        ankieta.Notatki = input.Notatki;
        if (ankieta.StatusWeryfikacji == StatusWeryfikacjiOcr.NieDotyczy && input.Zrodlo != ZrodloAnkiety.Papierowa)
            ankieta.StatusWeryfikacji = StatusWeryfikacjiOcr.NieDotyczy;

        // Odśwież odpowiedzi.
        if (ankieta.Odpowiedzi.Count > 0)
            _db.QuestionnaireAnswers.RemoveRange(ankieta.Odpowiedzi);
        ankieta.Odpowiedzi = input.Odpowiedzi
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => new QuestionnaireAnswer { KodPytania = kv.Key, Wartosc = kv.Value })
            .ToList();

        var zestaw = await AktywnyZestawAsync(input.DataWplywu, ct);
        var wynik = zestaw is null
            ? new ScoringResult(0, Array.Empty<string>())
            : _scoring.Oblicz(ankieta, zestaw);

        ankieta.Score = wynik.Score;
        ankieta.ScoringRuleSetId = zestaw?.Id;

        if (e.Ankieta is null) _db.Questionnaires.Add(ankieta);
        e.TerminKontaktu = input.DataWplywu.AddDays(30);

        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Modyfikacja, nameof(Questionnaire), ankieta.Id.ToString(), e.PatientId, ct: ct);

        return new QuestionnaireSaveResult(wynik.Score, wynik.CzynnikiRyzyka);
    }

    /// <summary>Wgrywa skan PDF ankiety, uruchamia OCR i oznacza do ręcznej weryfikacji.</summary>
    public async Task WgrajSkanAsync(Guid enrollmentId, Stream pdf, string nazwaPliku, CancellationToken ct = default)
    {
        var e = await _db.Enrollments.Include(x => x.Ankieta)
            .FirstOrDefaultAsync(x => x.Id == enrollmentId, ct)
            ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        var katalog = _config.GetValue<string>("Storage:UploadsPath") ?? "uploads";
        Directory.CreateDirectory(katalog);
        var ext = Path.GetExtension(nazwaPliku);
        var sciezka = Path.Combine(katalog, $"{enrollmentId}_{Guid.NewGuid():N}{ext}");

        using var ms = new MemoryStream();
        await pdf.CopyToAsync(ms, ct);
        await File.WriteAllBytesAsync(sciezka, ms.ToArray(), ct);

        ms.Position = 0;
        var ocr = await _ocr.OdczytajAsync(ms, ct);

        var ankieta = e.Ankieta ?? new Questionnaire { EnrollmentId = e.Id, DataWplywu = DateOnly.FromDateTime(DateTime.Today) };
        ankieta.Zrodlo = ZrodloAnkiety.Papierowa;
        ankieta.SkanPdfSciezka = sciezka;
        ankieta.OcrTekst = ocr.Tekst;
        ankieta.OcrPewnosc = ocr.Pewnosc;
        ankieta.StatusWeryfikacji = StatusWeryfikacjiOcr.OczekujeNaWeryfikacje;

        if (e.Ankieta is null) _db.Questionnaires.Add(ankieta);
        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Modyfikacja, nameof(Questionnaire), ankieta.Id.ToString(), e.PatientId, ct: ct);
    }

    private Task<ScoringRuleSet?> AktywnyZestawAsync(DateOnly naDzien, CancellationToken ct) =>
        _db.ScoringRuleSets
            .Include(z => z.Reguly)
            .Where(z => z.Aktywny && z.ObowiazujeOd <= naDzien && (z.ObowiazujeDo == null || z.ObowiazujeDo >= naDzien))
            .OrderByDescending(z => z.ObowiazujeOd)
            .FirstOrDefaultAsync(ct);
}
