using Microsoft.EntityFrameworkCore;
using MZ.Application.Abstractions;
using MZ.Application.Dtos;
using MZ.Domain.Entities;
using MZ.Domain.Enums;
using MZ.Infrastructure.Persistence;

namespace MZ.Infrastructure.Services;

/// <summary>Operacje na pacjentach (rejestr uczestników programu).</summary>
public class PatientService
{
    private readonly MzDbContext _db;
    private readonly IAuditService _audit;

    public PatientService(MzDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<List<Patient>> ListaAsync(string? szukaj = null, CancellationToken ct = default)
    {
        var q = _db.Patients.AsNoTracking().Where(p => p.Aktywny);
        if (!string.IsNullOrWhiteSpace(szukaj))
        {
            szukaj = szukaj.Trim();
            q = q.Where(p => p.Nazwisko.Contains(szukaj) || p.Imie.Contains(szukaj) || p.Pesel.Contains(szukaj));
        }
        return await q.OrderBy(p => p.Nazwisko).ThenBy(p => p.Imie).Take(500).ToListAsync(ct);
    }

    public Task<Patient?> PobierzAsync(Guid id, CancellationToken ct = default) =>
        _db.Patients.FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<Patient> UtworzAsync(PatientInput input, CancellationToken ct = default)
    {
        var p = new Patient();
        Przepisz(p, input);
        _db.Patients.Add(p);
        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Utworzenie, nameof(Patient), p.Id.ToString(), p.Id, ct: ct);
        return p;
    }

    public async Task AktualizujAsync(Guid id, PatientInput input, CancellationToken ct = default)
    {
        var p = await _db.Patients.FirstOrDefaultAsync(x => x.Id == id, ct)
                ?? throw new InvalidOperationException("Nie znaleziono pacjenta.");
        Przepisz(p, input);
        await _db.SaveChangesAsync(ct);
        await _audit.ZapiszAsync(AkcjaAudytu.Modyfikacja, nameof(Patient), p.Id.ToString(), p.Id, ct: ct);
    }

    private static void Przepisz(Patient p, PatientInput i)
    {
        p.Pesel = i.Pesel.Trim();
        p.Imie = i.Imie.Trim();
        p.Nazwisko = i.Nazwisko.Trim();
        p.DataUrodzenia = i.DataUrodzenia;
        p.Plec = i.Plec;
        p.Telefon = string.IsNullOrWhiteSpace(i.Telefon) ? null : i.Telefon.Trim();
        p.Email = string.IsNullOrWhiteSpace(i.Email) ? null : i.Email.Trim();
        p.ZgodaSms = i.ZgodaSms;
        p.DataOstatniegoBilansu = i.DataOstatniegoBilansu;
    }
}
