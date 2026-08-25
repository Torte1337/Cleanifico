# Cleanifico

Cleanifico ist eine mandantenfähige Betriebssoftware für Gebäudereinigungsunternehmen. Dieses Repository enthält Cleanifico Office, die ASP.NET-Core-API und das technische Fundament der Fachmodule.

## Aktueller Stand

- .NET 10 (`net10.0`), SDK-Familie über `global.json` festgelegt
- Clean-Architecture-orientierte Schichten ohne zyklische Abhängigkeiten
- EF Core 9.0.19 mit stabilem Pomelo-MySQL-Provider 9.0.0 für MySQL 8.4
- `CleanificoDbContext` und initiale Migration `InitialCleanificoPersistence`
- erster vertikaler Fachschnitt `CleaningType` mit Domain-Regeln, Application Service, REST API und Blazor-Verwaltung
- separate ASP.NET Core API mit `GET /health` und `/api/cleaning-types`
- separate Blazor-Web-App für Cleanifico Office mit Seite `/reinigungstypen`
- vier xUnit-Testprojekte mit Domain-, Application-, EF-Metadaten-, Architektur- und HTTP-Integrationstests
- noch keine Authentifizierung, Lizenzprüfung oder externen Integrationen

## Projektstruktur

| Pfad | Verantwortung |
| --- | --- |
| `src/Cleanifico.Domain` | Fachmodell und Domain-Regeln |
| `src/Cleanifico.Application` | Use Cases, Ports und fachliche Orchestrierung |
| `src/Cleanifico.Contracts` | Öffentliche API-Verträge und DTOs |
| `src/Cleanifico.Infrastructure` | EF-Core-/MySQL-Persistenz und technische Adapter |
| `src/Cleanifico.Api` | ASP.NET Core API und Composition Root |
| `src/Cleanifico.Web` | Blazor-basierte Office-Webanwendung |
| `tests` | Architektur-, Unit- und Integrationstests |
| `docs` | Aktuelle Wissens- und Architekturbasis |
| `Reports` | Historische Arbeitsnachweise pro Prompt |

## Lokale Entwicklung

Voraussetzungen sind ein passendes .NET-10-SDK und MySQL 8.4. Für die API wird der Connection String `ConnectionStrings:Cleanifico` benötigt. Lokale Zugangsdaten werden über User Secrets gesetzt und nicht committed:

```bash
dotnet user-secrets set --project src/Cleanifico.Api "ConnectionStrings:Cleanifico" "Server=localhost;Port=3306;Database=cleanifico_dev;User=<user>;Password=<password>"
```

Anschließend im Repository-Root ausführen:

```bash
dotnet restore Cleanifico.slnx
dotnet build Cleanifico.slnx --no-restore
dotnet test Cleanifico.slnx --no-build
```

API oder Web-App können separat gestartet werden:

```bash
dotnet run --project src/Cleanifico.Api
dotnet run --project src/Cleanifico.Web
```

Die Development-Konfiguration der Web-App erwartet die API unter `https://localhost:7182`; die Web-App selbst startet standardmäßig unter `https://localhost:7282`.

## Datenbankmigrationen

Migrationen werden kontrolliert ausgeführt. Die Anwendung migriert beim Start keine Datenbank automatisch. Eine lokale Datenbank kann mit einem expliziten, nicht eingecheckten Connection String aktualisiert werden:

```bash
dotnet ef database update \
  --project src/Cleanifico.Infrastructure \
  --startup-project src/Cleanifico.Api \
  --context CleanificoDbContext \
  --connection "Server=localhost;Port=3306;Database=cleanifico_dev;User=<user>;Password=<password>"
```

Neue Migrationen werden aus dem Repository-Root erzeugt:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Cleanifico.Infrastructure \
  --startup-project src/Cleanifico.Api \
  --context CleanificoDbContext \
  --output-dir Persistence/Migrations
```

Der Design-Time-Context verwendet ohne explizite Zielverbindung nur eine nicht funktionsfähige lokale Platzhalterverbindung zur Modellerzeugung. Produktive Tenant-Migrationen benötigen später einen kontrollierten Rollout-Prozess.

## API für Reinigungstypen

| Methode | Route | Zweck |
| --- | --- | --- |
| `GET` | `/api/cleaning-types?search=&isActive=` | Suchen, filtern und sortiert auflisten |
| `GET` | `/api/cleaning-types/{id}` | Einzelnen Reinigungstyp laden |
| `POST` | `/api/cleaning-types` | Reinigungstyp anlegen |
| `PUT` | `/api/cleaning-types/{id}` | Reinigungstyp bearbeiten |
| `POST` | `/api/cleaning-types/{id}/activate` | Reaktivieren |
| `POST` | `/api/cleaning-types/{id}/deactivate` | Deaktivieren |
| `DELETE` | `/api/cleaning-types/{id}` | Unreferenzierten Datensatz endgültig löschen |

Die Fachendpunkte sind noch nicht authentifiziert und müssen vor einem produktiven Einsatz abgesichert werden.

## Dokumentation

- [Projektwissen](docs/PROJECT_MEMORY.md)
- [Architektur](docs/ARCHITECTURE.md)
- [Entscheidungen](docs/DECISIONS.md)
- [Offene Aufgaben](docs/TODO.md)
