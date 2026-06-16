using MZ.Application.Abstractions;
using MZ.Domain.Enums;

namespace MZ.Infrastructure.Identity;

/// <summary>
/// Domyślny fallback użytkownika (np. zadania w tle/seed). Warstwa Web nadpisuje
/// rejestrację ICurrentUser implementacją opartą o kontekst AD/HTTP.
/// </summary>
public class SystemCurrentUser : ICurrentUser
{
    public string? NazwaLogowania => "system";
    public RolaUzytkownika? Rola => RolaUzytkownika.Administrator;
}
