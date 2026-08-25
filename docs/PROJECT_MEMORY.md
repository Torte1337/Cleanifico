# Cleanifico – Projektwissen

## Zweck und Zielgruppen

Cleanifico wird eine kommerzielle Betriebssoftware für Gebäudereinigungsunternehmen. Cleanifico Office richtet sich an Geschäftsführung, Verwaltung, Disposition, Objektleitung und Personalverwaltung. Die spätere Mobile-App richtet sich an Reinigungskräfte, Vorarbeiter und Objektleiter im Außendienst.

## Wichtige Fachbegriffe

- **Tenant:** ein lizenzierter Gebäudereinigungsbetrieb; besitzt eine eigene Cleanifico-Instanz und MySQL-Datenbank.
- **Kunde/Auftraggeber:** Geschäftskunde eines Tenants; kann mehrere Objekte besitzen.
- **Objekt:** konkreter Einsatz- beziehungsweise Reinigungsort eines Auftraggebers.
- **Reinigungstyp / Zeittyp:** tenantseitig konfigurierbare Klassifikation von Leistungen beziehungsweise Zeiten.

## Aktueller technischer Stand

- Solution `Cleanifico.slnx` mit sechs Produkt- und vier Testprojekten auf `net10.0`.
- `CleaningType` ist der erste vollständige vertikale Fachschnitt: Domain, Application, Contracts, EF-/MySQL-Persistenz, REST API und Blazor-Seite.
- `CleanificoDbContext` nutzt EF Core 9.0.19 und Pomelo 9.0.0 mit einem fest konfigurierten MySQL-8.4-Serverprofil.
- Der Laufzeit-Connection-String wird ausschließlich unter `ConnectionStrings:Cleanifico` erwartet.
- Die initiale Migration heißt `InitialCleanificoPersistence`; Migrationen werden nicht automatisch beim Hoststart ausgeführt.
- Die API stellt `GET /health` und CRUD-/Lifecycle-Routen unter `/api/cleaning-types` bereit.
- Cleanifico Office ruft die API über einen typisierten HTTP-Client auf; die Verwaltungsseite liegt unter `/reinigungstypen`.
- Authentifizierung, Lizenzprüfung und Discovery sind noch nicht implementiert und vor Produktiveinsatz zwingend nachzuziehen.

## Zentrale Dateien und Typen

- `src/Cleanifico.Domain/CleaningTypes/CleaningType.cs`: Entity, Normalisierung und Invarianten.
- `src/Cleanifico.Application/CleaningTypes`: Application Service und Persistenzabstraktion.
- `src/Cleanifico.Infrastructure/Persistence/CleanificoDbContext.cs`: zentraler EF Core Context.
- `src/Cleanifico.Infrastructure/Persistence/Configurations`: Fluent-API-Mappings.
- `src/Cleanifico.Infrastructure/Persistence/Migrations`: versionierte Schemaänderungen.
- `src/Cleanifico.Api/ApiApplication.cs`: Composition Root, Fehlerbehandlung und Routenregistrierung.
- `src/Cleanifico.Web/ApiClients`: typisierte API-Zugriffe ohne Serverprojekt-Abhängigkeit.
- `tests/Architecture/RepositoryStructure.cs`: gemeinsame Prüfung der Projekt- und Solution-Struktur.
- `docs/ARCHITECTURE.md`: verbindliche Beschreibung des aktuellen und geplanten Aufbaus.
- `docs/DECISIONS.md`: akzeptierte Architekturentscheidungen.

## Konventionen

- Abhängigkeiten zeigen nach innen; Domain und Contracts kennen keine Infrastruktur.
- Öffentliche API-Nachrichten gehören nach `Cleanifico.Contracts`.
- Keine Fachlogik in API, Web oder Infrastructure.
- Tenant-Isolation erfolgt durch eine eigene Datenbank; Business-Entities tragen derzeit keine zusätzliche `TenantId`.
- Stammdaten mit späterem Historienbezug werden regulär deaktiviert; physisches Löschen ist nur bei fehlenden Referenzen zulässig.
- Listen von Reinigungstypen sind standardmäßig nach `SortOrder`, danach `Name` sortiert.
- Nullable Reference Types und implizite Usings bleiben aktiviert.
- Keine Template-Demos oder ungenutzten Pakete.
- Nach jedem abgeschlossenen Prompt: Wissensdateien prüfen und einen Report unter `Reports/` anlegen.
