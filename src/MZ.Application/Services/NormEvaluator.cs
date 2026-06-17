using MZ.Domain.Enums;

namespace MZ.Application.Services;

/// <summary>Ocena wyniku liczbowego względem zakresu referencyjnego badania.</summary>
public static class NormEvaluator
{
    public static OcenaNormy Ocen(decimal? wartosc, decimal? normaMin, decimal? normaMax)
    {
        if (wartosc is null) return OcenaNormy.Nieokreslona;
        if (normaMin is null && normaMax is null) return OcenaNormy.Nieokreslona;
        if (normaMin is not null && wartosc < normaMin) return OcenaNormy.Ponizej;
        if (normaMax is not null && wartosc > normaMax) return OcenaNormy.Powyzej;
        return OcenaNormy.Norma;
    }
}
