# Cleanifico – Architektur

## Status und technische Basis

Repository-Root:

```text
/Users/torstenfergens/Documents/Projekte/FergenixLabs/Cleanifico
```

Die Solution verwendet das moderne Format `Cleanifico.slnx`. Alle Projekte zielen auf `net10.0`. Verwendet wurde .NET SDK 10.0.102; `global.json` fordert die SDK-Familie 10.0.100 mit `latestPatch`-Roll-forward an. .NET 10 wurde als aktuelle langfristig geeignete Basis gewählt. Nullable Reference Types und implizite Usings sind in allen Projekten aktiv.

## Solution-Struktur

| Projekt | Verantwortung | Aktueller Inhalt |
| --- | --- | --- |
| `Cleanifico.Domain` | Entities, Value Objects, Enums, Domain-Regeln und Domain Exceptions | `CleaningType`, `TimeType`, `Customer`, `CleaningObject` und `Employee` mit UTC-Lifecycle |
| `Cleanifico.Application` | Use Cases, Interfaces, Validierung und Orchestrierung | Reinigungstyp-, Zeittyp-, Kunden-, Objekt-, Mitarbeiter- und Benutzerverwaltungs-Ports |
| `Cleanifico.Contracts` | DTOs, Requests, Responses und öffentliche Verträge | Cleaning-Type-, Identity- und Benutzerverträge sowie Security-Konstanten |
| `Cleanifico.Infrastructure` | EF Core, MySQL, Identity, Repositories und technische Adapter | `CleanificoDbContext`, Identity-Services, Fluent Mapping und Migrationen |
| `Cleanifico.Api` | ASP.NET Core API und serverseitiger Composition Root | Identity-Cookie, Policies, Problem Details, Health-, Cleaning-Type- und Benutzer-Endpunkte |
| `Cleanifico.Web` | Blazor-Oberfläche für Cleanifico Office | Login, geschützte Office-Hülle, Reinigungstypen und Benutzerverwaltung |
| `*.Tests` | Unit-, Architektur- und Integrationstests | Domain, Application, EF-Mapping, Referenzgraph und echte HTTP-Routen |

`Cleanifico.Mobile` ist als spätere .NET-MAUI-App vorgesehen, gehört aber noch nicht zur Solution.

## Abhängigkeitsrichtung

```text
Cleanifico.Domain               Cleanifico.Contracts
        ^                         ^           ^
        |                         |           |
        +---- Cleanifico.Application          |
                    ^                         |
                    |                         |
          Cleanifico.Infrastructure           |
                    ^                         |
                    |                         |
                    +----- Cleanifico.Api ----+
                                              |
                                   Cleanifico.Web
```

Konkret:

- Domain: keine Projektabhängigkeiten.
- Contracts: keine Projektabhängigkeiten.
- Application: Domain und Contracts.
- Infrastructure: Domain und Application.
- API: Application, Contracts und Infrastructure.
- Web: ausschließlich Contracts.

Die Application-Schicht benötigt keine konkrete Infrastructure-Implementierung. API und Web sind getrennte Hosts; es besteht keine Projektabhängigkeit zwischen ihnen.

## Laufzeit- und Tenant-Architektur

Geplant ist eine isolierte Instanz pro lizenziertem Gebäudereinigungsbetrieb:

```text
FergensHub / Discovery
          |
          +-- Tenant A: Cleanifico API + MySQL A
          |
          +-- Tenant B: Cleanifico API + MySQL B
```

Jeder Tenant erhält eine eigene API-/Instanz, Konfiguration, Lizenz und MySQL-Datenbank. Eine zentrale gemeinsame Cleanifico-Fachdatenbank ist ausgeschlossen. Da die Datenbank selbst die Isolationsgrenze bildet, tragen Business-Entities wie `CleaningType` derzeit keine zusätzliche `TenantId`. Lizenzseitig identifiziert eine persistente Installation-ID genau diese Instanz; die Zuordnung zum FergensHub-Kunden erfolgt bei Aktivierung über den Lizenzschlüssel.

## Datenbank und Persistenz

