using MZ.Domain.Common;
using MZ.Domain.Enums;

namespace MZ.Domain.Entities;

/// <summary>
/// Centralna encja procesu — pojedynczy „przypadek" udziału pacjenta w programie,
/// prowadzony przez kolejne statusy pipeline.
/// </summary>
public class ProgramEnrollment : BaseEntity
{
    public Guid PatientId { get; set; }
    public Patient Patient { get; set; } = null!;

    public PipelineStatus Status { get; set; } = PipelineStatus.NowaAnkieta;

    public DateOnly DataRozpoczecia { get; set; }
    public DateOnly? DataKwalifikacji { get; set; }

    /// <summary>Termin (30 dni) na kontakt z pacjentem po wpływie ankiety.</summary>
    public DateOnly? TerminKontaktu { get; set; }

    /// <summary>Identyfikator ankiety KBZOD wymagany w rozliczeniu SWIAD (POZ-MOJEZ).</summary>
    public string? IdAnkietyKbzod { get; set; }

    public Guid? PrzypisanyPakietId { get; set; }
    public TestPackage? PrzypisanyPakiet { get; set; }

    public Guid? KoordynatorId { get; set; }
    public AppUser? Koordynator { get; set; }

    public string? Uwagi { get; set; }

    /// <summary>Wyliczana flaga: czy występują braki wymaganych badań względem zlecenia.</summary>
    public bool MaBrakiBadan { get; set; }

    public Questionnaire? Ankieta { get; set; }
    public Qualification? Kwalifikacja { get; set; }
    public Referral? Skierowanie { get; set; }
    public Visit? Wizyta { get; set; }
    public ICollection<TestResult> Wyniki { get; set; } = new List<TestResult>();
    public ICollection<PipelineHistory> Historia { get; set; } = new List<PipelineHistory>();
    public ICollection<Appointment> Terminy { get; set; } = new List<Appointment>();
}
