using MZ.Domain.Common;
using MZ.Domain.Enums;

namespace MZ.Domain.Entities;

/// <summary>Ankieta KBZOD (Kwestionariusz Bilansu Zdrowia Osoby Dorosłej).</summary>
public class Questionnaire : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public ProgramEnrollment Enrollment { get; set; } = null!;

    public ZrodloAnkiety Zrodlo { get; set; }
    public DateOnly? DataWypelnienia { get; set; }
    public DateOnly DataWplywu { get; set; }

    /// <summary>Ścieżka/identyfikator wgranego skanu PDF (dla ankiet papierowych).</summary>
    public string? SkanPdfSciezka { get; set; }

    /// <summary>Surowy tekst odczytany przez OCR oraz miara pewności (0–1).</summary>
    public string? OcrTekst { get; set; }
    public double? OcrPewnosc { get; set; }
    public StatusWeryfikacjiOcr StatusWeryfikacji { get; set; } = StatusWeryfikacjiOcr.NieDotyczy;

    /// <summary>Automatycznie wyliczony wynik punktowy ankiety.</summary>
    public int? Score { get; set; }
    public Guid? ScoringRuleSetId { get; set; }
    public ScoringRuleSet? ScoringRuleSet { get; set; }

    public string? Notatki { get; set; }

    public ICollection<QuestionnaireAnswer> Odpowiedzi { get; set; } = new List<QuestionnaireAnswer>();
}

/// <summary>Znormalizowana odpowiedź na pytanie ankiety — podstawa scoringu i doboru pakietu.</summary>
public class QuestionnaireAnswer : BaseEntity
{
    public Guid QuestionnaireId { get; set; }
    public Questionnaire Questionnaire { get; set; } = null!;

    public string KodPytania { get; set; } = string.Empty;
    public string? Wartosc { get; set; }
}

/// <summary>Konfigurowalny, wersjonowany zestaw reguł punktacji ankiety.</summary>
public class ScoringRuleSet : BaseEntity
{
    public string Nazwa { get; set; } = string.Empty;
    public bool Aktywny { get; set; } = true;
    public DateOnly ObowiazujeOd { get; set; }
    public DateOnly? ObowiazujeDo { get; set; }

    public ICollection<ScoringRule> Reguly { get; set; } = new List<ScoringRule>();
}

/// <summary>Pojedyncza reguła punktacji: dla danego pytania i wartości przyznaje punkty/czynnik ryzyka.</summary>
public class ScoringRule : BaseEntity
{
    public Guid ScoringRuleSetId { get; set; }
    public ScoringRuleSet ScoringRuleSet { get; set; } = null!;

    public string KodPytania { get; set; } = string.Empty;
    /// <summary>Oczekiwana wartość odpowiedzi (np. "TAK"); null = dowolna niepusta.</summary>
    public string? WartoscDopasowania { get; set; }
    public int Punkty { get; set; }
    /// <summary>Opcjonalny czynnik ryzyka wyzwalany przez tę regułę (np. "PALENIE").</summary>
    public string? CzynnikRyzyka { get; set; }
}