`Cleanifico.Infrastructure` verwendet EF Core 9.0.19 mit dem stabilen Pomelo-Provider 9.0.0 und einem expliziten MySQL-8.4-Serverprofil. Pomelo 9 ist der derzeit stabile Providerzweig; deshalb bleibt auch EF Core auf der kompatiblen 9.0-Patchlinie, obwohl die Hosts `net10.0` verwenden. Ein späterer gemeinsamer Wechsel auf EF Core/Pomelo 10 erfolgt erst mit einer stabilen kompatiblen Providerfreigabe.

`CleanificoDbContext` ist der zentrale Context. Entity-Mappings liegen in separaten `IEntityTypeConfiguration<T>`-Klassen und werden aus der Infrastructure-Assembly geladen. `CleaningType` und `TimeType` besitzen eindeutige, case-insensitive Indizes für Name und Code. `Customer` besitzt eine eindeutige, case-insensitive Kundennummer. `CleaningObject` besitzt eine eindeutige, case-insensitive Objektnummer und einen verpflichtenden Customer-Fremdschlüssel mit `Restrict`. Lookup-Indizes unterstützen Status-, Kunden- und Ortssuchen. Technische Zeitstempel werden als `datetime(6)` in UTC geführt.

Der Laufzeit-Connection-String kommt aus `ConnectionStrings:Cleanifico`; echte Zugangsdaten liegen nicht im Repository. Die API schlägt beim Start mit einer neutralen Konfigurationsmeldung fehl, wenn der Wert fehlt. Die Design-Time-Factory verwendet für reine Modellerzeugung eine nicht funktionsfähige Platzhalterverbindung.

Schemaänderungen liegen versioniert unter `Persistence/Migrations`. Auf `InitialCleanificoPersistence`, `AddTenantIdentity`, `AddConfigurableTimeTypes`, `AddCustomers` und `AddCleaningObjects` folgt `AddEmployees`. Der Host führt weder `EnsureCreated` noch `Database.Migrate` beim Start aus. Lokale und spätere produktive Tenant-Migrationen werden explizit und kontrolliert ausgeführt.

Die Persistenzabstraktion gehört in Application, die EF-Implementierung in Infrastructure:

```text
Cleanifico.Web
      |
      | HTTP + Contracts
      v
Cleanifico.Api
      |
      v
Cleanifico.Application
      |
      | ICleaningTypeRepository
      v
Cleanifico.Infrastructure
      |
      v
EF Core / Pomelo / MySQL
```

MySQL-Fehler für eindeutige Indizes und spätere Fremdschlüssel werden in verständliche fachnahe Konflikte übersetzt. Eine vollständige Datenbank-Integrationstestumgebung existiert lokal noch nicht; die aktuelle Infrastructure-Suite prüft das reale Pomelo-EF-Modell, aber öffnet bewusst keine normale Entwicklungsdatenbank.

## API und Web-App

`Cleanifico.Api` ist der serverseitige Composition Root. Sie registriert Problem Details, zentrale Exception-Behandlung, Health Checks, Application Services und die Infrastructure-Adapter. Neben `GET /health` stellt sie Stammdaten-Routen unter `/api/cleaning-types`, `/api/time-types`, `/api/customers`, `/api/objects` und `/api/employees` bereit. Domainvalidierung führt zu `400`, fehlende Datensätze zu `404` und Eindeutigkeits-/Löschkonflikte zu `409`; unerwartete Fehler werden geloggt, aber ohne interne Details ausgeliefert.

`Cleanifico.Web` ist eine eigenständige Blazor-Web-App mit serverseitiger Interaktivität. Sie referenziert nur öffentliche Contracts und ruft die API über typisierte HTTP-Clients auf. Die Seiten `/reinigungstypen`, `/zeittypen`, `/kunden`, `/objekte` und `/mitarbeiter` bieten Suche, Statusfilter, Listen, Dialoge zum Anlegen/Bearbeiten und klar getrennte Lifecycle-Aktionen. Die Objektseite filtert zusätzlich nach Kunde; die Kundendetailansicht zeigt Anzahl und Direktlinks der zugeordneten Objekte. Backendfehler werden als sichere deutsche Meldungen dargestellt; rohe Antworttexte oder Stacktraces werden nicht gezeigt.

## Mitarbeiter und Benutzerkonten

`Employee` ist die fachliche Personal-Entity des tenantlokalen Reinigungsunternehmens. Sie existiert unabhängig von `ApplicationUser`; weder ein Mitarbeiter ohne Login noch ein Benutzerkonto ohne Personalstammsatz ist technisch fehlerhaft. Eine optionale Verknüpfung wird erst mit konkreten App-Zugangsanforderungen ergänzt und nicht automatisch erzeugt.

