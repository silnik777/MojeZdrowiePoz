using MZ.Domain.Common;
using MZ.Domain.Enums;

namespace MZ.Domain.Entities;

/// <summary>Pacjent / uczestnik programu.</summary>
public class Patient : BaseEntity
{
    public string Pesel { get; set; } = string.Empty;
    public string Imie { get; set; } = string.Empty;
    public string Nazwisko { get; set; } = string.Empty;
    public DateOnly DataUrodzenia { get; set; }
    public Plec Plec { get; set; }

    public string? Telefon { get; set; }
    public string? Email { get; set; }

    /// <summary>Zgoda na powiadomienia SMS (RODO) — warunek wysyłki przypomnień.</summary>
    public bool ZgodaSms { get; set; }

    /// <summary>Data ostatniego zrealizowanego bilansu — podstawa wyznaczenia uprawnienia.</summary>
    public DateOnly? DataOstatniegoBilansu { get; set; }

    public bool Aktywny { get; set; } = true;

    public ICollection<ProgramEnrollment> Udzialy { get; set; } = new List<ProgramEnrollment>();

    /// <summary>Wiek pacjenta na wskazany dzień.</summary>
    public int Wiek(DateOnly naDzien)
    {
        var wiek = naDzien.Year - DataUrodzenia.Year;
        if (DataUrodzenia > naDzien.AddYears(-wiek)) wiek--;
        return wiek;
    }

    /// <summary>
    /// Częstotliwość uprawnienia wg zasad programu: 20–49 lat co 5 lat, 50+ co 3 lata.
    /// </summary>
    public int CzestotliwoscLat(DateOnly naDzien) => Wiek(naDzien) <= 49 ? 5 : 3;

    /// <summary>Czy pacjent jest uprawniony do udziału na wskazany dzień (≥20 lat i upłynął okres).</summary>
    public bool CzyUprawniony(DateOnly naDzien)
    {
        if (Wiek(naDzien) < 20) return false;
        if (DataOstatniegoBilansu is null) return true;
        return DataOstatniegoBilansu.Value.AddYears(CzestotliwoscLat(naDzien)) <= naDzien;
    }
}
