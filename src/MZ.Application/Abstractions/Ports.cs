using MZ.Domain.Enums;
using MZ.Application.Dtos;

namespace MZ.Application.Abstractions;

/// <summary>
/// Port źródła wyników badań. Pozwala startować na imporcie pliku/wpisaniu ręcznym,
/// a integrację HL7/API dołączyć później bez zmiany rdzenia (wzorzec adapter).
/// </summary>
public interface ILabResultSource
{
    TypIntegracjiLaboratorium Typ { get; }
    Task<IReadOnlyList<TestResultDto>> WczytajWynikiAsync(Stream zrodlo, Guid laboratoriumId, CancellationToken ct = default);
}

/// <summary>Port bramki SMS — wysyłka powiadomień do pacjentów.</summary>
public interface ISmsGateway
{
    Task<SmsWynikDto> WyslijAsync(string numerTelefonu, string tresc, CancellationToken ct = default);
}

/// <summary>Port OCR — odczyt tekstu ze skanu PDF ankiety (przetwarzanie lokalne, RODO).</summary>
public interface IOcrService
{
    Task<OcrWynikDto> OdczytajAsync(Stream pdf, CancellationToken ct = default);
}

/// <summary>Port podpisu elektronicznego dokumentu PDF (PAdES).</summary>
public interface IDocumentSigner
{
    TypPodpisu Typ { get; }
    Task<byte[]> PodpiszAsync(byte[] pdf, CancellationToken ct = default);
}

/// <summary>Port audytu RODO — rejestracja dostępu/zmian danych pacjenta.</summary>
public interface IAuditService
{
    Task ZapiszAsync(AkcjaAudytu akcja, string encja, string? encjaId = null, Guid? patientId = null,
        string? szczegolyJson = null, CancellationToken ct = default);
}

/// <summary>Informacje o bieżącym użytkowniku (z kontekstu AD).</summary>
public interface ICurrentUser
{
    string? NazwaLogowania { get; }
    RolaUzytkownika? Rola { get; }
}