`EmployeeNumber` ist ein benutzerverwalteter, case-insensitive eindeutiger Geschäftsschlüssel. Beschäftigungsarten sind bewusst kein Enum, sondern ein optionaler frei pflegbarer Text, damit kundenspezifische Bezeichnungen ohne Schemaänderung möglich bleiben. Datumsregeln und nichtnegative Wochen-/Monatssollstunden liegen in der Domain; eine automatische Stundenberechnung existiert nicht. Deaktivierung setzt kein Austrittsdatum.

Ohne fachliche Referenzen darf ein Employee physisch gelöscht werden. Sobald Verträge, Arbeitszeiten, Objektzuweisungen, Einsätze, Schlüssel oder historische Daten hinzukommen, müssen Restrict-Fremdschlüssel das Löschen verhindern; dann ist Deaktivierung der reguläre Lifecycle. Es werden keine künstlichen Referenzen auf diese späteren Module angelegt.

## Kunden und Objekte

`Customer` ist der tenantlokale Auftraggeber. `CustomerNumber` ist ein benutzerverwalteter, case-insensitive eindeutiger Geschäftsschlüssel; die technische ID bleibt der Primärschlüssel. Ansprechpartner und Verwaltungsadresse liegen bis zu konkreten Mehrfachkontakt-Anforderungen direkt am Customer. `CleaningObject` gehört genau einem Customer, besitzt eine eigene Einsatz-/Reinigungsadresse und einen optionalen direkten Kontakt. `ObjectNumber` ist tenantlokal case-insensitive eindeutig und änderbar.

Customer zu CleaningObject ist eine verpflichtende 1:n-Beziehung ohne Cascade Delete. Besitzt ein Customer mindestens ein CleaningObject, weist der Application Service den Löschversuch verständlich zurück und der Datenbank-Fremdschlüssel schützt zusätzlich; Deaktivierung ist dann der reguläre Lifecycle. Es werden keine künstlichen Referenzen auf noch nicht vorhandene Module eingeführt.

CleaningObjects dürfen derzeit physisch gelöscht werden, solange keine weiteren fachlichen Referenzen existieren. Sobald Verträge, Einsätze, Arbeitszeiten, Qualitätskontrollen oder historische Daten ein Objekt referenzieren, schützen Fremdschlüssel das Objekt und Deaktivierung wird der reguläre Lifecycle.

## Zeittypen und historische Stabilität

`TimeType` ist tenantlokaler, frei konfigurierbarer Stammdatensatz und ausdrücklich kein Enum. Die sieben Startwerte sind normale Kundendaten ohne `IsSystem`, `IsBuiltIn`, `IsLocked` oder Sonderbehandlung. Ein technischer Initialisierungsmarker wird atomar mit den Startwerten gespeichert. Nach gesetztem Marker führt jeder weitere Start keinerlei Seed-Änderung aus – auch dann nicht, wenn Werte umbenannt, Codes geändert, deaktiviert oder gelöscht wurden.

Bis Zeitbuchungen existieren, darf ein Zeittyp physisch gelöscht werden. Spätere `TimeEntry`-Datensätze müssen für historische Stabilität mindestens `TimeTypeId`, `TimeTypeNameSnapshot`, `CountsAsWorkTimeSnapshot`, `IsPaidSnapshot`, `RequiresObjectSnapshot` und `IsAbsenceSnapshot` speichern. Änderungen am aktuellen Zeittyp dürfen alte Buchungen nicht rückwirkend umdeuten. Ein `TimeEntry`-Modul ist noch nicht implementiert.

## Authentifizierung und Autorisierung

ASP.NET Core Identity verwendet `ApplicationUser`, `IdentityRole<Guid>` und denselben `CleanificoDbContext` wie die tenantlokalen Fachdaten. Eine zusätzliche `TenantId` ist wegen der Instanz-/Datenbank-Isolation nicht nötig. `ApplicationUser` führt Vorname, Nachname, eindeutige E-Mail als Benutzername, Aktivstatus und UTC-Auditzeiten. Ein Konto ist kein fachlicher Mitarbeiterdatensatz.

