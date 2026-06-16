using MZ.Domain.Common;

namespace MZ.Domain.Entities;

/// <summary>Wizyta podsumowująca — pomiary, analiza wyników, podstawa IPZ.</summary>
public class Visit : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public ProgramEnrollment Enrollment { get; set; } = null!;

    public DateOnly DataWizyty { get; set; }

    public Guid? PersonelId { get; set; }
    public AppUser? Personel { get; set; }

    public int? CisnienieSkurczowe { get; set; }
    public int? CisnienieRozkurczowe { get; set; }
    public int? Tetno { get; set; }
    public double? WzrostCm { get; set; }
    public double? WagaKg { get; set; }
    public double? ObwodTaliiCm { get; set; }

    public string? PodsumowanieWynikow { get; set; }
    public string? Zalecenia { get; set; }

    public IndividualHealthPlan? Ipz { get; set; }

    /// <summary>BMI wyliczane z wzrostu i wagi (kg/m²).</summary>
    public double? Bmi =>
        WzrostCm is > 0 && WagaKg is > 0
            ? Math.Round(WagaKg.Value / Math.Pow(WzrostCm.Value / 100.0, 2), 1)
            : null;
}

/// <summary>Indywidualny Plan Zdrowotny (IPZ) tworzony na wizycie podsumowującej.</summary>
public class IndividualHealthPlan : BaseEntity
{
    public Guid VisitId { get; set; }
    public Visit Visit { get; set; } = null!;

    public Guid EnrollmentId { get; set; }

    public List<string> Cele { get; set; } = new();
    public List<string> Interwencje { get; set; } = new();
    public string? ZaleceniaSzczepien { get; set; }
    public string? DalszaDiagnostyka { get; set; }

    public DateOnly DataUtworzenia { get; set; }
    public DateOnly? DataPrzegladu { get; set; }
}
