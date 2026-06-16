namespace MZ.Web.Kbzod;

/// <summary>Pojedyncze pytanie ankiety do formularza (kod zgodny z regułami scoringu).</summary>
public record KbzodPytanie(string Kod, string Tresc, string[] Opcje);

/// <summary>
/// Uproszczony, statyczny zestaw pytań KBZOD na potrzeby Fazy 1. Kody odpowiadają regułom
/// scoringu z seeda. Docelowo lista będzie konfigurowalna / pobierana z definicji KBZOD.
/// </summary>
public static class KbzodPytania
{
    private static readonly string[] TakNie = { "TAK", "NIE" };

    public static readonly IReadOnlyList<KbzodPytanie> Lista = new List<KbzodPytanie>
    {
        new("PALENIE", "Czy pali Pan/Pani papierosy?", TakNie),
        new("AKTYWNOSC_FIZYCZNA", "Czy podejmuje Pan/Pani regularną aktywność fizyczną?", TakNie),
        new("WYWIAD_RODZINNY_CHUK", "Czy w rodzinie wystąpiły choroby układu krążenia?", TakNie),
        new("NADCISNIENIE", "Czy rozpoznano u Pana/Pani nadciśnienie tętnicze?", TakNie),
        new("ALKOHOL", "Czy spożywa Pan/Pani alkohol ryzykownie?", TakNie)
    };
}
