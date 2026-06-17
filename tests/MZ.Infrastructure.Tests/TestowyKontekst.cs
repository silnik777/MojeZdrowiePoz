using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MZ.Application.Abstractions;
using MZ.Application.Services;
using MZ.Domain.Enums;
using MZ.Infrastructure.Lab;
using MZ.Infrastructure.Ocr;
using MZ.Infrastructure.Persistence;
using MZ.Infrastructure.Services;

namespace MZ.Infrastructure.Tests;

/// <summary>Buduje izolowany DbContext na SQLite in-memory wraz z zaseedowanymi słownikami i serwisami.</summary>
public sealed class TestowyKontekst : IDisposable
{
    private readonly SqliteConnection _conn;
    public MzDbContext Db { get; }

    public PatientService Patients { get; }
    public EnrollmentService Enrollments { get; }
    public QuestionnaireService Questionnaires { get; }
    public QualificationService Qualifications { get; }
    public LabResultImportService LabImport { get; }
    public LabResultService LabResults { get; }
    public AppointmentService Appointments { get; }
    public SmsService Sms { get; }
    public NotificationService Notifications { get; }
    public FakeSmsGateway Gateway { get; }

    public TestowyKontekst()
    {
        _conn = new SqliteConnection("DataSource=:memory:");
        _conn.Open();
        var options = new DbContextOptionsBuilder<MzDbContext>().UseSqlite(_conn).Options;
        Db = new MzDbContext(options);
        Db.Database.EnsureCreated();
        DbSeeder.SeedAsync(Db).GetAwaiter().GetResult();

        var audit = new NoopAudit();
        var time = TimeProvider.System;
        var scoring = new ScoringService();
        var packages = new PackageSelectionService();
        var completeness = new CompletenessService();
        var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();

        Patients = new PatientService(Db, audit);
        Enrollments = new EnrollmentService(Db, audit, time);
        Questionnaires = new QuestionnaireService(Db, scoring, new StubOcrService(), audit, config);
        Qualifications = new QualificationService(Db, scoring, packages, Enrollments, audit, time);
        LabImport = new LabResultImportService(Db, new ELaboratCdaAdapter(), completeness, Enrollments, audit);
        LabResults = new LabResultService(Db, completeness, LabImport, audit);
        Appointments = new AppointmentService(Db, Enrollments, audit, time);
        Gateway = new FakeSmsGateway();
        Sms = new SmsService(Db, Gateway, audit, time, config);
        Notifications = new NotificationService(Db, time);
    }

    public void Dispose()
    {
        Db.Dispose();
        _conn.Dispose();
    }

    private sealed class NoopAudit : IAuditService
    {
        public Task ZapiszAsync(AkcjaAudytu akcja, string encja, string? encjaId = null, Guid? patientId = null,
            string? szczegolyJson = null, CancellationToken ct = default) => Task.CompletedTask;
    }
}

/// <summary>Atrapa bramki SMS — zapamiętuje wysłane wiadomości, zawsze zwraca sukces.</summary>
public sealed class FakeSmsGateway : ISmsGateway
{
    public List<(string Numer, string Tresc)> Wyslane { get; } = new();

    public Task<MZ.Application.Dtos.SmsWynikDto> WyslijAsync(string numerTelefonu, string tresc, CancellationToken ct = default)
    {
        Wyslane.Add((numerTelefonu, tresc));
        return Task.FromResult(new MZ.Application.Dtos.SmsWynikDto(true, "fake-1", null));
    }
}
