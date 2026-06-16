# Moje Zdrowie POZ

Narzędzie wspierające prowadzenie i rozliczanie programu profilaktycznego NFZ
**„Moje Zdrowie – bilans zdrowia osoby dorosłej"** w poradni POZ.

Aplikacja wspomaga (nie zastępuje mMedica) proces programu: konsoliduje ankiety
(papierowe i z IKP), prowadzi pacjenta przez kolejne etapy (pipeline), kontroluje
kompletność zleconych badań, wspiera terminarz i powiadomienia SMS, wizytę
podsumowującą z IPZ oraz przygotowanie danych do rozliczenia z NFZ.

## Architektura

Warstwowa solucja .NET 8:

| Projekt | Rola |
|---|---|
| `src/MZ.Domain` | Encje, enumy, maszyna stanów pipeline, reguły domenowe |
| `src/MZ.Application` | Serwisy (scoring, dobór pakietów, kontrola kompletności), porty/adaptery |
| `src/MZ.Infrastructure` | EF Core (`MzDbContext`), interceptor audytu, seed, migracje |
| `src/MZ.Web` | Aplikacja Blazor Server (UI), uwierzytelnianie Windows/AD |
| `tests/*` | Testy jednostkowe (xUnit + FluentAssertions) |

### Stack docelowy (produkcja)
- ASP.NET Core 8 + Blazor Server, hostowany na **IIS** na lokalnym serwerze.
- Dostęp przez **RDS** w domenie **Active Directory**; logowanie kontami domenowymi
  (Windows Authentication), role mapowane z grup AD (sekcja `Ad:Roles` w `appsettings.json`).
- Baza **SQL Server Express** (migracje EF Core).
- Dane pozostają w placówce (RODO); audyt dostępu w tabeli `AuditLogs`.

## Uruchomienie deweloperskie

W środowisku developerskim aplikacja używa **SQLite** (tworzona automatycznie, seed słowników)
i użytkownika zastępczego, więc działa bez serwera Windows/AD:

```bash
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/MZ.Web --urls http://127.0.0.1:5080
```

Strony: `/` (start) oraz `/pipeline` (stan procesu).

## Testy

```bash
dotnet test
```

## Konfiguracja produkcyjna
- `appsettings.json` → `Database:Provider = SqlServer`, `ConnectionStrings:MzDb`.
- `Ad:Roles` → mapowanie grup AD na role: `Rejestracja`, `Pielegniarka`, `Lekarz`, `Administrator`.
- Migracje stosowane automatycznie przy starcie (`Database.Migrate()`); dla SQLite używany jest `EnsureCreated`.

## Status (Faza 0 — fundament)
Zrealizowano: struktura solucji i warstw, model domenowy, `MzDbContext` + migracja inicjalna,
interceptor audytu, seed słowników (definicje badań, przykładowe pakiety wg wieku/płci,
przykładowy zestaw reguł scoringu), uwierzytelnianie Windows/AD + role, szkielet UI
(start, pipeline), serwisy: scoring, dobór pakietu, kontrola kompletności badań, oraz testy.

Kolejne fazy (ankiety + OCR + auto-score, kontrola kompletności w UI, terminarz + SMS,
wizyta + IPZ + e-podpis, rozliczenie SWIAD, integracja P1) opisano w planie projektu.
