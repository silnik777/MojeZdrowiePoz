using MZ.Application.Dtos;
using MZ.Domain.Entities;

namespace MZ.Application.Services;

/// <summary>Wylicza wynik punktowy ankiety i czynniki ryzyka wg konfigurowalnego zestawu reguł.</summary>
public class ScoringService
{
    public ScoringResult Oblicz(Questionnaire ankieta, ScoringRuleSet zestaw)
    {
        var odpowiedzi = ankieta.Odpowiedzi
            .GroupBy(o => o.KodPytania, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.Last().Wartosc, StringComparer.OrdinalIgnoreCase);

        var score = 0;
        var czynniki = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var regula in zestaw.Reguly)
        {
            if (!odpowiedzi.TryGetValue(regula.KodPytania, out var wartosc)) continue;
            if (string.IsNullOrWhiteSpace(wartosc)) continue;

            var dopasowanie = regula.WartoscDopasowania is null
                || string.Equals(regula.WartoscDopasowania, wartosc, StringComparison.OrdinalIgnoreCase);
            if (!dopasowanie) continue;

            score += regula.Punkty;
            if (!string.IsNullOrWhiteSpace(regula.CzynnikRyzyka))
                czynniki.Add(regula.CzynnikRyzyka);
        }

        return new ScoringResult(score, czynniki.ToArray());
    }
}
