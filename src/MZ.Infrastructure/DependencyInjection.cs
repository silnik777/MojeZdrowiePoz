using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MZ.Application.Abstractions;
using MZ.Application.Services;
using MZ.Infrastructure.Identity;
using MZ.Infrastructure.Persistence;

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

        return services;
    }
}
