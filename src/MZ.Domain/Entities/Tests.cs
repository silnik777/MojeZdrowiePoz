using MZ.Domain.Common;
using MZ.Domain.Enums;

namespace MZ.Domain.Entities;

/// <summary>Słownik definicji badań (konfigurowalny).</summary>
public class TestDefinition : BaseEntity
{
    public string Kod { get; set; } = string.Empty;
    public string Nazwa { get; set; } = string.Empty;
    public string? Jednostka { get; set; }
    public decimal? NormaMin { get; set; }
    public decimal? NormaMax { get; set; }
    public bool Aktywny { get; set; } = true;
}

/// <summary>
/// Konfigurowalny pakiet badań przypisywany wg kryteriów (wiek/płeć/czynnik ryzyka),
/// wersjonowany datami obowiązywania.
/// </summary>
public class TestPackage : BaseEntity
{
    public string Nazwa { get; set; } = string.Empty;
    public TypPakietu Typ { get; set; }
    public bool Aktywny { get; set; } = true;
    public DateOnly ObowiazujeOd { get; set; }
    public DateOnly? ObowiazujeDo { get; set; }

    public int WiekMin { get; set; }
    public int WiekMax { get; set; } = 200;
    /// <summary>Płeć, dla której pakiet obowiązuje; null = dowolna.</summary>
    public Plec? Plec { get; set; }
    /// <summary>Wymagany czynnik ryzyka (kod), jeśli pakiet warunkowy; null = bezwarunkowy.</summary>
    public string? WymaganyCzynnikRyzyka { get; set; }

    public ICollection<PackageItem> Pozycje { get; set; } = new List<PackageItem>();

    /// <summary>Czy pakiet pasuje do pacjenta o danym wieku/płci i zbiorze czynników ryzyka.</summary>
    public bool Pasuje(int wiek, Plec plec, IEnumerable<string> czynnikiRyzyka)
    {
        if (!Aktywny) return false;
        if (wiek < WiekMin || wiek > WiekMax) return false;
        if (Plec is not null && Plec != plec) return false;
        if (!string.IsNullOrWhiteSpace(WymaganyCzynnikRyzyka)
            && !czynnikiRyzyka.Contains(WymaganyCzynnikRyzyka)) return false;
        return true;
    }
}

/// <summary>Pozycja pakietu — powiązanie pakietu z konkretnym badaniem.</summary>
public class PackageItem : BaseEntity
{
    public Guid TestPackageId { get; set; }
    public TestPackage TestPackage { get; set; } = null!;

    public Guid TestDefinitionId { get; set; }
    public TestDefinition TestDefinition { get; set; } = null!;

    public bool Wymagane { get; set; } = true;
    public int Kolejnosc { get; set; }
}

/// <summary>Skierowanie/zlecenie na badania wystawione w ramach przypadku.</summary>
public class Referral : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public ProgramEnrollment Enrollment { get; set; } = null!;

    public DateOnly DataWystawienia { get; set; }

    public Guid? LekarzId { get; set; }
    public AppUser? Lekarz { get; set; }

    public Guid? LaboratoriumId { get; set; }
    public Laboratory? Laboratorium { get; set; }

    public ICollection<OrderedTest> Zlecone { get; set; } = new List<OrderedTest>();
}

/// <summary>Badanie zlecone — „migawka" zakresu w chwili wystawienia skierowania.</summary>
public class OrderedTest : BaseEntity
{
    public Guid ReferralId { get; set; }
    public Referral Referral { get; set; } = null!;

    public Guid TestDefinitionId { get; set; }
    public TestDefinition TestDefinition { get; set; } = null!;

    public bool Wymagane { get; set; } = true;
    public DateOnly DataZlecenia { get; set; }
}

/// <summary>Wynik faktycznie wykonanego badania.</summary>
public class TestResult : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public ProgramEnrollment Enrollment { get; set; } = null!;

    public Guid? OrderedTestId { get; set; }
    public OrderedTest? OrderedTest { get; set; }

    public Guid TestDefinitionId { get; set; }
    public TestDefinition TestDefinition { get; set; } = null!;

    public decimal? WartoscLiczbowa { get; set; }
    public string? WartoscTekstowa { get; set; }
    public string? Jednostka { get; set; }

    public DateOnly? DataPobrania { get; set; }
    public DateOnly? DataWyniku { get; set; }

    public OcenaNormy OcenaNormy { get; set; } = OcenaNormy.Nieokreslona;
    public ZrodloWyniku Zrodlo { get; set; }

    public Guid? LaboratoriumId { get; set; }
    public Laboratory? Laboratorium { get; set; }

    public string? SurowyRekord { get; set; }
}

/// <summary>Słownik laboratoriów wraz z typem integracji.</summary>
public class Laboratory : BaseEntity
{
    public string Nazwa { get; set; } = string.Empty;
    public string? Adres { get; set; }
    public string? Kontakt { get; set; }
    public TypIntegracjiLaboratorium TypIntegracji { get; set; } = TypIntegracjiLaboratorium.Brak;
    public string? KonfiguracjaJson { get; set; }
    public bool Aktywny { get; set; } = true;

    public ICollection<LabTestMapping> Mapowania { get; set; } = new List<LabTestMapping>();
}

/// <summary>Mapowanie kodu badania z laboratorium na wewnętrzną definicję badania.</summary>
public class LabTestMapping : BaseEntity
{
    public Guid LaboratoriumId { get; set; }
    public Laboratory Laboratorium { get; set; } = null!;

    public string KodLab { get; set; } = string.Empty;

    public Guid TestDefinitionId { get; set; }
    public TestDefinition TestDefinition { get; set; } = null!;
}
