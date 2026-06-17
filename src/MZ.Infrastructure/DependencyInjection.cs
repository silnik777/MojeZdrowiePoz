using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MZ.Application.Abstractions;
using MZ.Application.Services;
using MZ.Infrastructure.Documents;
using MZ.Infrastructure.Identity;
using MZ.Infrastructure.Lab;
using MZ.Infrastructure.Notifications;
using MZ.Infrastructure.Ocr;
using MZ.Infrastructure.Persistence;
using MZ.Infrastructure.Services;
using MZ.Infrastructure.Sms;

namespace MZ.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Rejestruje warstwę infrastruktury: DbContext, interceptor audytu, serwisy aplikacyjne.
    /// Dostawca bazy wybierany przez konfigurację (SqlServer produkcyjnie, Sqlite dla dev/testów).
    /// </summary>
    public static IServiceCollection AddMzInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton(TimeProvider.System);
        services.TryAddScoped<ICurrentUser, SystemCurrentUser>();

        services.AddScoped<AuditSaveChangesInterceptor>();

        var dostawca = config.GetValue<string>("Database:Provider") ?? "SqlServer";
        var connStr = config.GetConnectionString("MzDb")
                      ?? "Data Source=mzdb.sqlite";

        services.AddDbContext<MzDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetRequiredService<AuditSaveChangesInterceptor>());
            if (dostawca.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
                options.UseSqlite(connStr);
            else
                options.UseSqlServer(connStr);
        });

        services.AddScoped<IAuditService, DbAuditService>();

        // Serwisy domenowe/aplikacyjne (czysta logika).
        services.AddScoped<ScoringService>();
        services.AddScoped<PackageSelectionService>();
        services.AddScoped<CompletenessService>();

        // Adaptery (na razie zaślepka OCR — docelowo silnik OCR/OMR przetwarzający lokalnie).
        services.AddScoped<IOcrService, StubOcrService>();

        // Adapter wyników laboratorium: eLaborat (Marcel) HL7 CDA.
        services.AddScoped<ILabResultSource, ELaboratCdaAdapter>();

        // Serwisy aplikacyjne Fazy 1 (orkiestracja na EF Core).
        services.AddScoped<PatientService>();
        services.AddScoped<EnrollmentService>();
        services.AddScoped<QuestionnaireService>();
        services.AddScoped<QualificationService>();

        // Serwisy Fazy 2: import i kontrola kompletności wyników.
        services.AddScoped<LabResultImportService>();
        services.AddScoped<LabResultService>();

        // Nasłuch folderu eksportu eLaborat (aktywny tylko gdy skonfigurowano ELaborat:WatchFolder).
        services.AddHostedService<ELaboratFolderWatcher>();

        // Faza 3: terminarz, powiadomienia, bramka SMS (SMS Gate na Androidzie, tryb lokalny).
        services.AddHttpClient();
        services.AddScoped<ISmsGateway, SmsGateGateway>();
        services.AddScoped<AppointmentService>();
        services.AddScoped<SmsService>();
        services.AddScoped<NotificationService>();
        services.AddHostedService<ReminderDispatcher>();

        // Faza 4: wizyta, IPZ, dokumenty PDF.
        services.AddScoped<VisitService>();
        services.AddScoped<DocumentService>();

        return services;
    }
}
