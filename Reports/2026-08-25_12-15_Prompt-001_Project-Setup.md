# Report

## Auftrag

Das technische Fundament für Cleanifico von Grund auf erstellen: moderne `.slnx`, sechs initiale Produktprojekte, vier Testprojekte, klare Abhängigkeitsrichtung, minimale API- und Blazor-Basis, dauerhafte Wissensdateien sowie erfolgreiche Restore-, Build- und Testläufe.

## Analyse

Vor der Implementierung wurden geprüft:

- Arbeitsverzeichnis und vorhandene Dateien,
- lokale Repository-Vorgaben (`AGENTS.md`),
- installierte .NET-SDKs und Runtimes,
- installierte .NET-Workloads,
- Git-Status,
- verfügbare `.slnx`-, Web-API- und Blazor-Templates.

Ergebnis: .NET SDK 10.0.102 und der MAUI-Workload sind installiert. Der Arbeitsordner war noch kein Git-Repository. Vorhanden waren lediglich `.DS_Store` und `Prompts/Cleanifico_Codex_Prompt_001.md`; beide blieben unverändert. Im Cleanifico-Root existierte keine `AGENTS.md`.

## Änderungen

Erstellt beziehungsweise eingerichtet wurden:

- `Cleanifico.slnx` mit sechs Projekten unter `src/` und vier Projekten unter `tests/`,
- `global.json` für die .NET-10.0.100-SDK-Familie mit `latestPatch`,
- `.gitignore` und `README.md`,
- `Cleanifico.Domain`, `Cleanifico.Application`, `Cleanifico.Contracts` und `Cleanifico.Infrastructure` als Klassenbibliotheken,
- `Cleanifico.Api` als ASP.NET Core API mit Problem Details und `GET /health`,
- `Cleanifico.Web` als minimale Blazor-Web-App mit Server-Interaktivität,
- vier xUnit-Testprojekte,
- Assembly-Referenztypen für die vier Bibliotheksschichten,
- gemeinsame Testhilfe `tests/Architecture/RepositoryStructure.cs`,
- acht Architektur- und API-Integrationstests,
- vier Wissensdateien unter `docs/`,
- dieser Prompt-Report unter `Reports/`.

Entfernt wurden sämtliche bedeutungslosen Template-Inhalte: `Class1`-Klassen, `UnitTest1`-Tests, WeatherForecast-Endpunkt/-Typ, Hello-World-Inhalt und die WeatherForecast-HTTP-Datei.

Projekt-Referenzen:

- Application → Domain, Contracts
- Infrastructure → Domain, Application
- API → Application, Contracts, Infrastructure
- Web → Contracts
- Domain und Contracts → keine anderen Cleanifico-Projekte

## Architekturentscheidungen

- `.slnx` und .NET 10 bilden die initiale Plattform.
- API und Web sind getrennte Hosts; Web kennt nur öffentliche Contracts.
- Eine eigene Instanz und eine eigene MySQL-Datenbank pro Tenant bleiben verbindlich.
- FergensHub ist für Lizenzierung, Discovery für Tenant-Auflösung vorgesehen.
- `Cleanifico.Mobile` bleibt geplant, wurde aber nicht vorzeitig erzeugt.
- EF Core, Pomelo und Identity-Persistenz werden erst mit dem ersten echten Persistenzmodul eingebunden, damit keine ungenutzten Pakete oder Fake-Konfigurationen entstehen.

## Tests

Ausgeführt wurden:

```text
dotnet restore Cleanifico.slnx
dotnet build Cleanifico.slnx --no-restore
dotnet test Cleanifico.slnx --no-build --no-restore
```

Für die erste Wiederherstellung wurde im Codex-Sandboxkontext eine temporäre Offline-NuGet-Konfiguration verwendet; die benötigten Testpakete waren bereits lokal vorhanden. Anschließend war auch der vollständige Restore mit der normalen NuGet-Konfiguration erfolgreich. Wegen eines lokalen MSBuild-Parallelitätsproblems liefen beide kontrolliert mit einem MSBuild-Knoten. Der Testhost benötigte eine Freigabe für lokale Loopback-Sockets.

Ergebnis:

- Restore: erfolgreich, 10 Projekte
- Build: erfolgreich, 0 Warnungen, 0 Fehler
- Unit-/Architekturtests: 7 bestanden
- Integrationstests: 1 bestanden (`GET /health` über lokalen Kestrel-Host)
- Gesamt: 8 bestanden, 0 fehlgeschlagen

## Probleme / Risiken

- Der Ordner ist weiterhin kein initialisiertes Git-Repository; Prompt 001 verlangte nur die Prüfung eines eventuell vorhandenen Repositorys.
- FergensHub-, Assetfico- und Discovery-Referenzimplementierungen liegen nicht in diesem Repository und konnten daher noch nicht verglichen werden.
- Datenbank, Identity, Lizenzprüfung und Discovery sind absichtlich noch nicht implementiert.
- Die Sandbox blockierte standardmäßig lokale Testhost-Sockets; außerhalb der Sandbox ist dafür keine Sonderbehandlung vorgesehen.

## Offene Punkte

- Als nächster schmaler End-to-End-Schnitt werden Reinigungstypen empfohlen.
- Mit diesem ersten Persistenzmodul sollen EF Core, Pomelo, `DbContext`, MySQL-Konfiguration und Migrationsstrategie ergänzt werden.
- Vor Lizenz- und Discovery-Code müssen die bestehenden Verträge beziehungsweise Referenzimplementierungen geprüft werden.

## Aktualisierte Wissensdateien

- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
