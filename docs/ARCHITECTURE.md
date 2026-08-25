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
| `Cleanifico.Domain` | Entities, Value Objects, Enums, Domain-Regeln und Domain Exceptions | Assembly-Referenz; noch kein Fachmodell |
| `Cleanifico.Application` | Use Cases, Interfaces, Validierung und Orchestrierung | Assembly-Referenz; noch keine Use Cases |
| `Cleanifico.Contracts` | DTOs, Requests, Responses und öffentliche Verträge | Assembly-Referenz; noch keine Fachverträge |
| `Cleanifico.Infrastructure` | EF Core, MySQL, Identity-Persistenz, Repositories und externe Adapter | Assembly-Referenz; Persistenz bewusst noch nicht eingerichtet |
| `Cleanifico.Api` | ASP.NET Core API und serverseitiger Composition Root | Problem Details und `GET /health` |
| `Cleanifico.Web` | Blazor-Oberfläche für Cleanifico Office | minimale Blazor-Web-App mit Server-Interaktivität |
| `*.Tests` | Unit-, Architektur- und Integrationstests | Referenzgraph-, Solution- und Health-Tests |

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

Jeder Tenant erhält eine eigene Tenant-ID, API-/Instanz, Konfiguration, Lizenz und MySQL-Datenbank. Eine zentrale gemeinsame Cleanifico-Fachdatenbank ist ausgeschlossen. Die konkrete Deployment- und Connection-String-Strategie wird festgelegt, sobald wiederverwendbare FergensHub-/Assetfico-Patterns verfügbar und geprüft sind.

## Datenbank und Persistenz

Vorgesehen sind Entity Framework Core und MySQL mit Pomelo. Die Identity-Persistenz soll ebenfalls tenantlokal in MySQL liegen. In Prompt 001 wurden noch keine Providerpakete, kein `DbContext` und keine Migration angelegt: Ohne echtes Persistenzmodell wären diese Abhängigkeiten ungenutzt und ihre Konfiguration spekulativ. Sie werden gemeinsam mit dem ersten schmalen Persistenzmodul versionskonsistent ergänzt.

## API und Web-App

`Cleanifico.Api` ist der serverseitige Composition Root. Aktuell registriert sie Problem Details und Health Checks und stellt `GET /health` bereit. Spätere Use Cases werden aus Application aufgerufen; technische Implementierungen werden aus Infrastructure injiziert.

`Cleanifico.Web` ist eine eigenständige Blazor-Web-App mit serverseitiger Interaktivität. Sie referenziert nur öffentliche Contracts und soll die API später über typisierte HTTP-Clients aufrufen. Dadurch gelangen weder Infrastructure noch serverinterne Application-Implementierungen in die UI.

## Authentifizierung und Autorisierung

Noch nicht implementiert. Geplant ist ASP.NET Core Identity mit tenantlokaler Persistenz. Rollen, Claims, App-Zugänge und die genaue Token-/Cookie-Kopplung werden erst mit konkreten Anforderungen definiert.

## FergensHub-Lizenzierung

FergensHub bleibt die zentrale Quelle für Tenant, Produkt, Lizenzstatus, Laufzeit, Tarif, Features, Limits und Tenant-Endpunkt. Cleanifico soll Lizenzentscheidungen langfristig nicht ausschließlich aus lokaler Konfiguration ableiten. Vor der Implementierung werden vorhandene Assetfico-/FergensHub-Verträge und Fehlerbehandlungs-Patterns geprüft; sie liegen aktuell nicht in diesem Repository vor.

## Discovery

Die bestehende Discovery API soll später `Firmencode + Produkt` in Tenant-ID, Firmenname, API-Basis-URL und API-Version auflösen. Dies ist insbesondere für `Cleanifico.Mobile` vorgesehen. Discovery-Verträge sind noch nicht im Repository vorhanden und werden nicht vorab neu erfunden.

## Mobile, Storage und Hintergrunddienste

- `Cleanifico.Mobile`: späteres .NET-MAUI-Projekt; Offline-Synchronisierung und SQLite gehören nicht zu Prompt 001.
- Storage: noch kein Anbieter oder Vertrag festgelegt; Dokumente und Fotos werden erst mit dem jeweiligen Fachmodul geplant.
- Hintergrunddienste: derzeit keine. Jobs werden nur für konkrete Anwendungsfälle ergänzt.

## Tests

xUnit prüft derzeit:

- die verbotene Abhängigkeitsfreiheit der Domain,
- die exakten Projekt-Referenzen von Application, Infrastructure, API und Web,
- die Vollständigkeit der zehn initialen Solution-Projekte,
- den laufenden API-Health-Endpunkt über einen echten lokalen Kestrel-Host.
