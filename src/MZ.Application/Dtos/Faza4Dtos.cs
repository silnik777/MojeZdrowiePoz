namespace MZ.Application.Dtos;

/// <summary>Dane wejściowe wizyty podsumowującej (pomiary i podsumowanie).</summary>
public record VisitInput(
    DateOnly DataWizyty,
    int? CisnienieSkurczowe,
    int? CisnienieRozkurczowe,
    int? Tetno,
    double? WzrostCm,
    double? WagaKg,
    double? ObwodTaliiCm,
    string? PodsumowanieWynikow,
    string? Zalecenia);

/// <summary>Dane wejściowe Indywidualnego Planu Zdrowotnego.</summary>
public record IpzInput(
    IReadOnlyList<string> Cele,
    IReadOnlyList<string> Interwencje,
    string? ZaleceniaSzczepien,
    string? DalszaDiagnostyka,
    DateOnly? DataPrzegladu);
