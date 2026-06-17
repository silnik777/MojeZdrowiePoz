using Microsoft.EntityFrameworkCore;
using MZ.Domain.Entities;
using MZ.Domain.Enums;

namespace MZ.Infrastructure.Persistence;

/// <summary>
/// Wstępne dane słownikowe. PRZYKŁADOWE wartości — dokładny wykaz badań, progi i reguły
/// scoringu należy zweryfikować z aktualnym rozporządzeniem i skonfigurować w aplikacji.
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(MzDbContext db, CancellationToken ct = default)
    {
        if (await db.TestDefinitions.AnyAsync(ct)) return;

        var morfologia = Def("MORF", "Morfologia krwi");
        var glukoza = Def("GLU", "Glukoza", "mg/dl", 70, 99);
        var kreatynina = Def("KREA", "Kreatynina", "mg/dl", 0.6m, 1.3m);
        var lipidogram = Def("LIPID", "Lipidogram");
        var tsh = Def("TSH", "TSH", "mIU/l", 0.27m, 4.2m);
        var psa = Def("PSA", "PSA", "ng/ml", 0, 4);
        var lpa = Def("LPA", "Lipoproteina (a)", "nmol/l");

        var badania = new[] { morfologia, glukoza, kreatynina, lipidogram, tsh, psa, lpa };
        db.TestDefinitions.AddRange(badania);

        var od = new DateOnly(2025, 5, 1);

        // Pakiet podstawowy — dla wszystkich dorosłych 20+.
        var podstawowy = new TestPackage
        {
            Nazwa = "Pakiet podstawowy", Typ = TypPakietu.Podstawowy,
            WiekMin = 20, ObowiazujeOd = od,
            Pozycje = Pozycje(morfologia, glukoza, kreatynina, lipidogram, tsh)
        };

        // Pakiet rozszerzony — PSA dla mężczyzn 50+.
        var psaPakietM50 = new TestPackage
        {
            Nazwa = "PSA (mężczyźni 50+)", Typ = TypPakietu.Rozszerzony,
            WiekMin = 50, Plec = Plec.Mezczyzna, ObowiazujeOd = od,
            Pozycje = Pozycje(psa)
        };

        // Pakiet rozszerzony — lipoproteina(a) dla młodszych dorosłych.
        var lpaPakiet2040 = new TestPackage
        {
            Nazwa = "Lipoproteina(a) (20–40)", Typ = TypPakietu.Rozszerzony,
            WiekMin = 20, WiekMax = 40, ObowiazujeOd = od,
            Pozycje = Pozycje(lpa)
        };

        db.TestPackages.AddRange(podstawowy, psaPakietM50, lpaPakiet2040);

        // Przykładowy zestaw reguł scoringu (do dostosowania wg KBZOD).
        var zestaw = new ScoringRuleSet
        {
            Nazwa = "KBZOD — zestaw przykładowy", ObowiazujeOd = od,
            Reguly = new List<ScoringRule>
            {
                new() { KodPytania = "PALENIE", WartoscDopasowania = "TAK", Punkty = 2, CzynnikRyzyka = "PALENIE" },
                new() { KodPytania = "AKTYWNOSC_FIZYCZNA", WartoscDopasowania = "NIE", Punkty = 1, CzynnikRyzyka = "MALA_AKTYWNOSC" },
                new() { KodPytania = "WYWIAD_RODZINNY_CHUK", WartoscDopasowania = "TAK", Punkty = 2, CzynnikRyzyka = "WYWIAD_RODZINNY" }
            }
        };
        db.ScoringRuleSets.Add(zestaw);

        // Przykładowe laboratorium (integracja do zbadania — patrz plan).
        db.Laboratories.Add(new Laboratory
        {
            Nazwa = "CM Medyk Łańcut",
            TypIntegracji = TypIntegracjiLaboratorium.Brak,
            Aktywny = true
        });

        await db.SaveChangesAsync(ct);
    }

    private static TestDefinition Def(string kod, string nazwa, string? jednostka = null,
        decimal? min = null, decimal? max = null) =>
        new() { Kod = kod, Nazwa = nazwa, Jednostka = jednostka, NormaMin = min, NormaMax = max };

    private static List<PackageItem> Pozycje(params TestDefinition[] defs)
    {
        var lista = new List<PackageItem>();
        for (var i = 0; i < defs.Length; i++)
            lista.Add(new PackageItem { TestDefinition = defs[i], Wymagane = true, Kolejnosc = i });
        return lista;
    }
}
