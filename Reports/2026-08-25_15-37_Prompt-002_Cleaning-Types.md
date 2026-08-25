# Report – Prompt 002

## Auftrag

Die dauerhaften Repository-Arbeitsregeln in `AGENTS.md` festlegen, das echte EF-Core-/MySQL-Persistenzfundament einrichten und Reinigungstypen als ersten vollständigen vertikalen Schnitt durch Domain, Application, Contracts, Infrastructure, API und Cleanifico Office umsetzen. Bestandteil waren außerdem eine initiale Migration, substanzielle Tests, aktualisierte Wissensdateien und die abschließende Git-Kontrolle.

## Vorheriger Stand

- `.slnx` mit sechs Produkt- und vier Testprojekten auf `net10.0`
- getrennte API- und Blazor-Hosts sowie `GET /health`
- 8 bestandene Architektur-/Health-Tests
- keine `AGENTS.md`, kein Fachmodell, kein Datenbankzugriff und keine Migration
- keine Authentifizierung, FergensHub-Integration oder Discovery-Anbindung
- Git-Branch `main` auf Stand des Prompt-001-Commits; `Prompts/Cleanifico_Codex_Prompt_002.md` war bereits untracked vorhanden

## Analyse

Vor Änderungen wurden README, sämtliche bestehenden Wissensdateien, Projektgraph, Hosts, Tests, lokale SDK-/EF-Tool-Versionen und Git-Status geprüft. Die Providerkompatibilität wurde anhand der offiziellen [Pomelo-Paketinformationen](https://www.nuget.org/packages/Pomelo.EntityFrameworkCore.MySql/9.0.0) und des [Pomelo-Projekts](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql) abgeglichen.

Zum Bearbeitungszeitpunkt existierte kein stabiler Pomelo-10-Provider. Deshalb wurde bewusst die stabile, unter `net10.0` nutzbare Kombination EF Core 9.0.19/Pomelo 9.0.0 gewählt und keine Preview-/Nightly-Abhängigkeit eingeführt. MySQL 8.4 wurde als explizites Serverprofil festgelegt.

## Implementierung

- `AGENTS.md` als kompakte, repositoryweite Arbeitsanweisung angelegt.
- `CleaningType` mit Guid-ID, Name, normalisiertem Code, optionaler Beschreibung, Status, Sortierung und UTC-Zeitstempeln modelliert.
- Invarianten für Pflichtfelder, Längen, nicht negative Sortierung und atomare Updates implementiert.
- Application Service und Repository-Port für Listen, Laden, Anlegen, Bearbeiten, Aktivieren, Deaktivieren und Löschen ergänzt.
- Suche nach Name/Code, optionaler Statusfilter und Standardsortierung nach `SortOrder`, danach `Name` umgesetzt.
- Separate Create-, Update- und Response-Contracts ergänzt.
- Zentrale, sichere API-Fehlerübersetzung für `400`, `404`, `409` und `500` eingerichtet.
- Blazor-Verwaltungsseite mit typisiertem API-Client, Suche, Filter, Dialogvalidierung und getrennten Lifecycle-Aktionen implementiert.
- Keine Seed-/Demodaten und kein Fake-Security- oder Fake-Lizenzsystem eingebaut.

## Neu erstellte Dateien

- `AGENTS.md`
- `src/Cleanifico.Domain/Common/DomainValidationException.cs`
- `src/Cleanifico.Domain/CleaningTypes/CleaningType.cs`
- `src/Cleanifico.Application/CleaningTypes/*`
- `src/Cleanifico.Contracts/CleaningTypes/*`
- `src/Cleanifico.Infrastructure/DependencyInjection.cs`
- `src/Cleanifico.Infrastructure/Persistence/CleanificoDbContext.cs`
- `src/Cleanifico.Infrastructure/Persistence/CleanificoDbContextFactory.cs`
- `src/Cleanifico.Infrastructure/Persistence/Configurations/CleaningTypeConfiguration.cs`
- `src/Cleanifico.Infrastructure/Persistence/Repositories/EfCleaningTypeRepository.cs`
- `src/Cleanifico.Infrastructure/Persistence/Migrations/*InitialCleanificoPersistence*`
- `src/Cleanifico.Infrastructure/Persistence/Migrations/CleanificoDbContextModelSnapshot.cs`
- `src/Cleanifico.Api/Endpoints/CleaningTypeEndpoints.cs`
- `src/Cleanifico.Api/ErrorHandling/ApiExceptionHandler.cs`
- `src/Cleanifico.Web/ApiClients/*`
- `src/Cleanifico.Web/Components/Pages/CleaningTypes.razor`
- `src/Cleanifico.Web/Components/Pages/CleaningTypes.razor.css`
- `tests/Cleanifico.Domain.Tests/CleaningTypeTests.cs`
- `tests/Cleanifico.Application.Tests/CleaningTypeServiceTests.cs`
- `tests/Cleanifico.Infrastructure.Tests/CleaningTypePersistenceTests.cs`
- `tests/Cleanifico.Api.Tests/ApiTestHost.cs`
- `tests/Cleanifico.Api.Tests/CleaningTypeEndpointTests.cs`
- dieser Report

## Geänderte Dateien

- `README.md`
- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- `src/Cleanifico.Infrastructure/Cleanifico.Infrastructure.csproj`
- `src/Cleanifico.Api/ApiApplication.cs`
- `src/Cleanifico.Api/Cleanifico.Api.csproj`
- `src/Cleanifico.Api/Properties/launchSettings.json`
- `src/Cleanifico.Web/Program.cs`
- `src/Cleanifico.Web/appsettings.Development.json`
- `src/Cleanifico.Web/Properties/launchSettings.json`
- `src/Cleanifico.Web/Components/Layout/MainLayout.razor`
- `src/Cleanifico.Web/Components/Layout/MainLayout.razor.css`
- `src/Cleanifico.Web/Components/Pages/Home.razor`
- `src/Cleanifico.Web/wwwroot/app.css`
- `tests/Cleanifico.Api.Tests/HealthEndpointTests.cs`

## Datenbank / Migrationen

- Pakete: EF Core 9.0.19, EF Core Design 9.0.19, EF Core Relational 9.0.19 und Pomelo 9.0.0.
- Laufzeitkonfiguration: `ConnectionStrings:Cleanifico`; API User-Secrets-ID wurde eingerichtet.
- `CleanificoDbContext` mit `DbSet<CleaningType>` und assemblyweiter Fluent-Konfiguration.
- Tabelle `CleaningTypes` mit Feldlängen, UTC-`datetime(6)`, eindeutigen case-insensitiven Indizes für Name/Code sowie Listenindex für Status/Sortierung/Name.
- Migration `20260825130433_InitialCleanificoPersistence` wurde real erzeugt und inhaltlich geprüft.
- Keine Migration wurde auf eine fremde, Entwicklungs- oder produktive Datenbank angewendet.
- Kein `EnsureCreated` und keine automatische Migration beim Hoststart; die Ausführung ist in README als expliziter Prozess dokumentiert.
- Das lokale globale `dotnet-ef` 9.0.17 meldete bei der Erzeugung nur, dass die Runtime-Patchversion 9.0.19 neuer ist. Die Migration wurde erfolgreich erzeugt; Restore und Build bleiben warnungsfrei.

## API

Implementierte Routen:

- `GET /api/cleaning-types`
- `GET /api/cleaning-types/{id}`
- `POST /api/cleaning-types`
- `PUT /api/cleaning-types/{id}`
- `POST /api/cleaning-types/{id}/activate`
- `POST /api/cleaning-types/{id}/deactivate`
- `DELETE /api/cleaning-types/{id}`

Die Listenroute akzeptiert `search` und `isActive`. Endpoints arbeiten ausschließlich mit Contracts und Application Services. Validation Problems, Not Found und Konflikte werden verständlich ausgeliefert; unerwartete interne Details bleiben serverseitig.

## Web

- Desktoporientierte Cleanifico-Office-Hülle mit Navigation und aktiver Seite ergänzt.
- Seite `/reinigungstypen` mit Seitenkopf, Suche, Statusfilter, sortierter Tabelle und Loading-/Empty-/Error-Zuständen umgesetzt.
- Dialog zum Anlegen/Bearbeiten mit deutscher Data-Annotations-Validierung; Eingaben bleiben bei API-Fehlern erhalten.
- Deaktivierung mit Bestätigung, Reaktivierung und deutlich abgesetztes endgültiges Löschen mit Warnung umgesetzt.
- Typisierter HTTP-Client verarbeitet sichere Problem-Details-Meldungen und zeigt keine rohen Serverantworten an.
- Keine zusätzliche große UI-Bibliothek installiert.

## Tests

Finales Ergebnis: 41 bestanden, 0 fehlgeschlagen, 0 übersprungen.

- Domain: Erstellung, Normalisierung, Pflichtfelder, Sortierung, UTC, atomare Updates, Deaktivierung und Reaktivierung.
- Application: Erstellen, Eindeutigkeitskonflikte, Update, Suche, Statusfilter, Sortierung, Not Found, Lifecycle und Delete.
- Infrastructure: reales EF-/Pomelo-Modell, Tabelle, Feldlängen, Nullability sowie eindeutige und zusammengesetzte Indizes.
- API: Liste, Einzelabruf, Create, Update, Activate, Deactivate, Delete, Not Found, Validation und Duplicate Conflict über echten lokalen Kestrel-HTTP-Host.
- Architektur: bestehende Projekt- und Solution-Grenzen bleiben abgesichert.

Die API-Tests ersetzen ausschließlich den Repository-Port durch eine isolierte In-Memory-Testimplementierung. Es wurde bewusst kein vermeintlicher MySQL-Test gegen eine normale Datenbank gebaut.

## Build

Final ausgeführt:

```text
dotnet restore Cleanifico.slnx --disable-build-servers -p:NuGetAudit=false -p:RestoreDisableParallel=true -m:1 --verbosity minimal
dotnet build Cleanifico.slnx --no-restore --disable-build-servers -m:1 --verbosity minimal
dotnet test Cleanifico.slnx --no-build --no-restore --disable-build-servers -m:1 --verbosity minimal
```

Ergebnis:

- Restore erfolgreich; alle Projekte aktuell
- Build erfolgreich; 0 Warnungen, 0 Fehler
- Tests erfolgreich; 41/41 grün

Die serielle MSBuild-Ausführung behält den bereits aus Prompt 001 bekannten stabilen lokalen Ablauf bei. Die Testfreigabe wurde nur für zufällige lokale Loopback-Ports benötigt.

## Architekturentscheidungen

- Stabiles EF Core 9.0.19/Pomelo 9.0.0 auf `net10.0`; kein Preview-Provider.
- Eigene MySQL-Datenbank pro Tenant bleibt Isolationsgrenze; daher vorerst keine redundante `TenantId` auf `CleaningType`.
- Persistenzmapping ausschließlich über separate Fluent-API-Konfiguration.
- API-Contracts bleiben vollständig von Domain-/EF-Entities getrennt.
- Reinigungstypen werden regulär deaktiviert; physisches Löschen ist nur bei fehlenden Referenzen zulässig.
- Schemaänderungen laufen ausschließlich über versionierte, kontrolliert ausgeführte Migrationen.

## Sicherheits-/Lizenzstatus

Identity, Authentifizierung, Autorisierung und FergensHub-Lizenzprüfung wurden bewusst nicht vorgetäuscht. Die Cleaning-Type-Endpunkte sind derzeit ungeschützt und nicht produktionsbereit. Vor Pre-Production müssen die Endpunkte authentifiziert/autorisiert und reale FergensHub-/Discovery-Verträge geprüft und integriert werden. Zugangsdaten wurden nicht eingecheckt; die API erwartet sie über Configuration/User Secrets beziehungsweise die spätere Secret-Verteilung.

## Bekannte Einschränkungen

- Noch keine echte MySQL-Integrationstestumgebung; getestet wird das reale Provider-Modell ohne Datenbankverbindung.
- Die initiale Migration wurde erzeugt, aber bewusst auf keine Datenbank angewendet.
- Pomelo 10 war zum Bearbeitungszeitpunkt nicht stabil verfügbar; der gemeinsame EF-/Provider-Upgradepfad bleibt offen.
- Es existieren noch keine fachlichen Referenzen auf Reinigungstypen. Der vorhandene MySQL-Fremdschlüsselkonflikt wird erst mit späteren Modulen praktisch relevant.
- FergensHub-, Assetfico- und Discovery-Referenzverträge liegen nicht im Repository vor.

## Offene Punkte

- Als Prompt 003 wird ein eigener tenantlokaler Identity-/Autorisierungsschnitt mit konkretem Rollenmodell und Absicherung der vorhandenen API empfohlen.
- Einen kontrollierten Tenant-Migrationsrollout und eine isolierte MySQL-Testdatenbank einrichten.
- Reale FergensHub-/Discovery-Verträge vor der Lizenzintegration bereitstellen und prüfen.
- CI für Restore, Build und Tests ergänzen.

## Aktualisierte Wissensdateien

- `AGENTS.md`
- `README.md`
- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`

## Git-Status

Die Änderungen sind nicht committed und nicht gepusht. `main` wurde nicht gewechselt, die bestehende History blieb unverändert. Neben allen Prompt-002-Änderungen ist `Prompts/Cleanifico_Codex_Prompt_002.md` weiterhin untracked; diese Datei war bereits vor der Implementierung vorhanden und wurde nicht verändert.
