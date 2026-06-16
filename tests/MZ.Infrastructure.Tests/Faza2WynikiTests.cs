using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MZ.Application.Dtos;
using MZ.Domain.Enums;
using MZ.Infrastructure.Lab;
using Xunit;

namespace MZ.Infrastructure.Tests;

public class Faza2WynikiTests
{
    private static string Cda(string pesel, params (string kod, string val, string unit)[] obs)
    {
        var sb = new StringBuilder();
        foreach (var (kod, val, unit) in obs)
            sb.Append($@"<entry><observation classCode=""OBS"" moodCode=""EVN"">
                <code code=""{kod}"" codeSystem=""marcel""/>
                <effectiveTime value=""20260605""/>
                <value xsi:type=""PQ"" value=""{val}"" unit=""{unit}""/>
                </observation></entry>");
        return $@"<?xml version=""1.0"" encoding=""utf-8""?>
            <ClinicalDocument xmlns=""urn:hl7-org:v3"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
              <id extension=""ZLEC-1""/>
              <recordTarget><patientRole>
                <id root=""2.16.840.1.113883.3.4424.1.1.616"" extension=""{pesel}""/>
              </patientRole></recordTarget>
              <component><structuredBody><component><section>{sb}</section></component></structuredBody></component>
            </ClinicalDocument>";
    }

    private static Stream Strumien(string xml) => new MemoryStream(Encoding.UTF8.GetBytes(xml));

    private static async Task<(Guid enrollmentId, string pesel)> PrzygotujZakwalifikowany(TestowyKontekst ctx)
    {
        const string pesel = "74010112345";
        var p = await ctx.Patients.UtworzAsync(new PatientInput(
            pesel, "Jan", "Kowalski", new DateOnly(1974, 1, 1),
            Plec.Mezczyzna, null, null, false, null));
        var e = await ctx.Enrollments.UtworzAsync(p.Id);
        await ctx.Questionnaires.ZapiszAsync(e.Id, new QuestionnaireInput(
            ZrodloAnkiety.WprowadzonaRecznie, null, new DateOnly(2026, 6, 1),
            new Dictionary<string, string>(), null));
        await ctx.Qualifications.KwalifikujAsync(e.Id, DecyzjaKwalifikacji.Zakwalifikowany, null);
        await ctx.Qualifications.WystawSkierowanieAsync(e.Id, null);
        return (e.Id, pesel);
    }

    [Fact]
    public async Task Adapter_parsuje_pesel_i_wyniki_z_cda()
    {
        var xml = Cda("74010112345", ("MORF", "4.5", "10^6/ul"), ("GLU", "95", "mg/dl"));
        var dokumenty = await new ELaboratCdaAdapter().WczytajAsync(Strumien(xml));

        dokumenty.Should().HaveCount(1);
        dokumenty[0].Pesel.Should().Be("74010112345");
        dokumenty[0].Wyniki.Should().HaveCount(2);
        dokumenty[0].Wyniki.First(w => w.KodLab == "GLU").WartoscLiczbowa.Should().Be(95);
    }

    [Fact]
    public async Task Import_czesciowy_oznacza_braki_a_komplet_je_usuwa()
    {
        using var ctx = new TestowyKontekst();
        var (enrollmentId, pesel) = await PrzygotujZakwalifikowany(ctx);

        // Import bez PSA -> braki (mężczyzna 52 ma w pakiecie PSA).
        var czesciowy = Cda(pesel,
            ("MORF", "4.5", "10^6/ul"), ("GLU", "120", "mg/dl"),
            ("KREA", "1.0", "mg/dl"), ("LIPID", "1", ""), ("TSH", "2.0", "mIU/l"));
        var podsumowanie = await ctx.LabImport.ImportujAsync(Strumien(czesciowy), await LabId(ctx));

        podsumowanie.Dopasowanych.Should().Be(1);
        podsumowanie.ZapisanychWynikow.Should().Be(5);

        var poCzesci = await ctx.Enrollments.PobierzZeSzczegolamiAsync(enrollmentId);
        poCzesci!.Status.Should().Be(PipelineStatus.WynikiCzesciowe);
        poCzesci.MaBrakiBadan.Should().BeTrue();

        var braki = await ctx.LabResults.RaportBrakowAsync();
        braki.Should().ContainSingle()
            .Which.BrakujaceBadania.Should().Contain("PSA");

        // GLU 120 powyżej normy (70–99).
        poCzesci.Wyniki.First(w => w.TestDefinition.Kod == "GLU").OcenaNormy.Should().Be(OcenaNormy.Powyzej);

        // Dograj brakujące PSA -> komplet.
        await ctx.LabImport.ImportujAsync(Strumien(Cda(pesel, ("PSA", "1.5", "ng/ml"))), await LabId(ctx));

        var poKomplecie = await ctx.Enrollments.PobierzZeSzczegolamiAsync(enrollmentId);
        poKomplecie!.Status.Should().Be(PipelineStatus.WynikiKomplet);
        poKomplecie.MaBrakiBadan.Should().BeFalse();
        (await ctx.LabResults.RaportBrakowAsync()).Should().BeEmpty();
    }

    [Fact]
    public async Task Wynik_dla_nieznanego_pesel_jest_niedopasowany()
    {
        using var ctx = new TestowyKontekst();
        await PrzygotujZakwalifikowany(ctx);
        var podsumowanie = await ctx.LabImport.ImportujAsync(
            Strumien(Cda("99999999999", ("MORF", "4.5", ""))), await LabId(ctx));

        podsumowanie.Dopasowanych.Should().Be(0);
        podsumowanie.Niedopasowanych.Should().Be(1);
    }

    private static async Task<Guid> LabId(TestowyKontekst ctx) =>
        (await ctx.Db.Laboratories.FirstAsync()).Id;
}
