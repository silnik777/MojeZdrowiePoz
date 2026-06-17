using MZ.Domain.Common;
using MZ.Domain.Enums;

namespace MZ.Domain.Entities;

/// <summary>Decyzja kwalifikacyjna do programu wraz z czynnikami ryzyka.</summary>
public class Qualification : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public ProgramEnrollment Enrollment { get; set; } = null!;

    public Guid? UzytkownikId { get; set; }
    public AppUser? Uzytkownik { get; set; }

    public DecyzjaKwalifikacji Decyzja { get; set; }
    public DateOnly DataDecyzji { get; set; }

    /// <summary>Lista czynników ryzyka (kody), wpływa na dobór pakietu rozszerzonego.</summary>
    public List<string> CzynnikiRyzyka { get; set; } = new();

    public string? Uzasadnienie { get; set; }
}
