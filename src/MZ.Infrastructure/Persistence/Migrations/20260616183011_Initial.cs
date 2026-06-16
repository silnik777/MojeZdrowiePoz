using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MZ.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UzytkownikId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NazwaLogowania = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataCzas = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Akcja = table.Column<int>(type: "int", nullable: false),
                    Encja = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EncjaId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Stacja = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SzczegolyJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Laboratories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nazwa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Kontakt = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypIntegracji = table.Column<int>(type: "int", nullable: false),
                    KonfiguracjaJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Aktywny = table.Column<bool>(type: "bit", nullable: false),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Laboratories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Pesel = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Imie = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nazwisko = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataUrodzenia = table.Column<DateOnly>(type: "date", nullable: false),
                    Plec = table.Column<int>(type: "int", nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZgodaSms = table.Column<bool>(type: "bit", nullable: false),
                    DataOstatniegoBilansu = table.Column<DateOnly>(type: "date", nullable: true),
                    Aktywny = table.Column<bool>(type: "bit", nullable: false),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScoringRuleSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nazwa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Aktywny = table.Column<bool>(type: "bit", nullable: false),
                    ObowiazujeOd = table.Column<DateOnly>(type: "date", nullable: false),
                    ObowiazujeDo = table.Column<DateOnly>(type: "date", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringRuleSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SmsMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReminderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    NumerTelefonu = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Tresc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataWyslania = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IdentyfikatorBramki = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BladOpis = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SmsMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Kod = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nazwa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Jednostka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NormaMin = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    NormaMax = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Aktywny = table.Column<bool>(type: "bit", nullable: false),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestDefinitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TestPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nazwa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    Aktywny = table.Column<bool>(type: "bit", nullable: false),
                    ObowiazujeOd = table.Column<DateOnly>(type: "date", nullable: false),
                    ObowiazujeDo = table.Column<DateOnly>(type: "date", nullable: true),
                    WiekMin = table.Column<int>(type: "int", nullable: false),
                    WiekMax = table.Column<int>(type: "int", nullable: false),
                    Plec = table.Column<int>(type: "int", nullable: true),
                    WymaganyCzynnikRyzyka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestPackages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NazwaLogowania = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ImieNazwisko = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rola = table.Column<int>(type: "int", nullable: false),
                    Aktywny = table.Column<bool>(type: "bit", nullable: false),
                    OstatnieLogowanie = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScoringRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScoringRuleSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodPytania = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WartoscDopasowania = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Punkty = table.Column<int>(type: "int", nullable: false),
                    CzynnikRyzyka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoringRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoringRules_ScoringRuleSets_ScoringRuleSetId",
                        column: x => x.ScoringRuleSetId,
                        principalTable: "ScoringRuleSets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LabTestMappings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LaboratoriumId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodLab = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TestDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabTestMappings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabTestMappings_Laboratories_LaboratoriumId",
                        column: x => x.LaboratoriumId,
                        principalTable: "Laboratories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabTestMappings_TestDefinitions_TestDefinitionId",
                        column: x => x.TestDefinitionId,
                        principalTable: "TestDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PackageItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestPackageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Wymagane = table.Column<bool>(type: "bit", nullable: false),
                    Kolejnosc = table.Column<int>(type: "int", nullable: false),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PackageItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PackageItems_TestDefinitions_TestDefinitionId",
                        column: x => x.TestDefinitionId,
                        principalTable: "TestDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PackageItems_TestPackages_TestPackageId",
                        column: x => x.TestPackageId,
                        principalTable: "TestPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Enrollments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DataRozpoczecia = table.Column<DateOnly>(type: "date", nullable: false),
                    DataKwalifikacji = table.Column<DateOnly>(type: "date", nullable: true),
                    TerminKontaktu = table.Column<DateOnly>(type: "date", nullable: true),
                    IdAnkietyKbzod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PrzypisanyPakietId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KoordynatorId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Uwagi = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaBrakiBadan = table.Column<bool>(type: "bit", nullable: false),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Enrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Enrollments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Enrollments_TestPackages_PrzypisanyPakietId",
                        column: x => x.PrzypisanyPakietId,
                        principalTable: "TestPackages",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Enrollments_Users_KoordynatorId",
                        column: x => x.KoordynatorId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    DataGodzina = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LaboratoriumId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PersonelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Appointments_Laboratories_LaboratoriumId",
                        column: x => x.LaboratoriumId,
                        principalTable: "Laboratories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Appointments_Users_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PipelineHistory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StatusZ = table.Column<int>(type: "int", nullable: true),
                    StatusNa = table.Column<int>(type: "int", nullable: false),
                    DataPrzejscia = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Komentarz = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PipelineHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PipelineHistory_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Qualifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UzytkownikId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Decyzja = table.Column<int>(type: "int", nullable: false),
                    DataDecyzji = table.Column<DateOnly>(type: "date", nullable: false),
                    CzynnikiRyzyka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Uzasadnienie = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Qualifications_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Qualifications_Users_UzytkownikId",
                        column: x => x.UzytkownikId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Questionnaires",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Zrodlo = table.Column<int>(type: "int", nullable: false),
                    DataWypelnienia = table.Column<DateOnly>(type: "date", nullable: true),
                    DataWplywu = table.Column<DateOnly>(type: "date", nullable: false),
                    SkanPdfSciezka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OcrTekst = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OcrPewnosc = table.Column<double>(type: "float", nullable: true),
                    StatusWeryfikacji = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    ScoringRuleSetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Notatki = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Questionnaires", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Questionnaires_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Questionnaires_ScoringRuleSets_ScoringRuleSetId",
                        column: x => x.ScoringRuleSetId,
                        principalTable: "ScoringRuleSets",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Referrals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataWystawienia = table.Column<DateOnly>(type: "date", nullable: false),
                    LekarzId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    LaboratoriumId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Referrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Referrals_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Referrals_Laboratories_LaboratoriumId",
                        column: x => x.LaboratoriumId,
                        principalTable: "Laboratories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Referrals_Users_LekarzId",
                        column: x => x.LekarzId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SignedDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    PlikSciezka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypPodpisu = table.Column<int>(type: "int", nullable: false),
                    PodpisujacyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DataPodpisu = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignedDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SignedDocuments_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SignedDocuments_Users_PodpisujacyId",
                        column: x => x.PodpisujacyId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DataWizyty = table.Column<DateOnly>(type: "date", nullable: false),
                    PersonelId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CisnienieSkurczowe = table.Column<int>(type: "int", nullable: true),
                    CisnienieRozkurczowe = table.Column<int>(type: "int", nullable: true),
                    Tetno = table.Column<int>(type: "int", nullable: true),
                    WzrostCm = table.Column<double>(type: "float", nullable: true),
                    WagaKg = table.Column<double>(type: "float", nullable: true),
                    ObwodTaliiCm = table.Column<double>(type: "float", nullable: true),
                    PodsumowanieWynikow = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Zalecenia = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Visits_Users_PersonelId",
                        column: x => x.PersonelId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Reminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Typ = table.Column<int>(type: "int", nullable: false),
                    DataZaplanowana = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Wyslane = table.Column<bool>(type: "bit", nullable: false),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reminders_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireAnswers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionnaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KodPytania = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Wartosc = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAnswers_Questionnaires_QuestionnaireId",
                        column: x => x.QuestionnaireId,
                        principalTable: "Questionnaires",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderedTests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReferralId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TestDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Wymagane = table.Column<bool>(type: "bit", nullable: false),
                    DataZlecenia = table.Column<DateOnly>(type: "date", nullable: false),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderedTests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderedTests_Referrals_ReferralId",
                        column: x => x.ReferralId,
                        principalTable: "Referrals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderedTests_TestDefinitions_TestDefinitionId",
                        column: x => x.TestDefinitionId,
                        principalTable: "TestDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HealthPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Cele = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Interwencje = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ZaleceniaSzczepien = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DalszaDiagnostyka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataUtworzenia = table.Column<DateOnly>(type: "date", nullable: false),
                    DataPrzegladu = table.Column<DateOnly>(type: "date", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HealthPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HealthPlans_Visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "Visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TestResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EnrollmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderedTestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TestDefinitionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    WartoscLiczbowa = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    WartoscTekstowa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Jednostka = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DataPobrania = table.Column<DateOnly>(type: "date", nullable: true),
                    DataWyniku = table.Column<DateOnly>(type: "date", nullable: true),
                    OcenaNormy = table.Column<int>(type: "int", nullable: false),
                    Zrodlo = table.Column<int>(type: "int", nullable: false),
                    LaboratoriumId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SurowyRekord = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UtworzonoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UtworzonoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ZmodyfikowanoUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ZmodyfikowanoPrzez = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TestResults_Enrollments_EnrollmentId",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TestResults_Laboratories_LaboratoriumId",
                        column: x => x.LaboratoriumId,
                        principalTable: "Laboratories",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_TestResults_OrderedTests_OrderedTestId",
                        column: x => x.OrderedTestId,
                        principalTable: "OrderedTests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TestResults_TestDefinitions_TestDefinitionId",
                        column: x => x.TestDefinitionId,
                        principalTable: "TestDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_EnrollmentId",
                table: "Appointments",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_LaboratoriumId",
                table: "Appointments",
                column: "LaboratoriumId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PersonelId",
                table: "Appointments",
                column: "PersonelId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_KoordynatorId",
                table: "Enrollments",
                column: "KoordynatorId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_PatientId",
                table: "Enrollments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollments_PrzypisanyPakietId",
                table: "Enrollments",
                column: "PrzypisanyPakietId");

            migrationBuilder.CreateIndex(
                name: "IX_HealthPlans_VisitId",
                table: "HealthPlans",
                column: "VisitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabTestMappings_LaboratoriumId_KodLab",
                table: "LabTestMappings",
                columns: new[] { "LaboratoriumId", "KodLab" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabTestMappings_TestDefinitionId",
                table: "LabTestMappings",
                column: "TestDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderedTests_ReferralId",
                table: "OrderedTests",
                column: "ReferralId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderedTests_TestDefinitionId",
                table: "OrderedTests",
                column: "TestDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_TestDefinitionId",
                table: "PackageItems",
                column: "TestDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_PackageItems_TestPackageId",
                table: "PackageItems",
                column: "TestPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Pesel",
                table: "Patients",
                column: "Pesel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PipelineHistory_EnrollmentId",
                table: "PipelineHistory",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Qualifications_EnrollmentId",
                table: "Qualifications",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Qualifications_UzytkownikId",
                table: "Qualifications",
                column: "UzytkownikId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAnswers_QuestionnaireId",
                table: "QuestionnaireAnswers",
                column: "QuestionnaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Questionnaires_EnrollmentId",
                table: "Questionnaires",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questionnaires_ScoringRuleSetId",
                table: "Questionnaires",
                column: "ScoringRuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_EnrollmentId",
                table: "Referrals",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_LaboratoriumId",
                table: "Referrals",
                column: "LaboratoriumId");

            migrationBuilder.CreateIndex(
                name: "IX_Referrals_LekarzId",
                table: "Referrals",
                column: "LekarzId");

            migrationBuilder.CreateIndex(
                name: "IX_Reminders_AppointmentId",
                table: "Reminders",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringRules_ScoringRuleSetId",
                table: "ScoringRules",
                column: "ScoringRuleSetId");

            migrationBuilder.CreateIndex(
                name: "IX_SignedDocuments_EnrollmentId",
                table: "SignedDocuments",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SignedDocuments_PodpisujacyId",
                table: "SignedDocuments",
                column: "PodpisujacyId");

            migrationBuilder.CreateIndex(
                name: "IX_TestDefinitions_Kod",
                table: "TestDefinitions",
                column: "Kod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_EnrollmentId",
                table: "TestResults",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_LaboratoriumId",
                table: "TestResults",
                column: "LaboratoriumId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_OrderedTestId",
                table: "TestResults",
                column: "OrderedTestId");

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_TestDefinitionId",
                table: "TestResults",
                column: "TestDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_NazwaLogowania",
                table: "Users",
                column: "NazwaLogowania",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_EnrollmentId",
                table: "Visits",
                column: "EnrollmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_PersonelId",
                table: "Visits",
                column: "PersonelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "HealthPlans");

            migrationBuilder.DropTable(
                name: "LabTestMappings");

            migrationBuilder.DropTable(
                name: "PackageItems");

            migrationBuilder.DropTable(
                name: "PipelineHistory");

            migrationBuilder.DropTable(
                name: "Qualifications");

            migrationBuilder.DropTable(
                name: "QuestionnaireAnswers");

            migrationBuilder.DropTable(
                name: "Reminders");

            migrationBuilder.DropTable(
                name: "ScoringRules");

            migrationBuilder.DropTable(
                name: "SignedDocuments");

            migrationBuilder.DropTable(
                name: "SmsMessages");

            migrationBuilder.DropTable(
                name: "TestResults");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "Questionnaires");

            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "OrderedTests");

            migrationBuilder.DropTable(
                name: "ScoringRuleSets");

            migrationBuilder.DropTable(
                name: "Referrals");

            migrationBuilder.DropTable(
                name: "TestDefinitions");

            migrationBuilder.DropTable(
                name: "Enrollments");

            migrationBuilder.DropTable(
                name: "Laboratories");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "TestPackages");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
