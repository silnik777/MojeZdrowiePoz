using MZ.Domain.Entities;
using MZ.Domain.Enums;

namespace MZ.Application.Services;

/// <summary>Dobiera pakiety badań pasujące do pacjenta wg wieku, płci i czynników ryzyka.</summary>
public class PackageSelectionService
{
    /// <summary>
    /// Zwraca pakiety obowiązujące na dany dzień, pasujące do parametrów pacjenta.
    /// Łączy pakiet podstawowy z pasującymi pakietami rozszerzonymi.
    /// </summary>
    public IReadOnlyList<TestPackage> DobierzPakiety(
        IEnumerable<TestPackage> dostepne,
        int wiek,
        Plec plec,
        IEnumerable<string> czynnikiRyzyka,
        DateOnly naDzien)
    {
        var ryzyko = czynnikiRyzyka as ICollection<string> ?? czynnikiRyzyka.ToArray();

        return dostepne
            .Where(p => Obowiazuje(p, naDzien) && p.Pasuje(wiek, plec, ryzyko))
            .OrderBy(p => p.Typ)
            .ThenBy(p => p.Nazwa)
            .ToList();
    }

    /// <summary>Zbiór unikalnych badań (definicji) wynikających z dobranych pakietów.</summary>
    public IReadOnlyList<TestDefinition> Badania(IEnumerable<TestPackage> pakiety) =>
        pakiety.SelectMany(p => p.Pozycje)
            .Select(pi => pi.TestDefinition)
            .Where(td => td is not null)
            .DistinctBy(td => td.Id)
            .ToList();

    private static bool Obowiazuje(TestPackage p, DateOnly dzien) =>
        p.ObowiazujeOd <= dzien && (p.ObowiazujeDo is null || p.ObowiazujeDo >= dzien);
}
