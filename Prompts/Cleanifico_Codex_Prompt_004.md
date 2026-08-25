# Cleanifico – Codex Prompt 004
## Konfigurierbare Zeittypen End-to-End

Arbeite im bestehenden Repository:

`~/Documents/Projekte/FergenixLabs/Cleanifico`

Nutze die vorhandenen Regeln und Patterns aus:
- `AGENTS.md`
- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`

Analysiere nur die für diesen Auftrag relevanten Bereiche.
Verwende das vorhandene `CleaningType`-Modul als technisches Muster, soweit sinnvoll.

# Ziel

Implementiere ein vollständiges Modul für **Zeittypen**.

Zeittypen sind **keine Enums und keine fest codierten Werte**.

Jeder Tenant erhält initial Standard-Zeittypen als normale Datensätze. Danach dürfen diese vollständig:
- umbenannt
- bearbeitet
- deaktiviert
- reaktiviert
- ergänzt

werden.

Ein Standard-Zeittyp darf technisch nicht gesperrt oder anders behandelt werden als ein später selbst angelegter Zeittyp.

# Entity

Bevorzugter Name:

`TimeType`

Mindestens:
- Id
- Name
- Code
- Description
- CountsAsWorkTime
- IsPaid
- RequiresObject
- IsAbsence
- Color
- SortOrder
- IsActive
- CreatedAtUtc
- UpdatedAtUtc

Regeln:
- Name erforderlich
- Code erforderlich
- Name und Code tenantlokal eindeutig
- Code normalisieren, bevorzugt Großbuchstaben
- Description optional
- Color optional
- SortOrder frei konfigurierbar
- neue Zeittypen standardmäßig aktiv
- technische Zeitstempel in UTC
- keine zusätzliche TenantId, da jeder Kunde eine eigene DB besitzt

# Bedeutung

`CountsAsWorkTime`
- zählt der Typ als Arbeitszeit?

`IsPaid`
- ist die Zeit grundsätzlich bezahlt?
- keine Lohnabrechnung bauen

`RequiresObject`
- muss bei späterer Buchung ein Objekt gewählt werden?

`IsAbsence`
- kennzeichnet Abwesenheitstypen wie Urlaub/Krankheit
- noch keinen Abwesenheitsworkflow bauen

# Standard-Zeittypen

Ein neuer Tenant erhält mindestens:

- ARB | Arbeitszeit
- PAU | Pause
- FAH | Fahrzeit
- URL | Urlaub
- KRK | Krankheit
- SCH | Schulung
- BES | Besprechung

Lege sinnvolle Startwerte für:
- CountsAsWorkTime
- IsPaid
- RequiresObject
- IsAbsence
- SortOrder
- Color

fest.

Wichtig:
Diese Werte sind nur Startwerte und danach vollständig änderbar.

Kein `IsSystem`, `IsBuiltIn`, `IsLocked` oder ähnliches einführen.

# Initialisierung

Standard-Zeittypen dürfen nur einmal initial angelegt werden.

Die Initialisierung muss idempotent sein.

Wenn der Kunde später:
- Namen ändert
- Codes ändert
- Eigenschaften verändert
- Typen deaktiviert

dürfen diese Änderungen beim nächsten Start niemals überschrieben oder zurückgesetzt werden.

Keine Produktionsdaten automatisch überschreiben.

# Historische Stabilität

Dokumentiere in `docs/DECISIONS.md`:

Zeittypen dürfen später jederzeit geändert werden, historische Arbeitszeitbuchungen dürfen sich dadurch aber nicht rückwirkend verändern.

Spätere `TimeEntry`-Datensätze müssen deshalb relevante Eigenschaften des Zeittyps als Snapshot speichern, mindestens sinngemäß:

- TimeTypeId
- TimeTypeNameSnapshot
- CountsAsWorkTimeSnapshot
- IsPaidSnapshot
- RequiresObjectSnapshot
- IsAbsenceSnapshot

**Noch kein TimeEntry-Modul implementieren.**

# Persistenz

Erweitere den vorhandenen `CleanificoDbContext`.

Erstelle eine Fluent-Configuration für `TimeType`.

Konfiguriere:
- Tabelle
- Primärschlüssel
- Required-Felder
- Längen
- Defaults
- Indizes
- eindeutigen Namen
- eindeutigen Code

Erzeuge eine neue EF-Core-Migration.
Bestehende Migrationen nicht verändern.

# Application / API

Implementiere entsprechend dem vorhandenen CleaningType-Pattern:

- GetAll
- GetById
- Create
- Update
- Activate
- Deactivate
- Delete

GET-Liste unterstützt mindestens:
- `search`
- `isActive`

API bevorzugt:

- GET `/api/time-types`
- GET `/api/time-types/{id}`
- POST `/api/time-types`
- PUT `/api/time-types/{id}`
- POST `/api/time-types/{id}/activate`
- POST `/api/time-types/{id}/deactivate`
- DELETE `/api/time-types/{id}`

Nutze bestehende Validation-, Error- und Contract-Patterns.
Keine Domain-/EF-Entities direkt über HTTP ausgeben.

# Autorisierung

Nutze das bestehende Rollen-/Policy-System.

Bei Bedarf:
- `ViewTimeTypes`
- `ManageTimeTypes`

Rechte:
- Owner: lesen + verwalten
- Administrator: lesen + verwalten
- Dispatcher: lesen
- ObjectManager: lesen
- Employee: kein administrativer Zugriff

API:
- anonym -> 401
- authentifiziert ohne Recht -> 403

# Web

Erstelle die Office-Seite:

`/zeittypen`

Optisch am vorhandenen Cleanifico-Stil und an `/reinigungstypen` orientieren.

Mindestens:
- Suche
- Statusfilter
- Tabelle/Liste
- Anlegen
- Bearbeiten
- Deaktivieren
- Reaktivieren
- Löschen, solange fachlich zulässig

Spalten:
- Kürzel
- Name
- Arbeitszeit
- Bezahlt
- Objekt erforderlich
- Abwesenheit
- Sortierung
- Status
- Aktionen

Bearbeitungsmaske:
- Name
- Kürzel
- Beschreibung
- Farbe
- Sortierung
- CountsAsWorkTime
- IsPaid
- RequiresObject
- IsAbsence

UI-Texte auf Deutsch.

# Löschen

Solange noch keine Zeitbuchungen existieren, darf ein Zeittyp physisch gelöscht werden.

Dokumentiere:
Sobald historische Zeitbuchungen existieren, darf ein verwendeter Zeittyp nur noch deaktiviert werden.

Noch keine künstlichen `TimeEntry`-Referenzen bauen.

# Tests

Mindestens testen:

- gültiger Zeittyp kann angelegt werden
- Name/Code erforderlich
- Code wird normalisiert
- doppelter Name verhindert
- doppelter Code verhindert
- Update funktioniert
- alle konfigurierbaren Eigenschaften können geändert werden
- Standard-Zeittyp kann nach Initialisierung verändert werden
- Initialisierung ist idempotent
- veränderte Standardtypen werden nicht zurückgesetzt
- Aktivieren/Deaktivieren funktioniert
- anonym -> 401
- Owner/Admin dürfen verwalten
- Dispatcher/ObjectManager dürfen lesen, aber nicht schreiben
- Employee darf administrativ nicht zugreifen

Bestehende Tests müssen grün bleiben.
Security nicht für Tests deaktivieren.

# Dokumentation

Nur bei dauerhaft relevantem Wissen aktualisieren:
- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- `AGENTS.md`
- `README.md`

Wichtig dokumentieren:
- TimeType ist frei konfigurierbar
- Standardwerte sind normale Kundendaten
- keine festen Enums
- Initialisierung nur einmal/idempotent
- Änderungen werden nie durch Seed-Logik überschrieben
- spätere TimeEntry-Historie verwendet Snapshots

# Nicht Bestandteil

Nicht implementieren:
- TimeEntry
- Soll-/Ist-Berechnung
- Mitarbeiter
- Kunden
- Objekte
- Verträge
- Dienstplanung
- MAUI
- FergensHub
- Discovery
- MFA
- Urlaubsworkflow
- Krankmeldungsworkflow
- Lohnabrechnung

Keine Feature-Ausweitung.

# Abschluss

Am Ende ausführen:

```bash
dotnet build
dotnet test
```

Ziel:
- 0 Fehler
- 0 Warnungen
- alle Tests grün

Erstelle:

`Reports/YYYY-MM-DD_HH-mm_Prompt-004_Time-Types.md`

Prüfe:
```bash
git status
git diff --stat
```

Nicht automatisch committen oder pushen.

Antworte kompakt mit:
- umgesetzt
- Migration
- Standard-Zeittypen / Initialisierung
- Autorisierung
- Web
- Build
- Tests
- Report
- Git-Status
- Empfehlung für Prompt 005
