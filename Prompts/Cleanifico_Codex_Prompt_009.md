# Cleanifico – Codex Prompt 009
## Mitarbeiterverwaltung End-to-End

Arbeite im bestehenden Repository:

`~/Documents/Projekte/FergenixLabs/Cleanifico`

Nutze `AGENTS.md` und nur die relevanten Dateien unter `docs/`.
Analysiere ausschließlich die für diesen Auftrag nötigen Bereiche.

Verwende bestehende Patterns aus Customer, CleaningObject, CleaningType, TimeType, Identity und Authorization.

# Ziel

Implementiere ein vollständiges Modul für **Mitarbeiter**.

Ein Mitarbeiter ist eine fachliche Personal-Entity des Reinigungsunternehmens.

Wichtig:

> `Employee` ist nicht dasselbe wie `ApplicationUser`.

Ein Mitarbeiter kann später optional mit einem Login/App-Zugang verknüpft werden, muss aber auch ohne Benutzerkonto existieren können.

Noch keine automatische Verknüpfung mit Identity erzwingen.

# Entity

Bevorzugter Name: `Employee`

Mindestens:
- Id
- EmployeeNumber
- FirstName
- LastName
- Street
- PostalCode
- City
- Country
- Email
- Phone
- MobilePhone
- DateOfBirth
- EmploymentStartDate
- EmploymentEndDate
- EmploymentType
- WeeklyHours
- MonthlyTargetHours
- Notes
- IsActive
- CreatedAtUtc
- UpdatedAtUtc

Optional nur wenn sauber begründet:
- EmergencyContactName
- EmergencyContactPhone

Keine unnötigen HR-Felder hinzufügen.

# EmploymentType

EmploymentType soll für den aktuellen Stand sinnvoll modelliert werden.

Wenn Beschäftigungsarten voraussichtlich kundenspezifisch sein sollen, keine feste Enum-Struktur erzwingen.

Beispiele:
- Vollzeit
- Teilzeit
- Minijob
- Werkstudent
- Aushilfe

Noch keine Lohnabrechnung bauen.

# Regeln

`EmployeeNumber`
- erforderlich
- tenantlokal eindeutig
- vom Benutzer änderbar
- Trim
- sinnvolle Maximallänge

`FirstName` und `LastName`
- erforderlich
- Trim
- sinnvolle Maximallängen

`EmploymentEndDate`
- darf nicht vor `EmploymentStartDate` liegen

`WeeklyHours` und `MonthlyTargetHours`
- dürfen nicht negativ sein
- noch keine automatische Sollstundenberechnung implementieren

Neue Mitarbeiter sind standardmäßig aktiv.

Deaktivieren bedeutet nicht automatisch Austrittsdatum setzen.

# Löschen

Solange ein Mitarbeiter noch keine fachlichen Referenzen besitzt, darf physisches Löschen erlaubt sein.

Dokumentiere:

Sobald später Verträge, Arbeitszeiten, Objektzuweisungen, Einsätze, Schlüssel oder historische Daten auf einen Mitarbeiter verweisen, darf er nicht mehr physisch gelöscht werden.

Dann nur deaktivieren.

Noch keine künstlichen Referenzen auf zukünftige Module bauen.

# Persistenz

Erweitere `CleanificoDbContext`.

Erstelle Fluent Configuration für `Employee`.

Konfiguriere mindestens:
- Tabelle
- Primärschlüssel
- Required-Felder
- Längen
- eindeutige EmployeeNumber
- sinnvolle Indizes
- Auditfelder
- passende Datentypen für Stundenwerte

Erzeuge eine neue EF-Core-Migration.
Bestehende Migrationen nicht verändern.

# Application / API

Implementiere:
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

`search` soll mindestens über folgende Felder suchen:
- EmployeeNumber
- FirstName
- LastName
- Email
- Phone
- City

API:
- GET `/api/employees`
- GET `/api/employees/{id}`
- POST `/api/employees`
- PUT `/api/employees/{id}`
- POST `/api/employees/{id}/activate`
- POST `/api/employees/{id}/deactivate`
- DELETE `/api/employees/{id}`

Nutze bestehende Validation-, Error- und Contract-Patterns.

Keine Domain-/EF-Entities direkt über HTTP ausgeben.

# Contracts

Mindestens:
- `EmployeeResponse`
- `CreateEmployeeRequest`
- `UpdateEmployeeRequest`

Keine Identity-internen Felder exponieren.

