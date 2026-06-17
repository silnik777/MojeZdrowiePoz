using FluentAssertions;
using MZ.Application.Services;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using Xunit;

namespace MZ.Application.Tests;

public class CompletenessServiceTests
{
    private static OrderedTest Zlecone(Guid def, bool wymagane = true) =>
        new() { TestDefinitionId = def, Wymagane = wymagane };

    private static TestResult Wynik(Guid def) =>
        new() { TestDefinitionId = def };

    [Fact]
    public void Wykrywa_brakujace_wymagane_badania()
    {
        var a = Guid.NewGuid(); var b = Guid.NewGuid(); var c = Guid.NewGuid();
        var zlecone = new[] { Zlecone(a), Zlecone(b), Zlecone(c) };
        var wyniki = new[] { Wynik(a) }; // wykonano tylko 1 z 3

        var wynik = new CompletenessService().Sprawdz(zlecone, wyniki);

        wynik.BrakujaceWymagane.Should().BeEquivalentTo(new[] { b, c });
        wynik.Kompletne.Should().BeFalse();
        wynik.SugerowanyStatus.Should().Be(PipelineStatus.WynikiCzesciowe);
    }

    [Fact]
    public void Komplet_gdy_wszystkie_wymagane_wykonane()
    {
        var a = Guid.NewGuid(); var b = Guid.NewGuid();
        var wynik = new CompletenessService().Sprawdz(
            new[] { Zlecone(a), Zlecone(b) },
            new[] { Wynik(a), Wynik(b) });

        wynik.Kompletne.Should().BeTrue();
        wynik.SugerowanyStatus.Should().Be(PipelineStatus.WynikiKomplet);
    }

    [Fact]
    public void Brak_opcjonalnego_nie_psuje_kompletnosci_ale_jest_raportowany()
    {
        var a = Guid.NewGuid(); var opc = Guid.NewGuid();
        var wynik = new CompletenessService().Sprawdz(
            new[] { Zlecone(a), Zlecone(opc, wymagane: false) },
            new[] { Wynik(a) });

        wynik.Kompletne.Should().BeTrue();
        wynik.BrakujaceOpcjonalne.Should().BeEquivalentTo(new[] { opc });
    }

    [Fact]
    public void Wynik_bez_zlecenia_jest_nadmiarowy()
    {
        var a = Guid.NewGuid(); var extra = Guid.NewGuid();
        var wynik = new CompletenessService().Sprawdz(
            new[] { Zlecone(a) },
            new[] { Wynik(a), Wynik(extra) });

        wynik.Nadmiarowe.Should().BeEquivalentTo(new[] { extra });
    }
}
