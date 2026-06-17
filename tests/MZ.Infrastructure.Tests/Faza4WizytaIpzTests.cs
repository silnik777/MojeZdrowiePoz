using System.Text;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MZ.Application.Dtos;
using MZ.Domain.Enums;
using Xunit;

namespace MZ.Infrastructure.Tests;

public class Faza4WizytaIpzTests
{
    private static Stream Cda(string pesel, params string[] kody)
    {
        var sb = new StringBuilder();
        foreach (var k in kody)
            sb.Append($@"<entry><observation><code code=""{k}""/><effectiveTime value=""20260610""/>
                <value xsi:type=""PQ"" value=""1"" unit=""x""/></observation></entry>");
        var xml = $@"<ClinicalDocument xmlns=""urn:hl7-org:v3"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
            <recordTarget><patientRole><id extension=""{pesel}""/></patientRole></recordTarget>
            <component><structuredBody><component><section>{sb}</section></component></structuredBody></component>
            </ClinicalDocument>";
        return new MemoryStream(Encoding.UTF8.GetBytes(xml));
    }

    private static async Task<Guid> DoWynikowKomplet(TestowyKontekst ctx)
    {
        const string pesel = "74010112345";
        var p = await ctx.Patients.UtworzAsync(new PatientInput(
            pesel, "Jan", "Kowalski", new DateOnly(1974, 1, 1), Plec.Mezczyzna, null, null, false, null));
        var e = await ctx.Enrollments.UtworzAsync(p.Id);
        await ctx.Questionnaires.ZapiszAsync(e.Id, new QuestionnaireInput(
            ZrodloAnkiety.WprowadzonaRecznie, null, new DateOnly(2026, 6, 1), new Dictionary<string, string>(), null));
        await ctx.Qualifications.KwalifikujAsync(e.Id, DecyzjaKwalifikacji.Zakwalifikowany, null);
        await ctx.Qualifications.WystawSkierowanieAsync(e.Id, null);
        var labId = (await ctx.Db.Laboratories.FirstAsync()).Id;
        await ctx.LabImport.ImportujAsync(Cda(pesel, "MORF", "GLU", "KREA", "LIPID", "TSH", "PSA"), labId);
        return e.Id;
    }

    [Fact]
    public async Task Zapis_wizyty_liczy_bmi_i_przesuwa_status()
    {
        using var ctx = new TestowyKontekst();
        var id = await DoWynikowKomplet(ctx);

        await ctx.Visits.ZapiszWizyteAsync(id, new VisitInput(
            new DateOnly(2026, 6, 15), 120, 80, 70, WzrostCm: 180, WagaKg: 90, ObwodTaliiCm: 95, null, "Kontrola"));

        var e = await ctx.Enrollments.PobierzZeSzczegolamiAsync(id);
        e!.Status.Should().Be(PipelineStatus.WizytaZrealizowana);
        e.Wizyta!.Bmi.Should().Be(27.8);
    }

    [Fact]
    public async Task Ipz_i_generowanie_pdf_oraz_przekazanie_do_rozliczenia()
    {
        using var ctx = new TestowyKontekst();
        var id = await DoWynikowKomplet(ctx);
        await ctx.Visits.ZapiszWizyteAsync(id, new VisitInput(
            new DateOnly(2026, 6, 15), 120, 80, 70, 180, 90, 95, null, null));

        await ctx.Visits.ZapiszIpzAsync(id, new IpzInput(
            new[] { "Redukcja masy ciała" }, new[] { "Poradnictwo dietetyczne" }, "Grypa", "Kontrola za 12 mies.", null));

        var dok = await ctx.Documents.GenerujIpzAsync(id);
        var bytes = await File.ReadAllBytesAsync(dok.PlikSciezka);
        bytes.Length.Should().BeGreaterThan(500);
        Encoding.ASCII.GetString(bytes, 0, 4).Should().Be("%PDF");

        await ctx.Documents.GenerujSkierowanieAsync(id);
        (await ctx.Documents.ListaAsync(id)).Should().HaveCount(2);

        await ctx.Visits.PrzekazDoRozliczeniaAsync(id);
        var e = await ctx.Enrollments.PobierzZeSzczegolamiAsync(id);
        e!.Status.Should().Be(PipelineStatus.DoRozliczenia);
    }

    [Fact]
    public async Task Wgranie_podpisanego_oznacza_dokument_jako_podpisany()
    {
        using var ctx = new TestowyKontekst();
        var id = await DoWynikowKomplet(ctx);
        await ctx.Visits.ZapiszWizyteAsync(id, new VisitInput(new DateOnly(2026, 6, 15), null, null, null, null, null, null, null, null));
        await ctx.Visits.ZapiszIpzAsync(id, new IpzInput(new[] { "Cel" }, Array.Empty<string>(), null, null, null));
        var dok = await ctx.Documents.GenerujIpzAsync(id);

        using var podpisany = new MemoryStream(Encoding.ASCII.GetBytes("%PDF-signed"));
        await ctx.Documents.WgrajPodpisanyAsync(dok.Id, podpisany, TypPodpisu.ProfilZaufany);

        var po = await ctx.Documents.PobierzAsync(dok.Id);
        po!.Podpisany.Should().BeTrue();
        po.TypPodpisu.Should().Be(TypPodpisu.ProfilZaufany);
    }
}
