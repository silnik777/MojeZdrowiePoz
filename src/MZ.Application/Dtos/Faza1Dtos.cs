using MZ.Domain.Enums;

namespace MZ.Application.Dtos;

/// <summary>Dane wejściowe do utworzenia/edycji pacjenta.</summary>
public record PatientInput(
    string Pesel,
    string Imie,
    string Nazwisko,
    DateOnly DataUrodzenia,
    Plec Plec,
    string? Telefon,
    string? Email,
    bool ZgodaSms,
    DateOnly? DataOstatniegoBilansu);

/// <summary>Dane wejściowe zapisu ankiety KBZOD (wpis ręczny/IKP).</summary>
public record QuestionnaireInput(
    ZrodloAnkiety Zrodlo,
    DateOnly? DataWypelnienia,
    DateOnly DataWplywu,
    IReadOnlyDictionary<string, string> Odpowiedzi,
    string? Notatki);

/// <summary>Wynik zapisu ankiety: wyliczony score i wykryte czynniki ryzyka.</summary>
public record QuestionnaireSaveResult(int Score, IReadOnlyCollection<string> CzynnikiRyzyka);
