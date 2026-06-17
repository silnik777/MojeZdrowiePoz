using MZ.Domain.Common;
using MZ.Domain.Enums;

namespace MZ.Domain.Entities;

/// <summary>Wygenerowany (i opcjonalnie podpisany) dokument PDF powiązany z przypadkiem.</summary>
public class SignedDocument : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public ProgramEnrollment Enrollment { get; set; } = null!;

    public TypDokumentu Typ { get; set; }
    public string PlikSciezka { get; set; } = string.Empty;

    public TypPodpisu TypPodpisu { get; set; } = TypPodpisu.Brak;
    public Guid? PodpisujacyId { get; set; }
    public AppUser? Podpisujacy { get; set; }
    public DateTimeOffset? DataPodpisu { get; set; }

    public bool Podpisany => TypPodpisu != TypPodpisu.Brak && DataPodpisu is not null;
}
