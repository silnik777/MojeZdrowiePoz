using MZ.Application.Abstractions;
using MZ.Application.Dtos;

namespace MZ.Infrastructure.Ocr;

/// <summary>
/// Tymczasowy adapter OCR (zaślepka). Zapisuje fakt wgrania skanu i oznacza ankietę do
/// ręcznej weryfikacji. Docelowo zostanie zastąpiony silnikiem OCR/OMR (np. Tesseract z
/// polskim modelem + szablon KBZOD) przetwarzającym dane lokalnie (RODO).
/// </summary>
public class StubOcrService : IOcrService
{
    public Task<OcrWynikDto> OdczytajAsync(Stream pdf, CancellationToken ct = default)
    {
        // Pewność 0 => zawsze wymagana weryfikacja człowieka.
        return Task.FromResult(new OcrWynikDto(
            Tekst: "[OCR niezaimplementowany] Wgrano skan ankiety — wymaga ręcznego wprowadzenia/weryfikacji odpowiedzi.",
            Pewnosc: 0.0));
    }
}
