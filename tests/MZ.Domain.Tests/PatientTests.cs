using FluentAssertions;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using Xunit;

namespace MZ.Domain.Tests;

public class PatientTests
{
    private static Patient Pacjent(DateOnly urodziny, DateOnly? ostatniBilans = null) => new()
    {
        DataUrodzenia = urodziny,
        Plec = Plec.Mezczyzna,
        DataOstatniegoBilansu = ostatniBilans
    };

    [Fact]
    public void Wiek_liczony_poprawnie_przed_urodzinami()
    {
        var p = Pacjent(new DateOnly(1974, 7, 1));
        p.Wiek(new DateOnly(2026, 6, 16)).Should().Be(51);
    }

    [Theory]
    [InlineData(2000, 5)]   // 26 lat -> co 5 lat
    [InlineData(1970, 3)]   // 56 lat -> co 3 lata
    public void Czestotliwosc_zalezy_od_grupy_wiekowej(int rokUrodzenia, int oczekiwane)
    {
        var p = Pacjent(new DateOnly(rokUrodzenia, 1, 1));
        p.CzestotliwoscLat(new DateOnly(2026, 6, 16)).Should().Be(oczekiwane);
    }

    [Fact]
    public void Ponizej_20_lat_nieuprawniony()
    {
        var p = Pacjent(new DateOnly(2010, 1, 1));
        p.CzyUprawniony(new DateOnly(2026, 6, 16)).Should().BeFalse();
    }

    [Fact]
    public void Uprawniony_gdy_uplynal_okres_od_ostatniego_bilansu()
    {
        var p = Pacjent(new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1)); // 36 lat, okres 5 lat
        p.CzyUprawniony(new DateOnly(2026, 6, 16)).Should().BeTrue();
    }

    [Fact]
    public void Nieuprawniony_gdy_okres_jeszcze_nie_uplynal()
    {
        var p = Pacjent(new DateOnly(1990, 1, 1), new DateOnly(2024, 1, 1)); // 36 lat, okres 5 lat
        p.CzyUprawniony(new DateOnly(2026, 6, 16)).Should().BeFalse();
    }
}
