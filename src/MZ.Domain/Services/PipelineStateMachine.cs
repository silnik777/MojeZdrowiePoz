using MZ.Domain.Enums;

namespace MZ.Domain.Services;

/// <summary>
/// Reguły dozwolonych przejść statusów pipeline programu „Moje Zdrowie".
/// Trzyma logikę w jednym miejscu, niezależnie od warstwy aplikacji.
/// </summary>
public static class PipelineStateMachine
{
    private static readonly IReadOnlyDictionary<PipelineStatus, PipelineStatus[]> Dozwolone =
        new Dictionary<PipelineStatus, PipelineStatus[]>
        {
            [PipelineStatus.NowaAnkieta] = new[]
            {
                PipelineStatus.ZakwalifikowanyDoProgramu, PipelineStatus.Wykluczony
            },
            [PipelineStatus.ZakwalifikowanyDoProgramu] = new[]
            {
                PipelineStatus.SkierowanieWystawione, PipelineStatus.Wykluczony
            },
            [PipelineStatus.SkierowanieWystawione] = new[] { PipelineStatus.BadaniaUmowione },
            [PipelineStatus.BadaniaUmowione] = new[] { PipelineStatus.BadaniaWToku },
            [PipelineStatus.BadaniaWToku] = new[]
            {
                PipelineStatus.WynikiCzesciowe, PipelineStatus.WynikiKomplet
            },
            [PipelineStatus.WynikiCzesciowe] = new[]
            {
                PipelineStatus.WynikiKomplet, PipelineStatus.WizytaPodsumowujacaUmowiona
            },
            [PipelineStatus.WynikiKomplet] = new[] { PipelineStatus.WizytaPodsumowujacaUmowiona },
            [PipelineStatus.WizytaPodsumowujacaUmowiona] = new[] { PipelineStatus.WizytaZrealizowana },
            [PipelineStatus.WizytaZrealizowana] = new[] { PipelineStatus.DoRozliczenia },
            [PipelineStatus.DoRozliczenia] = new[]
            {
                PipelineStatus.RozliczonyNFZ, PipelineStatus.Zamkniety
            },
            [PipelineStatus.RozliczonyNFZ] = new[] { PipelineStatus.Zamkniety },
            [PipelineStatus.Wykluczony] = Array.Empty<PipelineStatus>(),
            [PipelineStatus.Zamkniety] = Array.Empty<PipelineStatus>()
        };

    public static bool CzyMoznaPrzejsc(PipelineStatus z, PipelineStatus na) =>
        Dozwolone.TryGetValue(z, out var dozwolone) && dozwolone.Contains(na);

    public static IReadOnlyCollection<PipelineStatus> MozliweNastepne(PipelineStatus z) =>
        Dozwolone.TryGetValue(z, out var dozwolone) ? dozwolone : Array.Empty<PipelineStatus>();

    public static bool JestTerminalny(PipelineStatus status) =>
        MozliweNastepne(status).Count == 0;
}
