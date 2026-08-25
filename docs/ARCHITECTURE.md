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
| `Cleanifico.Domain` | Entities, Value Objects, Enums, Domain-Regeln und Domain Exceptions | `CleaningType` mit Invarianten und UTC-Lifecycle |
| `Cleanifico.Application` | Use Cases, Interfaces, Validierung und Orchestrierung | Reinigungstyp-Service und Repository-Port |
| `Cleanifico.Contracts` | DTOs, Requests, Responses und öffentliche Verträge | getrennte Cleaning-Type-Requests und -Responses |
| `Cleanifico.Infrastructure` | EF Core, MySQL, spätere Identity-Persistenz, Repositories und externe Adapter | `CleanificoDbContext`, Fluent Mapping, Repository und Migrationen |
| `Cleanifico.Api` | ASP.NET Core API und serverseitiger Composition Root | Problem Details, Health Check und Cleaning-Type-Endpunkte |
| `Cleanifico.Web` | Blazor-Oberfläche für Cleanifico Office | Office-Hülle, typisierter API-Client und `/reinigungstypen` |
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

Jeder Tenant erhält eine eigene Tenant-ID, API-/Instanz, Konfiguration, Lizenz und MySQL-Datenbank. Eine zentrale gemeinsame Cleanifico-Fachdatenbank ist ausgeschlossen. Da die Datenbank selbst die Isolationsgrenze bildet, tragen Business-Entities wie `CleaningType` derzeit keine zusätzliche `TenantId`. Die konkrete Deployment- und Secret-Verteilungsstrategie wird festgelegt, sobald wiederverwendbare FergensHub-/Assetfico-Patterns verfügbar und geprüft sind.

## Datenbank und Persistenz

`Cleanifico.Infrastructure` verwendet EF Core 9.0.19 mit dem stabilen Pomelo-Provider 9.0.0 und einem expliziten MySQL-8.4-Serverprofil. Pomelo 9 ist der derzeit stabile Providerzweig; deshalb bleibt auch EF Core auf der kompatiblen 9.0-Patchlinie, obwohl die Hosts `net10.0` verwenden. Ein späterer gemeinsamer Wechsel auf EF Core/Pomelo 10 erfolgt erst mit einer stabilen kompatiblen Providerfreigabe.

`CleanificoDbContext` ist der zentrale Context. Entity-Mappings liegen in separaten `IEntityTypeConfiguration<T>`-Klassen und werden aus der Infrastructure-Assembly geladen. `CleaningType` besitzt eindeutige, case-insensitive Indizes für Name und Code sowie einen Listenindex über Status, Sortierung und Name. Technische Zeitstempel werden als `datetime(6)` in UTC geführt.

Der Laufzeit-Connection-String kommt aus `ConnectionStrings:Cleanifico`; echte Zugangsdaten liegen nicht im Repository. Die API schlägt beim Start mit einer neutralen Konfigurationsmeldung fehl, wenn der Wert fehlt. Die Design-Time-Factory verwendet für reine Modellerzeugung eine nicht funktionsfähige Platzhalterverbindung.

Schemaänderungen liegen versioniert unter `Persistence/Migrations`. Die erste Migration ist `InitialCleanificoPersistence`. Der Host führt weder `EnsureCreated` noch `Database.Migrate` beim Start aus. Lokale und spätere produktive Tenant-Migrationen werden explizit und kontrolliert ausgeführt.

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

`Cleanifico.Api` ist der serverseitige Composition Root. Sie registriert Problem Details, zentrale Exception-Behandlung, Health Checks, Application Services und die Infrastructure-Adapter. Neben `GET /health` stellt sie die Cleaning-Type-Routen unter `/api/cleaning-types` bereit. Domainvalidierung führt zu `400`, fehlende Datensätze zu `404` und Eindeutigkeits-/Löschkonflikte zu `409`; unerwartete Fehler werden geloggt, aber ohne interne Details ausgeliefert.

`Cleanifico.Web` ist eine eigenständige Blazor-Web-App mit serverseitiger Interaktivität. Sie referenziert nur öffentliche Contracts und ruft die API über einen typisierten HTTP-Client auf. Die Seite `/reinigungstypen` bietet Suche, Statusfilter, Standardliste, Dialoge zum Anlegen/Bearbeiten und klar getrennte Aktionen zum Deaktivieren, Reaktivieren und endgültigen Löschen. Backendfehler werden als sichere deutsche Meldungen dargestellt; rohe Antworttexte oder Stacktraces werden nicht gezeigt.

## Authentifizierung und Autorisierung

Noch nicht implementiert. Die Cleaning-Type-Endpunkte sind daher aktuell nicht geschützt und dürfen in diesem Zustand nicht produktiv erreichbar sein. Geplant ist ASP.NET Core Identity mit tenantlokaler Persistenz. Rollen, Claims, App-Zugänge und die genaue Token-/Cookie-Kopplung werden erst mit konkreten Anforderungen definiert; es existiert bewusst kein Fake-Security-System.

## FergensHub-Lizenzierung

FergensHub bleibt die zentrale Quelle für Tenant, Produkt, Lizenzstatus, Laufzeit, Tarif, Features, Limits und Tenant-Endpunkt. Cleanifico leitet Lizenzentscheidungen nicht aus einer konkurrierenden lokalen Lizenzarchitektur ab. Vor der Implementierung werden vorhandene Assetfico-/FergensHub-Verträge und Fehlerbehandlungs-Patterns geprüft; sie liegen aktuell nicht in diesem Repository vor. Authentifizierung, Autorisierung und Lizenzprüfung sind zwingende Pre-Production-Gates.

## Discovery

Die bestehende Discovery API soll später `Firmencode + Produkt` in Tenant-ID, Firmenname, API-Basis-URL und API-Version auflösen. Dies ist insbesondere für `Cleanifico.Mobile` vorgesehen. Discovery-Verträge sind noch nicht im Repository vorhanden und werden nicht vorab neu erfunden.

## Mobile, Storage und Hintergrunddienste

- `Cleanifico.Mobile`: späteres .NET-MAUI-Projekt; Offline-Synchronisierung und SQLite gehören nicht zu Prompt 001.
- Storage: noch kein Anbieter oder Vertrag festgelegt; Dokumente und Fotos werden erst mit dem jeweiligen Fachmodul geplant.
- Hintergrunddienste: derzeit keine. Jobs werden nur für konkrete Anwendungsfälle ergänzt.

## Tests

xUnit prüft derzeit:

- Domain-Invarianten, Normalisierung und Cleaning-Type-Lifecycle,
- Application-Orchestrierung, Eindeutigkeit, Suche, Filter und Sortierung,
- das tatsächliche EF-/Pomelo-Modell einschließlich Feldlängen und Indizes,
- alle Cleaning-Type-HTTP-Operationen und Fehlerstatus über einen lokalen Kestrel-Host mit isoliertem In-Memory-Testrepository,
- die Abhängigkeitsfreiheit der Domain,
- die exakten Projekt-Referenzen von Application, Infrastructure, API und Web,
- die Vollständigkeit der zehn initialen Solution-Projekte,
- den laufenden API-Health-Endpunkt.

Die HTTP-Tests tauschen nur den Repository-Port am Composition Root aus und berühren niemals eine Entwicklungs- oder produktive MySQL-Datenbank. Echte MySQL-Integrationstests bleiben offen, bis eine zuverlässig isolierte Testdatenbank bereitsteht.
