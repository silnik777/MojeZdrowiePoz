using FluentAssertions;
using MZ.Application.Services;
using MZ.Domain.Entities;
using Xunit;

namespace MZ.Application.Tests;

public class ScoringServiceTests
{
    private static ScoringRuleSet Zestaw() => new()
    {
        Reguly = new List<ScoringRule>
        {
            new() { KodPytania = "PALENIE", WartoscDopasowania = "TAK", Punkty = 2, CzynnikRyzyka = "PALENIE" },
            new() { KodPytania = "AKTYWNOSC", WartoscDopasowania = "NIE", Punkty = 1, CzynnikRyzyka = "MALA_AKTYWNOSC" }
        }
    };

    private static Questionnaire Ankieta(params (string kod, string wartosc)[] odp) => new()
    {
        Odpowiedzi = odp.Select(o => new QuestionnaireAnswer { KodPytania = o.kod, Wartosc = o.wartosc }).ToList()
    };

    [Fact]
    public void Sumuje_punkty_i_wykrywa_czynniki_ryzyka()
    {
        var wynik = new ScoringService().Oblicz(Ankieta(("PALENIE", "TAK"), ("AKTYWNOSC", "NIE")), Zestaw());

        wynik.Score.Should().Be(3);
        wynik.CzynnikiRyzyka.Should().BeEquivalentTo("PALENIE", "MALA_AKTYWNOSC");
    }

    [Fact]
    public void Niedopasowana_odpowiedz_nie_dolicza_punktow()
    {
        var wynik = new ScoringService().Oblicz(Ankieta(("PALENIE", "NIE")), Zestaw());

        wynik.Score.Should().Be(0);
        wynik.CzynnikiRyzyka.Should().BeEmpty();
    }
}
