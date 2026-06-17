using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MZ.Application.Abstractions;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

public record SmsProba(bool Wyslano, string Info);

/// <summary>Wysyłka i rejestracja SMS do pacjentów. Treść neutralna — bez danych medycznych (RODO).</summary>
public class SmsService
{
    private readonly MzDbContext _db;
    private readonly ISmsGateway _gateway;
    private readonly IAuditService _audit;
    private readonly TimeProvider _time;
    private readonly string _placowka;

    public SmsService(MzDbContext db, ISmsGateway gateway, IAuditService audit, TimeProvider time, IConfiguration config)
    {
        _db = db;
        _gateway = gateway;
        _audit = audit;
        _time = time;
        _placowka = config.GetValue<string>("Placowka:Nazwa") ?? "Przychodnia";
    }

    /// <summary>Buduje neutralną treść powiadomienia (bez informacji o stanie zdrowia).</summary>
    public string BudujTresc(TypPowiadomienia typ, Appointment? termin = null)
    {
        var kiedy = termin is not null ? termin.DataGodzina.LocalDateTime.ToString("dd.MM.yyyy HH:mm") : "";
        return typ switch
        {
            TypPowiadomienia.PrzypomnienieOTerminie =>
                $"Przypomnienie o terminie {kiedy}. {_placowka}.",
            TypPowiadomienia.WynikiGotowe =>
                $"Wyniki badan sa gotowe. Prosimy o kontakt w celu umowienia wizyty. {_placowka}.",
            TypPowiadomienia.ZaproszenieNaWizyte =>
                $"Zapraszamy na wizyte podsumowujaca. Prosimy o kontakt. {_placowka}.",
            TypPowiadomienia.KontaktTerminowy30Dni =>
                $"Prosimy o kontakt w sprawie badan profilaktycznych. {_placowka}.",
            _ => $"Prosimy o kontakt. {_placowka}."
        };
    }

    /// <summary>Wysyła SMS do pacjenta danego przypadku (gdy jest zgoda i numer) i zapisuje rekord.</summary>
    public async Task<SmsProba> WyslijDoPacjentaAsync(Guid enrollmentId, TypPowiadomienia typ,
        Appointment? termin = null, Guid? reminderId = null, CancellationToken ct = default)
    {
        var e = await _db.Enrollments.Include(x => x.Patient).FirstOrDefaultAsync(x => x.Id == enrollmentId, ct)
                ?? throw new InvalidOperationException("Nie znaleziono przypadku.");

        if (!e.Patient.ZgodaSms) return new SmsProba(false, "Brak zgody pacjenta na SMS.");
        if (string.IsNullOrWhiteSpace(e.Patient.Telefon)) return new SmsProba(false, "Brak numeru telefonu.");

        var tresc = BudujTresc(typ, termin);
        var wynik = await _gateway.WyslijAsync(e.Patient.Telefon, tresc, ct);

        var sms = new SmsMessage
        {
            EnrollmentId = e.Id, ReminderId = reminderId, NumerTelefonu = e.Patient.Telefon,
            Tresc = tresc, Typ = typ,
            Status = wynik.Sukces ? StatusSms.Wyslany : StatusSms.Blad,
            DataWyslania = wynik.Sukces ? _time.GetUtcNow() : null,
            IdentyfikatorBramki = wynik.IdentyfikatorBramki, BladOpis = wynik.Blad
        };
        _db.SmsMessages.Add(sms);
        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Utworzenie, nameof(SmsMessage), sms.Id.ToString(), e.PatientId, ct: ct);

        return new SmsProba(wynik.Sukces, wynik.Sukces ? "Wysłano SMS." : $"Błąd: {wynik.Blad}");
    }

    public async Task<List<SmsMessage>> HistoriaAsync(Guid enrollmentId, CancellationToken ct = default) =>
        // Sortowanie po stronie pamięci — SQLite nie obsługuje ORDER BY po DateTimeOffset.
        (await _db.SmsMessages.AsNoTracking().Where(s => s.EnrollmentId == enrollmentId).ToListAsync(ct))
            .OrderByDescending(s => s.UtworzonoUtc).ToList();
}
