using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MZ.Application.Abstractions;
using MZ.Application.Dtos;

namespace MZ.Infrastructure.Sms;

/// <summary>
/// Adapter bramki SMS opartej o aplikację „SMS Gate" (sms-gate.app) na Androidzie w trybie lokalnym.
/// Wysyła przez lokalny HTTP API telefonu: POST {BaseUrl}/message (Basic Auth).
/// Konfiguracja: sekcja "SmsGate" (BaseUrl, Username, Password). Numery idą tylko do operatora.
/// </summary>
public class SmsGateGateway : ISmsGateway
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<SmsGateGateway> _log;

    public SmsGateGateway(IHttpClientFactory httpFactory, IConfiguration config, ILogger<SmsGateGateway> log)
    {
        _httpFactory = httpFactory;
        _config = config;
        _log = log;
    }

    public async Task<SmsWynikDto> WyslijAsync(string numerTelefonu, string tresc, CancellationToken ct = default)
    {
        var baseUrl = _config.GetValue<string>("SmsGate:BaseUrl");
        if (string.IsNullOrWhiteSpace(baseUrl))
            return new SmsWynikDto(false, null, "Bramka SMS nieskonfigurowana (SmsGate:BaseUrl).");

        var user = _config.GetValue<string>("SmsGate:Username") ?? "";
        var pass = _config.GetValue<string>("SmsGate:Password") ?? "";

        try
        {
            var client = _httpFactory.CreateClient(nameof(SmsGateGateway));
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            var auth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{pass}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", auth);

            var payload = new { textMessage = new { text = tresc }, phoneNumbers = new[] { numerTelefonu } };
            using var resp = await client.PostAsJsonAsync("message", payload, ct);
            var tresc2 = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
                return new SmsWynikDto(false, null, $"HTTP {(int)resp.StatusCode}: {tresc2}");

            string? id = null;
            try { id = JsonDocument.Parse(tresc2).RootElement.TryGetProperty("id", out var p) ? p.GetString() : null; }
            catch { /* odpowiedź bez JSON id — pomijamy */ }

            return new SmsWynikDto(true, id, null);
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "SMS Gate: błąd wysyłki.");
            return new SmsWynikDto(false, null, ex.Message);
        }
    }
}