# Autorisierung

Nutze das bestehende Rollen-/Policy-System.

Bei Bedarf:
- `ViewEmployees`
- `ManageEmployees`

Rechte:
- Owner: lesen + verwalten
- Administrator: lesen + verwalten
- Dispatcher: lesen
- ObjectManager: lesen
- Employee: kein administrativer Zugriff

API:
- anonym -> 401
- authentifiziert ohne Recht -> 403

Bestehende Lizenzprüfung bleibt zusätzlich wirksam.

# Web

Erstelle die Office-Seite:

`/mitarbeiter`

Optisch am bestehenden Cleanifico-Stil orientieren.

Mindestens:
- Suche
- Statusfilter
- Mitarbeiterliste
- Anlegen
- Bearbeiten
- Deaktivieren
- Reaktivieren
- Löschen, solange zulässig
- Detailansicht

Spalten mindestens:
- Personalnummer
- Name
- Beschäftigungsart
- Wochenstunden
- Ort
- Kontakt
- Status
- Aktionen

Bearbeitungsmaske mindestens:
- Personalnummer
- Vorname
- Nachname
- Straße
- PLZ
- Ort
- Land
- E-Mail
- Telefon
- Mobiltelefon
- Geburtsdatum
- Beschäftigungsbeginn
- Beschäftigungsende
- Beschäftigungsart
- Wochenstunden
- Monatliche Sollstunden
- Notizen

UI-Texte auf Deutsch.

# Detailansicht

Mindestens anzeigen:
- Stammdaten
- Adresse
- Kontakt
- Beschäftigungsdaten
- Wochenstunden
- Monatliche Sollstunden
- Status

Noch keine Fake-Tabs für Verträge, Arbeitszeiten, Objekte, Schlüssel, Abwesenheiten oder App-Zugang.

# Validierung

Mindestens:
- EmployeeNumber erforderlich
- EmployeeNumber eindeutig
- FirstName erforderlich
- LastName erforderlich
- gültige E-Mail, wenn angegeben
- WeeklyHours >= 0
- MonthlyTargetHours >= 0
- EmploymentEndDate nicht vor EmploymentStartDate
- sinnvolle Maximallängen
- Whitespace normalisieren

Verständliche deutsche Fehlermeldungen.

# Tests

Mindestens testen:
- gültiger Mitarbeiter kann angelegt werden
- Personalnummer erforderlich
- Vorname/Nachname erforderlich
- doppelte Personalnummer verhindert
- ungültige Datumsreihenfolge verhindert
- negative Stunden verhindert
- Update funktioniert
- Suche funktioniert
- Statusfilter funktioniert
- Aktivieren/Deaktivieren funktioniert
- unreferenzierter Mitarbeiter kann gelöscht werden
- anonym -> 401
- Owner/Admin dürfen verwalten
- Dispatcher/ObjectManager dürfen lesen, aber nicht schreiben
- Employee kein administrativer Zugriff
- Lizenzprüfung bleibt wirksam

Bestehende Tests müssen grün bleiben.
Security und Lizenzprüfung nicht für Tests deaktivieren.

# Dokumentation

Nur dauerhaft relevantes Wissen aktualisieren:
- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- ggf. `AGENTS.md`
- ggf. `README.md`

Wichtig dokumentieren:
- `Employee` ist fachliche Personal-Entity
- `Employee` und `ApplicationUser` bleiben getrennte Konzepte
- spätere optionale Verknüpfung möglich
- EmployeeNumber tenantlokal eindeutig
- physisches Löschen nur solange unreferenziert

# Nicht Bestandteil

Nicht implementieren:
- Mitarbeiterverträge
- Arbeitszeiten
- TimeEntry
- Objektzuweisungen
- Schlüsselverwaltung
- Urlaub
- Krankheit
- Abwesenheiten
- MAUI
- App-Zugang
- automatische Identity-Verknüpfung
- Lohnabrechnung
- Dienstplanung
- FergensHub-Umbauten
- Discovery

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

`Reports/YYYY-MM-DD_HH-mm_Prompt-009_Employees.md`

Prüfe:

```bash
git status
git diff --stat
git diff --check
```

Nicht automatisch committen oder pushen.

Antworte kompakt mit:
- umgesetzt
- Migration
- Employee/ApplicationUser-Trennung
- API
- Autorisierung
- Web
- Build
- Tests
- Report
- Git-Status
- Empfehlung für Prompt 010
