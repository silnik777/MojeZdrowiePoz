using System.Security.Claims;
using MZ.Application.Abstractions;
using MZ.Domain.Enums;

namespace MZ.Web.Identity;

/// <summary>
/// Bieżący użytkownik na podstawie tożsamości z kontekstu HTTP (Windows Authentication / AD).
/// Rola wyznaczana z przynależności do grup AD wg konfiguracji (sekcja "Ad:Roles").
/// W środowisku developerskim, gdy brak tożsamości Windows, używany jest użytkownik zastępczy.
/// </summary>
public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;
    private readonly AdRoleMapper _mapper;
    private readonly IHostEnvironment _env;

    public HttpContextCurrentUser(IHttpContextAccessor accessor, AdRoleMapper mapper, IHostEnvironment env)
    {
        _accessor = accessor;
        _mapper = mapper;
        _env = env;
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public string? NazwaLogowania
    {
        get
        {
            var name = Principal?.Identity?.IsAuthenticated == true ? Principal!.Identity!.Name : null;
            if (!string.IsNullOrWhiteSpace(name)) return name;
            return _env.IsDevelopment() ? "dev\\local" : null;
        }
    }

    public RolaUzytkownika? Rola
    {
        get
        {
            var p = Principal;
            if (p?.Identity?.IsAuthenticated == true)
            {
                var rola = _mapper.Map(p);
                if (rola is not null) return rola;
            }
            return _env.IsDevelopment() ? RolaUzytkownika.Administrator : null;
        }
    }
}
