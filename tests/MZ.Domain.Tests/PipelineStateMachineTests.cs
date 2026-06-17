using FluentAssertions;
using MZ.Domain.Enums;
using MZ.Domain.Services;
using Xunit;

namespace MZ.Domain.Tests;

public class PipelineStateMachineTests
{
    [Theory]
    [InlineData(PipelineStatus.NowaAnkieta, PipelineStatus.ZakwalifikowanyDoProgramu)]
    [InlineData(PipelineStatus.ZakwalifikowanyDoProgramu, PipelineStatus.SkierowanieWystawione)]
    [InlineData(PipelineStatus.BadaniaWToku, PipelineStatus.WynikiCzesciowe)]
    [InlineData(PipelineStatus.DoRozliczenia, PipelineStatus.RozliczonyNFZ)]
    public void Dozwolone_przejscia_sa_akceptowane(PipelineStatus z, PipelineStatus na) =>
        PipelineStateMachine.CzyMoznaPrzejsc(z, na).Should().BeTrue();

    [Theory]
    [InlineData(PipelineStatus.NowaAnkieta, PipelineStatus.RozliczonyNFZ)]
    [InlineData(PipelineStatus.WynikiKomplet, PipelineStatus.NowaAnkieta)]
    public void Niedozwolone_przejscia_sa_odrzucane(PipelineStatus z, PipelineStatus na) =>
        PipelineStateMachine.CzyMoznaPrzejsc(z, na).Should().BeFalse();

    [Theory]
    [InlineData(PipelineStatus.Zamkniety)]
    [InlineData(PipelineStatus.Wykluczony)]
    public void Stany_terminalne_nie_maja_nastepnikow(PipelineStatus status)
    {
        PipelineStateMachine.JestTerminalny(status).Should().BeTrue();
        PipelineStateMachine.MozliweNastepne(status).Should().BeEmpty();
    }
}
