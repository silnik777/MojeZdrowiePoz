using MZ.Domain.Enums;

namespace MZ.Application.Dtos;

/// <summary>Pojedynczy wynik badania wczytany przez adapter laboratorium.</summary>
public record TestResultDto(
    string KodLab,
    decimal? WartoscLiczbowa,
    string? WartoscTekstowa,
    string? Jednostka,
    DateOnly? DataPobrania,
    DateOnly? DataWyniku,
    string? SurowyRekord);

public record SmsWynikDto(bool Sukces, string? IdentyfikatorBramki, string? Blad);

public record OcrWynikDto(string Tekst, double Pewnosc);

/// <summary>Wynik scoringu ankiety: suma punktów i wykryte czynniki ryzyka.</summary>
public record ScoringResult(int Score, IReadOnlyCollection<string> CzynnikiRyzyka);

/// <summary>
/// Wynik kontroli kompletności: braki wymagane/opcjonalne (zlecone bez wyniku),
/// wyniki nadmiarowe (bez zlecenia) oraz ogólny status.
/// </summary>
public record CompletenessResult(
    IReadOnlyCollection<Guid> BrakujaceWymagane,
    IReadOnlyCollection<Guid> BrakujaceOpcjonalne,
    IReadOnlyCollection<Guid> Nadmiarowe)
{
    public bool Kompletne => BrakujaceWymagane.Count == 0;

    public PipelineStatus SugerowanyStatus =>
        Kompletne ? PipelineStatus.WynikiKomplet : PipelineStatus.WynikiCzesciowe;
}
