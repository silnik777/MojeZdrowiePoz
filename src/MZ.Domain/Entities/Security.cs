using MZ.Domain.Common;
using MZ.Domain.Enums;

namespace MZ.Domain.Entities;

/// <summary>Użytkownik aplikacji — mapowany z konta domenowego AD, przechowywany lokalnie do audytu i powiązań.</summary>
public class AppUser : BaseEntity
{
    /// <summary>Identyfikator domenowy (UPN/SID) z Active Directory.</summary>
    public string NazwaLogowania { get; set; } = string.Empty;
    public string ImieNazwisko { get; set; } = string.Empty;
    public RolaUzytkownika Rola { get; set; }
    public bool Aktywny { get; set; } = true;
    public DateTimeOffset? OstatnieLogowanie { get; set; }
}

/// <summary>Wpis logu audytu (append-only) — wymóg RODO dla dostępu do danych pacjenta.</summary>
public class AuditLog : BaseEntity
{
    public Guid? UzytkownikId { get; set; }
    public string? NazwaLogowania { get; set; }

    public DateTimeOffset DataCzas { get; set; }
    public AkcjaAudytu Akcja { get; set; }

    public string Encja { get; set; } = string.Empty;
    public string? EncjaId { get; set; }
    public Guid? PatientId { get; set; }

    public string? Stacja { get; set; }
    public string? SzczegolyJson { get; set; }
}
