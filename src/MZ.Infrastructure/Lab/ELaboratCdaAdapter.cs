using System.Globalization;
using System.Xml.Linq;
using MZ.Application.Abstractions;
using MZ.Application.Dtos;
using MZ.Domain.Enums;

namespace MZ.Infrastructure.Lab;

/// <summary>
/// Adapter wyników z eLaborat (Marcel) w formacie HL7 CDA (XML). Parser jest tolerancyjny na
/// przestrzenie nazw i wyłuskuje obserwacje (kod badania, wartość, jednostkę, datę) oraz PESEL
/// pacjenta. Best-effort — wymaga dostrojenia na realnych próbkach z laboratorium kl888.
/// </summary>
public class ELaboratCdaAdapter : ILabResultSource
{
    public TypIntegracjiLaboratorium Typ => TypIntegracjiLaboratorium.Hl7;

    public async Task<IReadOnlyList<LabResultDocumentDto>> WczytajAsync(Stream zrodlo, CancellationToken ct = default)
    {
        var doc = await XDocument.LoadAsync(zrodlo, LoadOptions.None, ct);
        var dokumenty = new List<LabResultDocumentDto>();

        // Obsłuż zarówno pojedynczy ClinicalDocument, jak i kopertę z wieloma dokumentami.
        var clinicalDocs = Potomkowie(doc.Root, "ClinicalDocument").ToList();
        if (clinicalDocs.Count == 0 && doc.Root is not null) clinicalDocs.Add(doc.Root);

        foreach (var cd in clinicalDocs)
        {
            var pesel = ZnajdzPesel(cd);
            var idZlecenia = Atrybut(Dziecko(cd, "id"), "extension");
            var wyniki = Potomkowie(cd, "observation")
                .Select(Mapuj)
                .Where(w => w is not null)
                .Cast<TestResultDto>()
                .ToList();

            if (wyniki.Count == 0) continue;
            var data = wyniki.Select(w => w.DataWyniku).FirstOrDefault(d => d is not null);
            dokumenty.Add(new LabResultDocumentDto(pesel, idZlecenia, data, wyniki));
        }

        return dokumenty;
    }

    private static TestResultDto? Mapuj(XElement obs)
    {
        var code = Dziecko(obs, "code");
        var kod = Atrybut(code, "code");
        if (string.IsNullOrWhiteSpace(kod)) return null;

        var value = Dziecko(obs, "value");
        decimal? liczba = null;
        string? tekst = null;
        string? jednostka = null;

        if (value is not null)
        {
            var typ = TypXsi(value);
            if (typ is "PQ" or "INT" or "REAL")
            {
                jednostka = Atrybut(value, "unit");
                var v = Atrybut(value, "value");
                if (decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) liczba = d;
            }
            else
            {
                tekst = string.IsNullOrWhiteSpace(value.Value) ? Atrybut(value, "value") : value.Value.Trim();
            }
        }

        var data = ParsujTs(Atrybut(Dziecko(obs, "effectiveTime"), "value"));
        return new TestResultDto(kod, liczba, tekst, jednostka, null, data, obs.ToString());
    }

    private static string? ZnajdzPesel(XElement cd)
    {
        // PESEL to 11-cyfrowy identyfikator pacjenta w sekcji recordTarget/patientRole.
        var patientRole = Potomkowie(cd, "patientRole").FirstOrDefault();
        var zakres = patientRole ?? cd;
        foreach (var id in Potomkowie(zakres, "id"))
        {
            var ext = Atrybut(id, "extension");
            if (!string.IsNullOrWhiteSpace(ext) && ext.Length == 11 && ext.All(char.IsDigit))
                return ext;
        }
        return null;
    }

    private static DateOnly? ParsujTs(string? ts)
    {
        if (string.IsNullOrWhiteSpace(ts) || ts.Length < 8) return null;
        return DateOnly.TryParseExact(ts[..8], "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
            ? d : null;
    }

    // Pomocnicze: dopasowanie po nazwie lokalnej (ignorując przestrzeń nazw CDA).
    private static IEnumerable<XElement> Potomkowie(XElement? el, string localName) =>
        el is null ? Enumerable.Empty<XElement>()
            : el.Descendants().Where(e => e.Name.LocalName == localName);

    private static XElement? Dziecko(XElement? el, string localName) =>
        el?.Elements().FirstOrDefault(e => e.Name.LocalName == localName);

    private static string? Atrybut(XElement? el, string localName) =>
        el?.Attributes().FirstOrDefault(a => a.Name.LocalName == localName)?.Value;

    private static string? TypXsi(XElement el) =>
        el.Attributes().FirstOrDefault(a => a.Name.LocalName == "type")?.Value?.Split(':').Last();
}
