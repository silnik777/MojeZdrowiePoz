using MZ.Domain.Common;
using MZ.Domain.Enums;

namespace MZ.Domain.Entities;

/// <summary>Termin w terminarzu (pobranie badań lub wizyta podsumowująca).</summary>
public class Appointment : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public ProgramEnrollment Enrollment { get; set; } = null!;

    public TypTerminu Typ { get; set; }
    public DateTimeOffset DataGodzina { get; set; }
    public StatusTerminu Status { get; set; } = StatusTerminu.Zaplanowany;

    public Guid? LaboratoriumId { get; set; }
    public Laboratory? Laboratorium { get; set; }

    public Guid? PersonelId { get; set; }
    public AppUser? Personel { get; set; }

    public ICollection<Reminder> Przypomnienia { get; set; } = new List<Reminder>();
}

/// <summary>Zaplanowane przypomnienie powiązane z terminem/przypadkiem.</summary>
public class Reminder : BaseEntity
{
    public Guid? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    public Guid EnrollmentId { get; set; }

    public TypPowiadomienia Typ { get; set; }
    public DateTimeOffset DataZaplanowana { get; set; }
    public bool Wyslane { get; set; }
}

/// <summary>Rekord wysłanej wiadomości SMS (treść neutralna, bez danych wrażliwych).</summary>
public class SmsMessage : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public Guid? ReminderId { get; set; }

    public string NumerTelefonu { get; set; } = string.Empty;
    public string Tresc { get; set; } = string.Empty;
    public TypPowiadomienia Typ { get; set; }

    public StatusSms Status { get; set; } = StatusSms.Zakolejkowany;
    public DateTimeOffset? DataWyslania { get; set; }
    public string? IdentyfikatorBramki { get; set; }
    public string? BladOpis { get; set; }
}