Die Rollen sind `Owner`, `Administrator`, `Dispatcher`, `ObjectManager` und `Employee`. Rollen und Policy-Namen liegen zentral in Contracts. Die API erzwingt zusätzlich zu den Rollen eine Active-User-Anforderung als Fallback-Policy. Fachliche Lesezugriffe einschließlich `ViewEmployees` erlauben die vier Office-Rollen, Schreibzugriffe einschließlich `ManageEmployees` nur Owner und Administrator. Benutzerverwaltung und Rollenvergabe sind ebenfalls Owner/Administrator vorbehalten. Der letzte aktive Owner ist gegen Deaktivierung und Rollenentzug geschützt.

API und Web sind getrennte Hosts und teilen den ASP.NET-Identity-Anwendungscookie über denselben Scheme-/Cookie-Namen, Application Name und persistenten Data-Protection-Schlüsselbund. Der Cookie ist `HttpOnly`, `Secure`, `SameSite=Lax`, acht Stunden gültig und gleitend erneuert. Die Web-BFF-Endpunkte vermitteln Login und Logout; typisierte Server-Clients reichen eine kurzlebig geschützte Sitzung an die API weiter. Die API validiert den Security Stamp bei jeder Anfrage, die Web-App prüft die Sitzung fail-closed gegen `/api/auth/session`. Anonyme API-Aufrufe liefern `401`, fehlende Rollen `403`.

Passwörter erfordern mindestens zwölf Zeichen sowie Groß-/Kleinbuchstaben, Zahl und Sonderzeichen. Fünf Fehlversuche sperren das Konto für 15 Minuten. Öffentliche Registrierung, MFA und Passwort-Reset sind nicht Teil des aktuellen Schnitts. Rollen werden idempotent gestartet; ein erster Owner wird nur bei expliziter Bootstrap-Konfiguration mit externem Initialpasswort erzeugt. Produktiv müssen beide Hosts denselben zugriffsgeschützten und at-rest verschlüsselten Keyring verwenden. Für eine spätere Mobile-App ist ein eigener Bearer-/Token-Flow zu entwerfen; der Office-Cookie wird nicht vorweggenommen.

## FergensHub-Lizenzierung

AssetFico identifiziert jede isolierte Kundeninstallation durch eine zufällig erzeugte, dauerhaft im lokalen License State gespeicherte Installation-ID. Ein `flk1_`-Lizenzschlüssel aktiviert die Installation über `POST api/licensing/v1/activate`; FergensHub antwortet mit einem geheimen `flr1_`-Refresh-Credential und einer signierten Lease. Erneuerungen verwenden `POST api/licensing/v1/refresh`. Der JSON-State wird atomar, ohne Symlinks und unter Unix nur mit Benutzer-Lese-/Schreibrechten gespeichert. Die Lease ist an Installation-ID und Produktcode gebunden, trägt kanonische Featurecodes und wird mit `ECDSA-P256-SHA256` gegen einen eingebetteten öffentlichen Trust Anchor verifiziert. Sie gilt 30 Tage regulär und anschließend 14 Tage in Grace. Numerical Limits sind im AssetFico-Lease-Vertrag nicht vorhanden.

Cleanifico übernimmt dieses Muster mit Produktcode `CLEANIFICO`, Feature `base`, denselben Credentialformaten, Routen, Lease-Schema-, Signatur- und Zeitregeln. `ILicenseService` bleibt die einzige Policy-Grenze und liefert `NotActivated`, `Valid`, `Grace`, `Expired` oder `Invalid`. Der API-Host erneuert die Lease beim Start und danach standardmäßig alle 24 Stunden. Ein temporärer FergensHub-Ausfall lässt eine bereits verifizierte Lease bis zum Ende von Grace weiterarbeiten; fachliche Ablehnung wie Widerruf oder Sperrung blockiert sofort. State-Pfad, FergensHub-Basis-URL, Timeouts und Intervall kommen aus `Licensing`-Konfiguration beziehungsweise Environment Variables/Secrets. Lizenz- und Refresh-Credentials werden weder ausgegeben noch protokolliert.

