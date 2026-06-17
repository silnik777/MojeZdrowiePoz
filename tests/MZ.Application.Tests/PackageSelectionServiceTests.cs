using FluentAssertions;
using MZ.Application.Services;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using Xunit;

namespace MZ.Application.Tests;

public class PackageSelectionServiceTests
{
    private static readonly DateOnly Od = new(2025, 5, 1);
    private static readonly DateOnly Dzien = new(2026, 6, 16);

    private static List<TestPackage> Pakiety() => new()
    {
        new TestPackage { Nazwa = "Podstawowy", Typ = TypPakietu.Podstawowy, WiekMin = 20, ObowiazujeOd = Od },
        new TestPackage { Nazwa = "PSA M50+", Typ = TypPakietu.Rozszerzony, WiekMin = 50, Plec = Plec.Mezczyzna, ObowiazujeOd = Od },
        new TestPackage { Nazwa = "LPA 20-40", Typ = TypPakietu.Rozszerzony, WiekMin = 20, WiekMax = 40, ObowiazujeOd = Od }
    };

    [Fact]
    public void Mezczyzna_52_dostaje_podstawowy_i_psa()
    {
        var wybrane = new PackageSelectionService()
            .DobierzPakiety(Pakiety(), wiek: 52, Plec.Mezczyzna, Array.Empty<string>(), Dzien);

        wybrane.Select(p => p.Nazwa).Should().BeEquivalentTo("Podstawowy", "PSA M50+");
    }

    [Fact]
    public void Kobieta_30_dostaje_podstawowy_i_lpa_bez_psa()
    {
        var wybrane = new PackageSelectionService()
            .DobierzPakiety(Pakiety(), wiek: 30, Plec.Kobieta, Array.Empty<string>(), Dzien);

        wybrane.Select(p => p.Nazwa).Should().BeEquivalentTo("Podstawowy", "LPA 20-40");
    }

    [Fact]
    public void Pakiet_poza_okresem_obowiazywania_jest_pomijany()
    {
        var pakiety = Pakiety();
        pakiety[0].ObowiazujeDo = new DateOnly(2025, 12, 31);

        var wybrane = new PackageSelectionService()
            .DobierzPakiety(pakiety, wiek: 30, Plec.Kobieta, Array.Empty<string>(), Dzien);

        wybrane.Select(p => p.Nazwa).Should().NotContain("Podstawowy");
    }
}
