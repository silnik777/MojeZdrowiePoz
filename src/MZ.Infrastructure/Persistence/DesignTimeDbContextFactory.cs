using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MZ.Infrastructure.Persistence;

/// <summary>
/// Fabryka używana przez narzędzia EF Core (dotnet ef) do generowania migracji.
/// Migracje celują w SQL Server (środowisko produkcyjne); połączenie nie musi być aktywne.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<MzDbContext>
{
    public MzDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MzDbContext>()
            .UseSqlServer("Server=.;Database=MojeZdrowie;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new MzDbContext(options);
    }
}