Das analysierte FergensHub-Repository besitzt weiterhin nur die interne Zuordnung `Tenant → TenantProduct → TenantProductFeature` und implementiert weder die von AssetFico bereits konsumierten Runtime-Routen noch Lizenzschlüssel, Installationen, Refresh-Credentials oder Lease-Signierung. Für den produktiven Cleanifico-Betrieb muss FergensHub daher die bestehenden AssetFico-Verträge implementieren, `CLEANIFICO` samt `base` verwalten und installationsgebundene Leases mit dem bestehenden Signing-Key ausstellen. Cleanifico erfindet keinen abweichenden Endpoint und enthält keinen Konfigurations-Bypass.

API-Business-Policies für CleaningTypes, TimeTypes, Customers, CleaningObjects und Employees enthalten zusätzlich `LicensedProductRequirement`. Anonyme Aufrufe bleiben `401`; bei gültiger Lizenz und fehlender Rolle bleibt `403`. Ungültige oder nicht prüfbare Lizenzen liefern ein kontrolliertes `403 ProblemDetails` ohne interne URL oder Exception. `/api/license/status` ist für aktive Benutzer lizenzunabhängig erreichbar. `/health`, Login, Logout, Sessionprüfung und Benutzeradministration werden nicht durch die Geschäftslizenz blockiert.

Cleanifico Office spiegelt dieselbe zusätzliche Policy-Anforderung für `/reinigungstypen`, `/zeittypen`, `/kunden` und `/objekte`. Der Status wird über die authentifizierte Cleanifico API bezogen und pro Server-Circuit eine Minute gecacht. `/lizenz` zeigt Lease- und Installationsstatus; Owner/Administrator dürfen dort aktivieren und manuell erneuern. Rollenprüfungen bleiben vollständig bestehen.

## Discovery

Die geplante Discovery API soll später `Firmencode + Produkt` in Tenant-ID, Firmenname, API-Basis-URL und API-Version auflösen. Dies ist insbesondere für `Cleanifico.Mobile` vorgesehen. AssetFico verwendet Discovery nicht für die Lizenzprüfung; deshalb bleibt Discovery von der hier übernommenen installationsgebundenen Lease getrennt.

## Mobile, Storage und Hintergrunddienste

- `Cleanifico.Mobile`: späteres .NET-MAUI-Projekt; Offline-Synchronisierung und SQLite gehören nicht zu Prompt 001.
- Storage: noch kein Anbieter oder Vertrag festgelegt; Dokumente und Fotos werden erst mit dem jeweiligen Fachmodul geplant.
- Hintergrunddienste: der API-Host erneuert die lokale Lizenz-Lease periodisch; weitere Jobs werden nur für konkrete Anwendungsfälle ergänzt.

## Tests

xUnit prüft derzeit:

- Domain-Invarianten, Normalisierung und Cleaning-Type-Lifecycle,
- TimeType-Invarianten, vollständige Änderbarkeit, Lifecycle und Standarddaten-Idempotenz ohne Reset,
- Customer- und CleaningObject-Pflichtfelder, Normalisierung, Eindeutigkeit, Suche, Status, Lifecycle und EF-Mapping einschließlich Restrict-FK,
- Application-Orchestrierung, Eindeutigkeit, Suche, Filter und Sortierung,
- das tatsächliche EF-/Pomelo-Modell einschließlich Feldlängen und Indizes,
- Identity-Benutzer, Passwort-Hashing, Login, Inaktivität, Lockout, Rollen, Owner-Schutz und sicheren Bootstrap,
- Cleaning-Type-, TimeType-, Customer- und CleaningObject-HTTP-Operationen sowie rollenabhängige `401`-/`403`-Fälle über einen lokalen Kestrel-Host,
- Login- und Routenschutz der eigenständigen Web-App,
- gültige, Grace-, nicht aktivierte, abgelaufene und ungültige Lizenzzustände, Lease-Signatur und Installationsbindung, die Kombination mit `401`/`403`, den lizenzunabhängigen Healthcheck und die Office-Lizenzanzeige,
- die Abhängigkeitsfreiheit der Domain,
- die exakten Projekt-Referenzen von Application, Infrastructure, API und Web,
- die Vollständigkeit der elf Solution-Projekte,
- den laufenden API-Health-Endpunkt.

Die HTTP-Tests tauschen nur den Repository-Port am Composition Root aus und berühren niemals eine Entwicklungs- oder produktive MySQL-Datenbank. Echte MySQL-Integrationstests bleiben offen, bis eine zuverlässig isolierte Testdatenbank bereitsteht.
