using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MZ.Domain.Entities;

namespace MZ.Infrastructure.Persistence;

public class MzDbContext : DbContext
{
    public MzDbContext(DbContextOptions<MzDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<ProgramEnrollment> Enrollments => Set<ProgramEnrollment>();
    public DbSet<PipelineHistory> PipelineHistory => Set<PipelineHistory>();
    public DbSet<Questionnaire> Questionnaires => Set<Questionnaire>();
    public DbSet<QuestionnaireAnswer> QuestionnaireAnswers => Set<QuestionnaireAnswer>();
    public DbSet<ScoringRuleSet> ScoringRuleSets => Set<ScoringRuleSet>();
    public DbSet<ScoringRule> ScoringRules => Set<ScoringRule>();
    public DbSet<Qualification> Qualifications => Set<Qualification>();
    public DbSet<TestDefinition> TestDefinitions => Set<TestDefinition>();
    public DbSet<TestPackage> TestPackages => Set<TestPackage>();
    public DbSet<PackageItem> PackageItems => Set<PackageItem>();
    public DbSet<Referral> Referrals => Set<Referral>();
    public DbSet<OrderedTest> OrderedTests => Set<OrderedTest>();
    public DbSet<TestResult> TestResults => Set<TestResult>();
    public DbSet<Laboratory> Laboratories => Set<Laboratory>();
    public DbSet<LabTestMapping> LabTestMappings => Set<LabTestMapping>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<IndividualHealthPlan> HealthPlans => Set<IndividualHealthPlan>();
    public DbSet<SignedDocument> SignedDocuments => Set<SignedDocument>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Reminder> Reminders => Set<Reminder>();
    public DbSet<SmsMessage> SmsMessages => Set<SmsMessage>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Konwersja List<string> -> JSON (czynniki ryzyka, cele/interwencje IPZ).
        var listConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var listComparer = new ValueComparer<List<string>>(
            (a, c) => (a ?? new()).SequenceEqual(c ?? new()),
            v => v == null ? 0 : v.Aggregate(0, (h, s) => HashCode.Combine(h, s.GetHashCode())),
            v => v.ToList());

        b.Entity<Qualification>()
            .Property(q => q.CzynnikiRyzyka)
            .HasConversion(listConverter, listComparer);

        b.Entity<IndividualHealthPlan>(e =>
        {
            e.Property(p => p.Cele).HasConversion(listConverter, listComparer);
            e.Property(p => p.Interwencje).HasConversion(listConverter, listComparer);
        });

        // Unikalne klucze naturalne.
        b.Entity<Patient>().HasIndex(p => p.Pesel).IsUnique();
        b.Entity<AppUser>().HasIndex(u => u.NazwaLogowania).IsUnique();
        b.Entity<TestDefinition>().HasIndex(t => t.Kod).IsUnique();
        b.Entity<LabTestMapping>().HasIndex(m => new { m.LaboratoriumId, m.KodLab }).IsUnique();

        // Relacje 1–1 wokół przypadku.
        b.Entity<ProgramEnrollment>()
            .HasOne(e => e.Ankieta).WithOne(q => q.Enrollment)
            .HasForeignKey<Questionnaire>(q => q.EnrollmentId);
        b.Entity<ProgramEnrollment>()
            .HasOne(e => e.Kwalifikacja).WithOne(q => q.Enrollment)
            .HasForeignKey<Qualification>(q => q.EnrollmentId);
        b.Entity<ProgramEnrollment>()
            .HasOne(e => e.Skierowanie).WithOne(r => r.Enrollment)
            .HasForeignKey<Referral>(r => r.EnrollmentId);
        b.Entity<ProgramEnrollment>()
            .HasOne(e => e.Wizyta).WithOne(v => v.Enrollment)
            .HasForeignKey<Visit>(v => v.EnrollmentId);
        b.Entity<Visit>()
            .HasOne(v => v.Ipz).WithOne(i => i.Visit)
            .HasForeignKey<IndividualHealthPlan>(i => i.VisitId);

        b.Entity<TestResult>()
            .HasOne(r => r.OrderedTest).WithMany()
            .HasForeignKey(r => r.OrderedTestId).OnDelete(DeleteBehavior.SetNull);

        // Precyzja wartości liczbowych badań.
        b.Entity<TestResult>().Property(r => r.WartoscLiczbowa).HasPrecision(18, 4);
        b.Entity<TestDefinition>().Property(r => r.NormaMin).HasPrecision(18, 4);
        b.Entity<TestDefinition>().Property(r => r.NormaMax).HasPrecision(18, 4);
    }
}
