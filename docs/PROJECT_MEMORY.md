# Cleanifico – Projektwissen

## Zweck und Zielgruppen

Cleanifico wird eine kommerzielle Betriebssoftware für Gebäudereinigungsunternehmen. Cleanifico Office richtet sich an Geschäftsführung, Verwaltung, Disposition, Objektleitung und Personalverwaltung. Die spätere Mobile-App richtet sich an Reinigungskräfte, Vorarbeiter und Objektleiter im Außendienst.

## Wichtige Fachbegriffe

- **Tenant:** ein lizenzierter Gebäudereinigungsbetrieb; besitzt eine eigene Cleanifico-Instanz und MySQL-Datenbank.
- **Kunde/Auftraggeber:** Geschäftskunde eines Tenants; kann mehrere Objekte besitzen.
- **Objekt:** konkreter Einsatz- beziehungsweise Reinigungsort eines Auftraggebers.
- **Reinigungstyp / Zeittyp:** tenantseitig konfigurierbare Klassifikation von Leistungen beziehungsweise Zeiten.

## Aktueller technischer Stand

- Solution `Cleanifico.slnx` mit sechs Produkt- und fünf Testprojekten auf `net10.0`.
- `CleaningType` ist der erste vollständige vertikale Fachschnitt: Domain, Application, Contracts, EF-/MySQL-Persistenz, REST API und Blazor-Seite.
- `TimeType` ist ein vollständiger Stammdaten-Schnitt mit frei änderbaren Arbeitszeit-/Bezahlungs-/Objekt-/Abwesenheitsmerkmalen, Farbe, Sortierung und Lifecycle.
- `Customer` bildet tenantlokale Auftraggeber mit eindeutiger änderbarer Kundennummer, direktem Ansprechpartner, Verwaltungsadresse, Notizen und Lifecycle ab.
- `CleanificoDbContext` nutzt EF Core 9.0.19 und Pomelo 9.0.0 mit einem fest konfigurierten MySQL-8.4-Serverprofil.
- Der Laufzeit-Connection-String wird ausschließlich unter `ConnectionStrings:Cleanifico` erwartet.
- Die Migrationen heißen `InitialCleanificoPersistence`, `AddTenantIdentity`, `AddConfigurableTimeTypes` und `AddCustomers`; sie werden nicht automatisch beim Hoststart ausgeführt.
- `ApplicationUser` und ASP.NET Core Identity liegen ohne zusätzliche `TenantId` in derselben tenantlokalen Datenbank. Benutzername ist die normalisierte, eindeutige E-Mail; Profile führen Vorname, Nachname, Aktivstatus und UTC-Auditzeiten.
- Rollen: `Owner`, `Administrator`, `Dispatcher`, `ObjectManager`, `Employee`. Zentrale Policies umfassen Office-Zugang, Lese-/Schreibrechte für CleaningType, TimeType und Customer, Benutzer-/Rollenverwaltung sowie die interne Aktivitätsprüfung.
- API und Web verwenden einen gemeinsamen verschlüsselten Identity-Cookie und Data-Protection-Schlüsselbund. Inaktive Benutzer werden bei Anmeldung und laufender Autorisierung abgewiesen.
- Cleanifico Office stellt `/login`, `/zugriff-verweigert`, `/kunden`, `/reinigungstypen`, `/zeittypen` und `/administration/benutzer` bereit; eine öffentliche Registrierung existiert nicht.
- Standard-Zeittypen werden genau einmal als normale Datensätze angelegt. Der technische Marker `TimeTypes.StandardData.v1` verhindert späteres Reseeding; Kundenänderungen werden niemals überschrieben.
- Rollen werden beim Start idempotent angelegt. Ein erster Owner entsteht nur durch explizite Secret-/Konfigurationswerte ohne fest codiertes Passwort.
- Lizenzprüfung und Discovery sind weiterhin nicht implementiert.

## Zentrale Dateien und Typen

- `src/Cleanifico.Domain/CleaningTypes/CleaningType.cs`: Entity, Normalisierung und Invarianten.
- `src/Cleanifico.Domain/TimeTypes/TimeType.cs`: frei konfigurierbarer Zeittyp mit fachlichen Merkmalen und Lifecycle.
- `src/Cleanifico.Domain/Customers/Customer.cs`: Auftraggeber, Ansprechpartner, Verwaltungsadresse und Lifecycle.
- `src/Cleanifico.Application/CleaningTypes`: Application Service und Persistenzabstraktion.
- `src/Cleanifico.Infrastructure/Persistence/CleanificoDbContext.cs`: zentraler EF Core Context.
- `src/Cleanifico.Infrastructure/Persistence/Configurations`: Fluent-API-Mappings.
- `src/Cleanifico.Infrastructure/Persistence/Migrations`: versionierte Schemaänderungen.
- `src/Cleanifico.Infrastructure/Security`: Identity-Persistenz, Benutzerverwaltung, Bootstrap und Active-User-Prüfung.
- `src/Cleanifico.Contracts/Security`: zentrale Rollen-, Policy- und Cookie-Namen.
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
- Benutzerkonten und spätere fachliche Mitarbeiterdatensätze sind getrennte Konzepte; eine Zuordnung wird erst mit konkreten Anforderungen eingeführt.
- Der letzte aktive Owner darf weder deaktiviert werden noch die Owner-Rolle verlieren.
- Stammdaten mit späterem Historienbezug werden regulär deaktiviert; physisches Löschen ist nur bei fehlenden Referenzen zulässig.
- Ein Customer kann später mehrere Objects besitzen. Object-Adressen werden nicht in die Kunden-Verwaltungsadresse integriert.
- CustomerNumber ist tenantlokal case-insensitive eindeutig, wird getrimmt und bleibt änderbar.
- Listen von Reinigungstypen sind standardmäßig nach `SortOrder`, danach `Name` sortiert.
- Zeittypen sind keine Enums und besitzen keine System-/Built-in-/Lock-Kennzeichen. Auch initiale Standardwerte bleiben vollständig änder- und löschbar.
- Spätere `TimeEntry`-Datensätze müssen relevante Zeittypwerte zusätzlich zur ID als historischen Snapshot führen.
- Nullable Reference Types und implizite Usings bleiben aktiviert.
- Keine Template-Demos oder ungenutzten Pakete.
- Nach jedem abgeschlossenen Prompt: Wissensdateien prüfen und einen Report unter `Reports/` anlegen.
