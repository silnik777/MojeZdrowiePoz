using FluentAssertions;
using MZ.Application.Dtos;
using MZ.Domain.Enums;
using Xunit;

namespace MZ.Infrastructure.Tests;

public class Faza3TerminySmsTests
{
    private static async Task<Guid> Zakwalifikowany(TestowyKontekst ctx, bool zgodaSms, string? telefon)
    {
        var p = await ctx.Patients.UtworzAsync(new PatientInput(
            "74010112345", "Jan", "Kowalski", new DateOnly(1974, 1, 1),
            Plec.Mezczyzna, telefon, null, zgodaSms, null));
        var e = await ctx.Enrollments.UtworzAsync(p.Id);
        await ctx.Questionnaires.ZapiszAsync(e.Id, new QuestionnaireInput(
            ZrodloAnkiety.WprowadzonaRecznie, null, new DateOnly(2026, 6, 1),
            new Dictionary<string, string>(), null));
        await ctx.Qualifications.KwalifikujAsync(e.Id, DecyzjaKwalifikacji.Zakwalifikowany, null);
        await ctx.Qualifications.WystawSkierowanieAsync(e.Id, null);
        return e.Id;
    }

    [Fact]
    public async Task Umowienie_pobrania_przesuwa_status_i_tworzy_przypomnienie()
    {
        using var ctx = new TestowyKontekst();
        var id = await Zakwalifikowany(ctx, zgodaSms: true, telefon: "500100200");

        var termin = await ctx.Appointments.UmowAsync(id, TypTerminu.PobranieBadan,
            new DateTimeOffset(2026, 7, 1, 9, 0, 0, TimeSpan.Zero), godzinPrzed: 24);

        var e = await ctx.Enrollments.PobierzZeSzczegolamiAsync(id);
        e!.Status.Should().Be(PipelineStatus.BadaniaUmowione);
        e.Terminy.Should().ContainSingle();

        ctx.Db.Reminders.Should().ContainSingle(r => r.AppointmentId == termin.Id);
    }

    [Fact]
    public async Task Sms_wysylany_przy_zgodzie_a_tresc_jest_neutralna()
    {
        using var ctx = new TestowyKontekst();
        var id = await Zakwalifikowany(ctx, zgodaSms: true, telefon: "500100200");

        var proba = await ctx.Sms.WyslijDoPacjentaAsync(id, TypPowiadomienia.WynikiGotowe);

        proba.Wyslano.Should().BeTrue();
        ctx.Gateway.Wyslane.Should().ContainSingle();
        var tresc = ctx.Gateway.Wyslane[0].Tresc;
        tresc.Should().NotContain("Kowalski").And.NotContain("74010112345");
        ctx.Db.SmsMessages.Should().ContainSingle(s => s.Status == StatusSms.Wyslany);
    }

    [Fact]
    public async Task Sms_nie_wysylany_bez_zgody()
    {
        using var ctx = new TestowyKontekst();
        var id = await Zakwalifikowany(ctx, zgodaSms: false, telefon: "500100200");

        var proba = await ctx.Sms.WyslijDoPacjentaAsync(id, TypPowiadomienia.WynikiGotowe);

        proba.Wyslano.Should().BeFalse();
        ctx.Gateway.Wyslane.Should().BeEmpty();
    }

    [Fact]
    public async Task DoKontaktu_zawiera_zakwalifikowany_przypadek_z_terminem()
    {
        using var ctx = new TestowyKontekst();
        var id = await Zakwalifikowany(ctx, zgodaSms: true, telefon: "500100200");

        var lista = await ctx.Notifications.DoKontaktuAsync();

        lista.Should().ContainSingle(x => x.EnrollmentId == id);
        lista[0].TerminKontaktu.Should().Be(new DateOnly(2026, 6, 1).AddDays(30));
    }
}
