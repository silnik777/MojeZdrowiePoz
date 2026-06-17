using MZ.Application.Dtos;
using MZ.Domain.Entities;

namespace MZ.Application.Services;

/// <summary>
/// Rdzeń wartości narzędzia: porównuje badania ZLECONE z faktycznie WYKONANYMI
/// i wyznacza braki. Adresuje problem niewykonywania części badań przez laboratorium.
/// </summary>
public class CompletenessService
{
    public CompletenessResult Sprawdz(
        IEnumerable<OrderedTest> zlecone,
        IEnumerable<TestResult> wyniki)
    {
        var zleconeList = zlecone as ICollection<OrderedTest> ?? zlecone.ToArray();
        var wykonaneDef = wyniki.Select(w => w.TestDefinitionId).ToHashSet();
        var zleconeDef = zleconeList.Select(z => z.TestDefinitionId).ToHashSet();

        var brakujaceWymagane = zleconeList
            .Where(z => z.Wymagane && !wykonaneDef.Contains(z.TestDefinitionId))
            .Select(z => z.TestDefinitionId)
            .Distinct()
            .ToArray();

        var brakujaceOpcjonalne = zleconeList
            .Where(z => !z.Wymagane && !wykonaneDef.Contains(z.TestDefinitionId))
            .Select(z => z.TestDefinitionId)
            .Distinct()
            .ToArray();

        var nadmiarowe = wyniki
            .Where(w => !zleconeDef.Contains(w.TestDefinitionId))
            .Select(w => w.TestDefinitionId)
            .Distinct()
            .ToArray();

        return new CompletenessResult(brakujaceWymagane, brakujaceOpcjonalne, nadmiarowe);
    }
}
