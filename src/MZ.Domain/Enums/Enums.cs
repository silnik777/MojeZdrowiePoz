namespace MZ.Domain.Enums;

/// <summary>Status uczestnika w procesie programu „Moje Zdrowie" (maszyna stanów pipeline).</summary>
public enum PipelineStatus
{
    NowaAnkieta = 0,
    ZakwalifikowanyDoProgramu = 1,
    Wykluczony = 2,
    SkierowanieWystawione = 3,
    BadaniaUmowione = 4,
    BadaniaWToku = 5,
    WynikiCzesciowe = 6,
    WynikiKomplet = 7,
    WizytaPodsumowujacaUmowiona = 8,
    WizytaZrealizowana = 9,
    DoRozliczenia = 10,
    RozliczonyNFZ = 11,
    Zamkniety = 12
}

public enum Plec
{
    Kobieta = 0,
    Mezczyzna = 1
}

/// <summary>Źródło ankiety KBZOD — konsoliduje kanały papierowy i online w jednym rejestrze.</summary>
public enum ZrodloAnkiety
{
    Papierowa = 0,
    IKP_Online = 1,
    WprowadzonaRecznie = 2
}

/// <summary>Status weryfikacji odczytu OCR ankiety papierowej (OCR wymaga akceptacji człowieka).</summary>
public enum StatusWeryfikacjiOcr
{
    NieDotyczy = 0,
    OczekujeNaWeryfikacje = 1,
    Zweryfikowana = 2,
    Odrzucona = 3
}

public enum TypPakietu
{
    Podstawowy = 0,
    Rozszerzony = 1
}

public enum DecyzjaKwalifikacji
{
    Zakwalifikowany = 0,
    Niezakwalifikowany = 1
}

/// <summary>Źródło wyniku badania — pozwala śledzić sposób pozyskania (ręczny/import/integracja).</summary>
public enum ZrodloWyniku
{
    Reczny = 0,
    ImportCsv = 1,
    ImportExcel = 2,
    Hl7 = 3,
    Api = 4
}

public enum OcenaNormy
{
    Norma = 0,
    Ponizej = 1,
    Powyzej = 2,
    Nieokreslona = 3
}

public enum TypIntegracjiLaboratorium
{
    Brak = 0,
    ImportPliku = 1,
    Hl7 = 2,
    Fhir = 3,
    Api = 4
}

public enum TypDokumentu
{
    SkierowanieNaBadania = 0,
    IndywidualnyPlanZdrowotny = 1,
    Zgoda = 2,
    Inny = 3
}

/// <summary>Metoda podpisu elektronicznego dokumentu (format PAdES dla PDF).</summary>
public enum TypPodpisu
{
    Brak = 0,
    Kwalifikowany = 1,
    ProfilZaufany = 2,
    CertyfikatZUS = 3
}

public enum TypTerminu
{
    PobranieBadan = 0,
    WizytaPodsumowujaca = 1
}

public enum StatusTerminu
{
    Zaplanowany = 0,
    Zrealizowany = 1,
    Odwolany = 2,
    Nieobecnosc = 3
}

public enum TypPowiadomienia
{
    PrzypomnienieOTerminie = 0,
    KontaktTerminowy30Dni = 1,
    WynikiGotowe = 2,
    ZaproszenieNaWizyte = 3,
    Inne = 4
}

public enum StatusSms
{
    Zakolejkowany = 0,
    Wyslany = 1,
    Dostarczony = 2,
    Blad = 3
}

public enum RolaUzytkownika
{
    Rejestracja = 0,
    Pielegniarka = 1,
    Lekarz = 2,
    Administrator = 3
}

/// <summary>Rodzaj operacji rejestrowanej w logu audytu (wymóg RODO).</summary>
public enum AkcjaAudytu
{
    Odczyt = 0,
    Utworzenie = 1,
    Modyfikacja = 2,
    Usuniecie = 3,
    Eksport = 4,
    Wydruk = 5,
    Logowanie = 6
}
