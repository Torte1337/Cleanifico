# Cleanifico – Entscheidungen

## DEC-001 – Eigene API-/Instanz pro Tenant

Status: Accepted

Entscheidung: Jeder Cleanifico-Tenant erhält eine eigene API-/Anwendungsinstanz.

Grund: Starke Isolation, tenantbezogene Konfiguration und kontrollierbare Deployments.

Datum: 2026-08-25

## DEC-002 – Eigene MySQL-Datenbank pro Tenant

Status: Accepted

Entscheidung: Jeder Tenant erhält eine eigene MySQL-Datenbank; es gibt keine zentrale Cleanifico-Fachdatenbank für alle Kunden.

Grund: Datenisolation, vereinfachte Backups und Migrationen sowie geringeres Risiko tenantübergreifender Zugriffe.

Datum: 2026-08-25

## DEC-003 – Lizenzierung über FergensHub

Status: Accepted

Entscheidung: FergensHub ist die zentrale Quelle für Produktlizenz, Tarif, Features, Limits und Laufzeit.

Grund: Vermeidung einer zweiten unabhängigen Lizenzarchitektur und zentrale Produktverwaltung.

Datum: 2026-08-25

## DEC-004 – Tenant-Auflösung über Discovery

Status: Accepted

Entscheidung: Firmencode und Produkt werden später über die bestehende Discovery API zum Tenant-Endpunkt aufgelöst.

Grund: Clients müssen keine tenantbezogenen Endpunkte fest einbauen.

Datum: 2026-08-25

## DEC-005 – Blazor-Web-App für das Büro

Status: Accepted

Entscheidung: Cleanifico Office wird als eigenständige Blazor-Web-App umgesetzt und kommuniziert über öffentliche Contracts mit der API.

Grund: Klare Trennung von UI und Serverimplementierung sowie Eignung für die Büro-Zielgruppen.

Datum: 2026-08-25

## DEC-006 – Spätere .NET-MAUI-App für den Außendienst

Status: Accepted

Entscheidung: Die Außendienst-App wird später als `Cleanifico.Mobile` mit .NET MAUI umgesetzt; sie ist noch nicht Teil der initialen Solution.

Grund: Mobile- und Offline-Anforderungen sollen separat und erst mit konkreten Workflows eingeführt werden.

Datum: 2026-08-25

## DEC-007 – Klare Schichtentrennung

Status: Accepted

Entscheidung: Domain, Application, Contracts, Infrastructure, API und Web besitzen eindeutige Verantwortlichkeiten und einen zyklusfreien Referenzgraphen.

Grund: Fachlogik bleibt unabhängig von UI, Persistenz und externen Diensten und kann gezielt getestet werden.

Datum: 2026-08-25

## DEC-008 – `.slnx` als Solution-Format

Status: Accepted

Entscheidung: Die Solution wird ausschließlich als `Cleanifico.slnx` geführt.

Grund: Modernes, kompaktes und gut diffbares Solution-Format des verwendeten SDKs.

Datum: 2026-08-25

## DEC-009 – .NET 10 als initiale Plattform

Status: Accepted

Entscheidung: Alle initialen Projekte zielen auf `net10.0`; die SDK-Familie wird über `global.json` festgelegt.

Grund: Installierte, aktuelle und langfristig geeignete Plattform für ein neu gestartetes Produkt.

Datum: 2026-08-25

## DEC-010 – Persistenzpakete erst mit einem echten Modul

Status: Accepted

Entscheidung: EF Core, Pomelo und Identity-Persistenz werden in Prompt 001 noch nicht als Pakete eingebunden.

Grund: Ohne Fachmodell, `DbContext` und Konfiguration wären die Pakete ungenutzt; die passende Version und Einrichtung wird mit dem ersten Persistenzmodul festgelegt.

Datum: 2026-08-25
