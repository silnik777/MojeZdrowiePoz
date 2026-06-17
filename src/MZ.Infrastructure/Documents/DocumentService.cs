using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MZ.Application.Abstractions;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MZ.Infrastructure.Documents;

/// <summary>
/// Generuje dokumenty PDF (IPZ, skierowanie) i obsługuje wgranie wersji podpisanej zewnętrznie
/// (Profil Zaufany / podpis kwalifikowany / certyfikat ZUS). Rejestruje je jako SignedDocument.
/// </summary>
public class DocumentService
{
    static DocumentService() => QuestPDF.Settings.License = LicenseType.Community;

    private readonly MzDbContext _db;
    private readonly IAuditService _audit;
    private readonly TimeProvider _time;
    private readonly string _folder;

    public DocumentService(MzDbContext db, IAuditService audit, TimeProvider time, IConfiguration config)
    {
        _db = db;
        _audit = audit;
        _time = time;
        _folder = config.GetValue<string>("Storage:DocumentsPath") ?? "dokumenty";
    }

    public async Task<SignedDocument> GenerujIpzAsync(Guid enrollmentId, CancellationToken ct = default)
    {
        var e = await Wczytaj(enrollmentId, ct);
        if (e.Wizyta?.Ipz is null) throw new InvalidOperationException("Brak IPZ do wygenerowania.");
        var pdf = BudujIpz(e);
        return await ZapiszDokumentAsync(e, TypDokumentu.IndywidualnyPlanZdrowotny, pdf, ct);
    }

    public async Task<SignedDocument> GenerujSkierowanieAsync(Guid enrollmentId, CancellationToken ct = default)
    {
        var e = await Wczytaj(enrollmentId, ct);
        if (e.Skierowanie is null) throw new InvalidOperationException("Brak skierowania do wygenerowania.");
        var pdf = BudujSkierowanie(e);
        return await ZapiszDokumentAsync(e, TypDokumentu.SkierowanieNaBadania, pdf, ct);
    }

