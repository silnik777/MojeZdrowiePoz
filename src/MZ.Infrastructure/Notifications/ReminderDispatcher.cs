using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MZ.Infrastructure.Persistence;
using MZ.Infrastructure.Services;

namespace MZ.Infrastructure.Notifications;

/// <summary>
/// Cyklicznie wysyła zaplanowane przypomnienia, których czas już nadszedł (SMS przez bramkę).
/// Aktywny tylko gdy Notifications:Enabled = true. Respektuje zgodę pacjenta na SMS.
/// </summary>
public class ReminderDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ReminderDispatcher> _log;

    public ReminderDispatcher(IServiceScopeFactory scopeFactory, IConfiguration config, ILogger<ReminderDispatcher> log)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_config.GetValue("Notifications:Enabled", false))
        {
            _log.LogInformation("Powiadomienia: dyspozytor wyłączony (Notifications:Enabled = false).");
            return;
        }

        var interwal = TimeSpan.FromSeconds(Math.Max(30, _config.GetValue("Notifications:IntervalSeconds", 60)));
        _log.LogInformation("Powiadomienia: dyspozytor aktywny, co {Sek}s.", interwal.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try { await PrzetworzAsync(stoppingToken); }
            catch (Exception ex) { _log.LogError(ex, "Powiadomienia: błąd przetwarzania."); }
            await Task.Delay(interwal, stoppingToken);
        }
    }

    private async Task PrzetworzAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MzDbContext>();
        var sms = scope.ServiceProvider.GetRequiredService<SmsService>();
        var teraz = DateTimeOffset.UtcNow;

        // Filtr czasu w pamięci — porównania DateTimeOffset nie są tłumaczone przez SQLite.
        var doWyslania = (await db.Reminders
                .Include(r => r.Appointment)
                .Where(r => !r.Wyslane)
                .ToListAsync(ct))
            .Where(r => r.DataZaplanowana <= teraz)
            .Take(50)
            .ToList();

        foreach (var r in doWyslania)
        {
            var proba = await sms.WyslijDoPacjentaAsync(r.EnrollmentId, r.Typ, r.Appointment, r.Id, ct);
            r.Wyslane = true; // oznacz jako obsłużone niezależnie od wyniku (próba zarejestrowana w SmsMessage)
            await db.SaveChangesAsync(ct);
            if (!proba.Wyslano)
                _log.LogWarning("Powiadomienie {Id}: nie wysłano — {Info}", r.Id, proba.Info);
        }
    }
}
