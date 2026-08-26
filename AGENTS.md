# Cleanifico – Arbeitsregeln

## Projektkontext

- Cleanifico ist eine B2B-Betriebssoftware für Gebäudereinigungsunternehmen.
- Cleanifico Office ist die Web-App für Büro und Administration; die ASP.NET Core API stellt die Serverfunktionen bereit.
- `Cleanifico.Mobile` wird später mit .NET MAUI für den Außendienst umgesetzt.
- Jeder Tenant erhält eine eigene API-/Anwendungsinstanz, Konfiguration und MySQL-Datenbank.
- Lizenzierung und Produktverwaltung erfolgen zentral über FergensHub; die Tenant-Auflösung erfolgt später über die Discovery API.

## Architekturregeln

- Domain hängt nicht von Infrastructure, API oder Web ab.
- Application benötigt keine konkrete Infrastructure-Implementierung.
- Contracts enthalten DTOs, Requests und Responses, aber keine Domain- oder EF-Entities.
- Infrastructure implementiert Persistenz und technische Adapter.
- API orchestriert HTTP-Aufrufe, enthält aber keine unnötige Businesslogik.
- Web enthält UI-Verhalten, aber keine zentrale Geschäftslogik.
- Projektabhängigkeiten bleiben zyklusfrei; funktionierende vorhandene Patterns haben Vorrang vor neuen Architekturen.

## Entwicklungsregeln

- Nullable Reference Types und moderne C#-Konventionen beibehalten.
- Keine Template-Demos, toten Code, unnötigen Packages oder unbegründeten Breaking Changes einführen.
- Keine Geschäftsanforderungen erfinden.
- Fachliche Stammdaten mit Historienbezug bevorzugt deaktivieren statt löschen.
- `CleaningObject` gehört verpflichtend zu genau einem `Customer`; die Beziehung darf keinen Cascade Delete verwenden, und referenzierte Customers dürfen nicht physisch gelöscht werden.
- Objektnummern sind tenantlokal eindeutig und änderbar; Objektadressen bleiben unabhängig von Kunden-Verwaltungsadressen.
- Kunden sind Auftraggeber und können später mehrere Objekte besitzen; referenzierte Kunden dürfen dann nur deaktiviert werden.
- Kundennummern sind innerhalb der tenantlokalen Datenbank eindeutig und bleiben durch Benutzer änderbar.
- Zeittypen sind frei konfigurierbare Datensätze, keine Enums; Standard-Zeittypen niemals sperren oder durch Initialisierung zurücksetzen.
- Spätere Zeitbuchungen müssen historisch relevante Zeittyp-Eigenschaften als Snapshot speichern.
- Technische Audit-Zeitstempel grundsätzlich in UTC speichern.
- Sicherheitsentscheidungen zentral über Rollen, Policies und Application-Services abbilden; Endpunkte nicht ad hoc nach Rollennamen verzweigen.
- Geschäftliche API- und Office-Bereiche benötigen zusätzlich eine gültige zentrale FergensHub-Lizenz; ohne belastbaren externen Contract gilt fail-closed und es darf keinen lokalen Lizenz-Bypass geben.
- Lizenzierung ersetzt weder Authentifizierung noch Rollen-/Policy-Autorisierung; `/health`, Login und Logout bleiben unabhängig von der Geschäftslizenz erreichbar.
- Passwörter, Tokens und sonstige Secrets weder fest codieren noch protokollieren oder als Contract ausgeben.
- Sicherheitsprüfungen in Tests gezielt ersetzen, aber nicht pauschal deaktivieren.

## Datenbankregeln

- EF Core über eigene Fluent-API-Konfigurationen abbilden.
- Schemaänderungen ausschließlich über Migrationen verwalten.
- Beim Produktionsstart keine automatische destruktive Schemaerstellung oder ungefragte Migration ausführen.
- Domain-/EF-Entities niemals direkt als API-Contracts verwenden.
- MySQL-spezifische Entscheidungen unter `docs/` dokumentieren.

## Arbeitsablauf je Prompt

1. `AGENTS.md` und die relevanten Dateien unter `docs/` lesen.
2. Nur relevante Codebereiche untersuchen.
3. Aufgabe implementieren und Tests ergänzen oder aktualisieren.
4. Restore nur bei Bedarf, danach Build und Tests ausführen.
5. Dauerhaft relevantes Wissen unter `docs/` aktualisieren.
6. Einen Report unter `Reports/` erzeugen.
7. `git status` und bei Änderungen `git diff` prüfen.
8. Ergebnis und offene Punkte kompakt melden.

## Wissensbasis und Reports

`docs/PROJECT_MEMORY.md` ist kein Logbuch. Nur dauerhaft wiederverwendbares Wissen gehört dorthin; historische Arbeitsdetails gehören ausschließlich in den jeweiligen Prompt-Report unter `Reports/`.

## Git

- Bestehende Commits nicht verändern und keine History-Rewrites durchführen.
- Keine Branches ohne Auftrag erstellen.
- Nicht automatisch committen oder pushen.
- Am Ende den uncommitted Git-Status melden.
