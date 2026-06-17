using System.Security.Claims;
using Microsoft.Extensions.Options;
using MZ.Domain.Enums;

namespace MZ.Web.Identity;

/// <summary>Konfiguracja mapowania grup AD na role aplikacji (sekcja "Ad:Roles" w appsettings).</summary>
public class AdOptions
{
    public Dictionary<string, string> Roles { get; set; } = new();
}

/// <summary>Wyznacza rolę aplikacji na podstawie członkostwa w grupach AD.</summary>
public class AdRoleMapper
{
    private readonly AdOptions _options;

    public AdRoleMapper(IOptions<AdOptions> options) => _options = options.Value;

    public RolaUzytkownika? Map(ClaimsPrincipal principal)
    {
        // Kolejność priorytetu: Administrator > Lekarz > Pielegniarka > Rejestracja.
        foreach (var rola in new[]
                 {
                     RolaUzytkownika.Administrator, RolaUzytkownika.Lekarz,
                     RolaUzytkownika.Pielegniarka, RolaUzytkownika.Rejestracja
                 })
        {
            var grupa = _options.Roles.FirstOrDefault(kv =>
                string.Equals(kv.Value, rola.ToString(), StringComparison.OrdinalIgnoreCase)).Key;
            if (!string.IsNullOrWhiteSpace(grupa) && principal.IsInRole(grupa))
                return rola;
        }
        return null;
    }
}
