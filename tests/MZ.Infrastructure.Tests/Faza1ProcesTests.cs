using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MZ.Application.Dtos;
using MZ.Domain.Enums;
using Xunit;

namespace MZ.Infrastructure.Tests;

/// <summary>Test integracyjny pełnej ścieżki Fazy 1: pacjent → przypadek → ankieta → kwalifikacja → skierowanie.</summary>
public class Faza1ProcesTests
{
    [Fact]
    public async Task Pelna_sciezka_mezczyzna_52_konczy_sie_skierowaniem_z_psa()
    {
        using var ctx = new TestowyKontekst();

        var pacjent = await ctx.Patients.UtworzAsync(new PatientInput(
            "70010112345", "Jan", "Kowalski", new DateOnly(1974, 1, 1),
            Plec.Mezczyzna, "500100200", null, ZgodaSms: true, DataOstatniegoBilansu: null));

        var przypadek = await ctx.Enrollments.UtworzAsync(pacjent.Id);
        przypadek.Status.Should().Be(PipelineStatus.NowaAnkieta);
        przypadek.TerminKontaktu.Should().BeNull();

        // Ankieta z czynnikami ryzyka (palenie + brak aktywności + wywiad rodzinny = 2+1+2).
        var odp = new Dictionary<string, string>
        {
            ["PALENIE"] = "TAK",
            ["AKTYWNOSC_FIZYCZNA"] = "NIE",
            ["WYWIAD_RODZINNY_CHUK"] = "TAK"
        };
        var dataWplywu = new DateOnly(2026, 6, 1);
        var wynik = await ctx.Questionnaires.ZapiszAsync(przypadek.Id,
            new QuestionnaireInput(ZrodloAnkiety.WprowadzonaRecznie, null, dataWplywu, odp, "Notatka testowa"));

        wynik.Score.Should().Be(5);
        wynik.CzynnikiRyzyka.Should().Contain("PALENIE");

        // Termin kontaktu = 30 dni od wpływu.
        var poAnkiecie = await ctx.Enrollments.PobierzZeSzczegolamiAsync(przypadek.Id);
        poAnkiecie!.TerminKontaktu.Should().Be(dataWplywu.AddDays(30));
        poAnkiecie.Ankieta!.Notatki.Should().Be("Notatka testowa");

        // Kwalifikacja.
        await ctx.Qualifications.KwalifikujAsync(przypadek.Id, DecyzjaKwalifikacji.Zakwalifikowany, "OK");
        var poKwalifikacji = await ctx.Enrollments.PobierzZeSzczegolamiAsync(przypadek.Id);
        poKwalifikacji!.Status.Should().Be(PipelineStatus.ZakwalifikowanyDoProgramu);
        poKwalifikacji.Kwalifikacja!.CzynnikiRyzyka.Should().Contain("PALENIE");

        // Skierowanie — dobór pakietu: podstawowy + PSA (mężczyzna 52).
        var skierowanie = await ctx.Qualifications.WystawSkierowanieAsync(przypadek.Id, laboratoriumId: null);
        var kody = skierowanie.Zlecone
            .Select(z => ctx.Db.TestDefinitions.First(d => d.Id == z.TestDefinitionId).Kod)
            .ToList();

        kody.Should().Contain(new[] { "MORF", "GLU", "KREA", "LIPID", "TSH", "PSA" });
        kody.Should().NotContain("LPA"); // lipoproteina(a) tylko 20–40

        var koniec = await ctx.Enrollments.PobierzZeSzczegolamiAsync(przypadek.Id);
        koniec!.Status.Should().Be(PipelineStatus.SkierowanieWystawione);
    }

    [Fact]
    public async Task Powtorne_wystawienie_skierowania_jest_blokowane()
    {
        using var ctx = new TestowyKontekst();
        var p = await ctx.Patients.UtworzAsync(new PatientInput(
            "90020254321", "Anna", "Nowak", new DateOnly(1990, 2, 2),
            Plec.Kobieta, null, null, false, null));
        var e = await ctx.Enrollments.UtworzAsync(p.Id);
        await ctx.Questionnaires.ZapiszAsync(e.Id, new QuestionnaireInput(
            ZrodloAnkiety.WprowadzonaRecznie, null, new DateOnly(2026, 6, 1),
            new Dictionary<string, string>(), null));
        await ctx.Qualifications.KwalifikujAsync(e.Id, DecyzjaKwalifikacji.Zakwalifikowany, null);
        await ctx.Qualifications.WystawSkierowanieAsync(e.Id, null);

        var act = async () => await ctx.Qualifications.WystawSkierowanieAsync(e.Id, null);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Wykluczenie_ustawia_status_wykluczony()
    {
        using var ctx = new TestowyKontekst();
        var p = await ctx.Patients.UtworzAsync(new PatientInput(
            "85050567890", "Ewa", "Wiśniewska", new DateOnly(1985, 5, 5),
            Plec.Kobieta, null, null, false, null));
        var e = await ctx.Enrollments.UtworzAsync(p.Id);
        await ctx.Questionnaires.ZapiszAsync(e.Id, new QuestionnaireInput(
            ZrodloAnkiety.WprowadzonaRecznie, null, new DateOnly(2026, 6, 1),
            new Dictionary<string, string>(), null));
        await ctx.Qualifications.KwalifikujAsync(e.Id, DecyzjaKwalifikacji.Niezakwalifikowany, "poza kryteriami");

        var koniec = await ctx.Enrollments.PobierzZeSzczegolamiAsync(e.Id);
        koniec!.Status.Should().Be(PipelineStatus.Wykluczony);
    }
}