    /// <summary>Zapisuje wgrany, podpisany zewnętrznie plik PDF i oznacza dokument jako podpisany.</summary>
    public async Task WgrajPodpisanyAsync(Guid documentId, Stream plik, TypPodpisu typPodpisu, CancellationToken ct = default)
    {
        var dok = await _db.SignedDocuments.Include(d => d.Enrollment)
                      .FirstOrDefaultAsync(d => d.Id == documentId, ct)
                  ?? throw new InvalidOperationException("Nie znaleziono dokumentu.");

        Directory.CreateDirectory(_folder);
        var sciezka = Path.Combine(_folder, $"{dok.Id}_signed.pdf");
        await using (var fs = File.Create(sciezka))
            await plik.CopyToAsync(fs, ct);

        dok.PlikSciezka = sciezka;
        dok.TypPodpisu = typPodpisu;
        dok.DataPodpisu = _time.GetUtcNow();
        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Modyfikacja, nameof(SignedDocument), dok.Id.ToString(), dok.Enrollment.PatientId, ct: ct);
    }

    public async Task<List<SignedDocument>> ListaAsync(Guid enrollmentId, CancellationToken ct = default) =>
        // Sortowanie po stronie pamięci — SQLite nie obsługuje ORDER BY po DateTimeOffset.
        (await _db.SignedDocuments.AsNoTracking().Where(d => d.EnrollmentId == enrollmentId).ToListAsync(ct))
            .OrderByDescending(d => d.UtworzonoUtc).ToList();

    public Task<SignedDocument?> PobierzAsync(Guid id, CancellationToken ct = default) =>
        _db.SignedDocuments.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);

    private async Task<ProgramEnrollment> Wczytaj(Guid id, CancellationToken ct) =>
        await _db.Enrollments
            .Include(e => e.Patient)
            .Include(e => e.Skierowanie!).ThenInclude(s => s.Zlecone).ThenInclude(z => z.TestDefinition)
            .Include(e => e.Skierowanie!).ThenInclude(s => s.Laboratorium)
            .Include(e => e.Wizyta!).ThenInclude(v => v.Ipz)
            .FirstOrDefaultAsync(e => e.Id == id, ct)
        ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

    private async Task<SignedDocument> ZapiszDokumentAsync(ProgramEnrollment e, TypDokumentu typ, byte[] pdf, CancellationToken ct)
    {
        var dok = new SignedDocument { EnrollmentId = e.Id, Typ = typ, TypPodpisu = TypPodpisu.Brak };
        Directory.CreateDirectory(_folder);
        var sciezka = Path.Combine(_folder, $"{dok.Id}.pdf");
        await File.WriteAllBytesAsync(sciezka, pdf, ct);
        dok.PlikSciezka = sciezka;
        _db.SignedDocuments.Add(dok);
        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Utworzenie, nameof(SignedDocument), dok.Id.ToString(), e.PatientId, ct: ct);
        return dok;
    }

    private static byte[] BudujIpz(ProgramEnrollment e)
    {
        var v = e.Wizyta!;
        var ipz = v.Ipz!;
        return Document.Create(doc =>
        {
            doc.Page(p =>
            {
                p.Margin(40);
                p.Size(PageSizes.A4);
                p.DefaultTextStyle(t => t.FontSize(11));
                p.Header().Text("Indywidualny Plan Zdrowotny (IPZ)").FontSize(16).Bold();
                p.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Text($"Pacjent: {e.Patient.Nazwisko} {e.Patient.Imie}, PESEL {e.Patient.Pesel}");
                    col.Item().Text($"Data wizyty: {v.DataWizyty:yyyy-MM-dd}");
                    col.Item().Text($"Pomiary: ciśnienie {v.CisnienieSkurczowe}/{v.CisnienieRozkurczowe} mmHg, " +
                                    $"tętno {v.Tetno}, BMI {v.Bmi?.ToString() ?? "—"}, obwód talii {v.ObwodTaliiCm} cm");
                    col.Item().PaddingTop(6).Text("Cele:").Bold();
                    foreach (var c in ipz.Cele) col.Item().Text($"• {c}");
                    col.Item().PaddingTop(6).Text("Interwencje:").Bold();
                    foreach (var i in ipz.Interwencje) col.Item().Text($"• {i}");
                    if (!string.IsNullOrWhiteSpace(ipz.ZaleceniaSzczepien))
                        col.Item().PaddingTop(6).Text($"Zalecenia szczepień: {ipz.ZaleceniaSzczepien}");
                    if (!string.IsNullOrWhiteSpace(ipz.DalszaDiagnostyka))
                        col.Item().Text($"Dalsza diagnostyka: {ipz.DalszaDiagnostyka}");
                    if (!string.IsNullOrWhiteSpace(v.Zalecenia))
                        col.Item().PaddingTop(6).Text($"Zalecenia ogólne: {v.Zalecenia}");
                });
                p.Footer().AlignRight().Text(t => t.Span($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8));
            });
        }).GeneratePdf();
    }

    private static byte[] BudujSkierowanie(ProgramEnrollment e)
    {
        var s = e.Skierowanie!;
        return Document.Create(doc =>
        {
            doc.Page(p =>
            {
                p.Margin(40);
                p.Size(PageSizes.A4);
                p.DefaultTextStyle(t => t.FontSize(11));
                p.Header().Text("Skierowanie na badania — program „Moje Zdrowie\"").FontSize(15).Bold();
                p.Content().PaddingVertical(10).Column(col =>
                {
                    col.Spacing(6);
                    col.Item().Text($"Pacjent: {e.Patient.Nazwisko} {e.Patient.Imie}, PESEL {e.Patient.Pesel}");
                    col.Item().Text($"Data wystawienia: {s.DataWystawienia:yyyy-MM-dd}");
                    if (s.Laboratorium is not null) col.Item().Text($"Laboratorium: {s.Laboratorium.Nazwa}");
                    col.Item().PaddingTop(6).Text("Zlecone badania:").Bold();
                    foreach (var z in s.Zlecone.OrderBy(z => z.TestDefinition.Nazwa))
                        col.Item().Text($"• {z.TestDefinition.Nazwa} ({z.TestDefinition.Kod})");
                });
                p.Footer().AlignRight().Text(t => t.Span($"Wygenerowano: {DateTime.Now:yyyy-MM-dd HH:mm}").FontSize(8));
            });
        }).GeneratePdf();
    }
}
