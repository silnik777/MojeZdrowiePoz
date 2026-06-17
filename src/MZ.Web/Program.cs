using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using MZ.Application.Abstractions;
using MZ.Domain.Enums;
using MZ.Infrastructure;
using MZ.Infrastructure.Persistence;
using MZ.Web.Components;
using MZ.Web.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Warstwa infrastruktury (DbContext, audyt, serwisy).
builder.Services.AddMzInfrastructure(builder.Configuration);

// Bieżący użytkownik z kontekstu AD/HTTP (nadpisuje fallback z infrastruktury).
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<AdOptions>(builder.Configuration.GetSection("Ad"));
builder.Services.AddSingleton<AdRoleMapper>();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// Uwierzytelnianie zintegrowane Windows (Kerberos/Negotiate) — działa pod IIS w domenie AD.
builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme).AddNegotiate();

// Polityki autoryzacji per rola programu.
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Rejestracja", p => p.RequireRole(RoleGroups(builder.Configuration, RolaUzytkownika.Rejestracja)))
    .AddPolicy("Pielegniarka", p => p.RequireRole(RoleGroups(builder.Configuration, RolaUzytkownika.Pielegniarka)))
    .AddPolicy("Lekarz", p => p.RequireRole(RoleGroups(builder.Configuration, RolaUzytkownika.Lekarz)))
    .AddPolicy("Administrator", p => p.RequireRole(RoleGroups(builder.Configuration, RolaUzytkownika.Administrator)));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Pobieranie wygenerowanych dokumentów PDF (dostęp chroniony uwierzytelnianiem Windows na IIS).
app.MapGet("/dokument/{id:guid}", async (Guid id, MZ.Infrastructure.Documents.DocumentService docs) =>
{
    var d = await docs.PobierzAsync(id);
    if (d is null || string.IsNullOrEmpty(d.PlikSciezka) || !File.Exists(d.PlikSciezka))
        return Results.NotFound();
    var bytes = await File.ReadAllBytesAsync(d.PlikSciezka);
    return Results.File(bytes, "application/pdf", Path.GetFileName(d.PlikSciezka));
});

await InicjalizujBazeAsync(app);

app.Run();

// Inicjalizacja bazy: migracje dla SQL Server, EnsureCreated dla Sqlite (dev), oraz seed słowników.
static async Task InicjalizujBazeAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<MzDbContext>();
    if (db.Database.IsSqlite())
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

// Nazwy grup AD przypisane do roli (z konfiguracji); brak konfiguracji => nazwa roli jako fallback.
static string[] RoleGroups(IConfiguration config, RolaUzytkownika rola)
{
    var grupy = config.GetSection("Ad:Roles").GetChildren()
        .Where(c => string.Equals(c.Value, rola.ToString(), StringComparison.OrdinalIgnoreCase))
        .Select(c => c.Key)
        .ToArray();
    return grupy.Length > 0 ? grupy : new[] { rola.ToString() };
}
