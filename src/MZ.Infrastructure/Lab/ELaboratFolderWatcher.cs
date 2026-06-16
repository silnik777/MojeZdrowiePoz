using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MZ.Infrastructure.Persistence;
using MZ.Infrastructure.Services;

namespace MZ.Infrastructure.Lab;

/// <summary>
/// Cyklicznie skanuje folder eksportu eLaborat (Menedżer Eksportu) w poszukiwaniu plików XML
/// z wynikami i importuje je. Pliki przetworzone trafiają do podfolderu „archiwum", błędne do
/// „bledy". Nieaktywny, jeśli folder nie jest skonfigurowany (sekcja ELaborat:WatchFolder).
/// </summary>
public class ELaboratFolderWatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<ELaboratFolderWatcher> _log;

    public ELaboratFolderWatcher(IServiceScopeFactory scopeFactory, IConfiguration config,
        ILogger<ELaboratFolderWatcher> log)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _log = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var folder = _config.GetValue<string>("ELaborat:WatchFolder");
        if (string.IsNullOrWhiteSpace(folder))
        {
            _log.LogInformation("ELaborat: nasłuch folderu wyłączony (brak ELaborat:WatchFolder).");
            return;
        }

        var interwal = TimeSpan.FromSeconds(Math.Max(15, _config.GetValue("ELaborat:IntervalSeconds", 60)));
        _log.LogInformation("ELaborat: nasłuch folderu {Folder}, co {Sek}s.", folder, interwal.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PrzetworzFolderAsync(folder, stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "ELaborat: błąd przetwarzania folderu.");
            }
            await Task.Delay(interwal, stoppingToken);
        }
    }

    private async Task PrzetworzFolderAsync(string folder, CancellationToken ct)
    {
        if (!Directory.Exists(folder)) return;
        var archiwum = Path.Combine(folder, "archiwum");
        var bledy = Path.Combine(folder, "bledy");
        Directory.CreateDirectory(archiwum);
        Directory.CreateDirectory(bledy);

        var pliki = Directory.EnumerateFiles(folder, "*.xml", SearchOption.TopDirectoryOnly).ToList();
        if (pliki.Count == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var import = scope.ServiceProvider.GetRequiredService<LabResultImportService>();
        var db = scope.ServiceProvider.GetRequiredService<MzDbContext>();
        var laboratoriumId = await UstalLaboratoriumAsync(db, ct);
        if (laboratoriumId is null)
        {
            _log.LogWarning("ELaborat: nie ustalono laboratorium docelowego — pomijam import.");
            return;
        }

        foreach (var plik in pliki)
        {
            try
            {
                await using (var fs = File.OpenRead(plik))
                {
                    var wynik = await import.ImportujAsync(fs, laboratoriumId.Value, ct);
                    _log.LogInformation("ELaborat: {Plik} — dokumentów {D}, dopasowanych {M}, wyników {W}.",
                        Path.GetFileName(plik), wynik.Dokumentow, wynik.Dopasowanych, wynik.ZapisanychWynikow);
                }
                Przenies(plik, archiwum);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "ELaborat: błąd pliku {Plik}.", plik);
                Przenies(plik, bledy);
            }
        }
    }

    private async Task<Guid?> UstalLaboratoriumAsync(MzDbContext db, CancellationToken ct)
    {
        var id = _config.GetValue<string>("ELaborat:LaboratoriumId");
        if (Guid.TryParse(id, out var g)) return g;

        var nazwa = _config.GetValue<string>("ELaborat:LaboratoriumNazwa");
        if (!string.IsNullOrWhiteSpace(nazwa))
        {
            var lab = await db.Laboratories.FirstOrDefaultAsync(l => l.Nazwa == nazwa, ct);
            if (lab is not null) return lab.Id;
        }

        var aktywne = await db.Laboratories.Where(l => l.Aktywny).Take(2).ToListAsync(ct);
        return aktywne.Count == 1 ? aktywne[0].Id : null;
    }

    private static void Przenies(string plik, string docelowy)
    {
        var cel = Path.Combine(docelowy, $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(plik)}");
        File.Move(plik, cel, overwrite: true);
    }
}
