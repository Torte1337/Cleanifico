# Cleanifico

Cleanifico ist eine geplante, mandantenfähige Betriebssoftware für Gebäudereinigungsunternehmen. Dieses Repository enthält das technische Fundament für die Office-Webanwendung, die ASP.NET-Core-API und die späteren Fachmodule.

## Aktueller Stand

- .NET 10 (`net10.0`), SDK-Familie über `global.json` festgelegt
- Clean-Architecture-orientierte Schichten ohne zyklische Abhängigkeiten
- separate ASP.NET Core API mit technischem Endpunkt `GET /health`
- separate Blazor-Web-App für Cleanifico Office
- vier xUnit-Testprojekte mit Architektur- und API-Integrationstests
- noch keine Fachmodule, Datenbank, Authentifizierung oder externen Integrationen

## Projektstruktur

| Pfad | Verantwortung |
| --- | --- |
| `src/Cleanifico.Domain` | Fachmodell und Domain-Regeln |
| `src/Cleanifico.Application` | Use Cases, Ports und fachliche Orchestrierung |
| `src/Cleanifico.Contracts` | Öffentliche API-Verträge und DTOs |
| `src/Cleanifico.Infrastructure` | Spätere Persistenz und technische Adapter |
| `src/Cleanifico.Api` | ASP.NET Core API und Composition Root |
| `src/Cleanifico.Web` | Blazor-basierte Office-Webanwendung |
| `tests` | Architektur-, Unit- und Integrationstests |
| `docs` | Aktuelle Wissens- und Architekturbasis |
| `Reports` | Historische Arbeitsnachweise pro Prompt |

## Lokale Entwicklung

Voraussetzung ist ein passendes .NET-10-SDK. Anschließend im Repository-Root ausführen:

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

## Dokumentation

- [Projektwissen](docs/PROJECT_MEMORY.md)
- [Architektur](docs/ARCHITECTURE.md)
- [Entscheidungen](docs/DECISIONS.md)
- [Offene Aufgaben](docs/TODO.md)
