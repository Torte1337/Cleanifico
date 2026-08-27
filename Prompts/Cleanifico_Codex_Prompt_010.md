# Cleanifico – Codex Prompt 010
## Mitarbeiterverträge und historienfähige Beschäftigungsdaten

Arbeite im bestehenden Repository:

`~/Documents/Projekte/FergenixLabs/Cleanifico`

Nutze `AGENTS.md` und nur die relevanten Dateien unter `docs/`.
Analysiere ausschließlich die für diesen Auftrag nötigen Bereiche und verwende die bestehenden Patterns.

# Ziel

Implementiere ein vollständiges Modul für **Mitarbeiterverträge**.

Ein `Employee` kann mehrere historische Verträge besitzen.

Wichtig:

> `Employee` enthält persönliche Stammdaten.
> `EmployeeContract` ist die fachliche Quelle für historienfähige Beschäftigungsbedingungen.

Noch keine Lohnabrechnung, Arbeitszeiten oder Dokumentenverwaltung bauen.

# Entity

Bevorzugter Name:

`EmployeeContract`

Mindestens:

- Id
- ContractNumber
- EmployeeId
- StartDate
- EndDate
- IsPermanent
- EmploymentType
- WeeklyHours
- MonthlyTargetHours
- VacationDaysPerYear
- ProbationEndDate
- Notes
- IsActive
- CreatedAtUtc
- UpdatedAtUtc

Optional nur wenn sauber begründet:

- TerminationNotice
- JobTitle

# Beziehung

Implementiere:

`Employee 1 -> n EmployeeContracts`

mit echtem Foreign Key.

Ein Vertrag darf nicht ohne existierenden Mitarbeiter angelegt werden.

Keine Cascade-Löschung historischer Verträge beim Löschen eines Mitarbeiters.

Sobald ein Mitarbeiter mindestens einen Vertrag besitzt, darf der Mitarbeiter nicht mehr physisch gelöscht werden. Dann nur deaktivieren.

# Historie / Source of Truth

Prüfe die aktuell auf `Employee` vorhandenen Felder:

- EmploymentType
- WeeklyHours
- MonthlyTargetHours
- EmploymentStartDate
- EmploymentEndDate

Da Vertragsbedingungen künftig historienfähig sein müssen, darf es keine widersprüchlichen Wahrheiten zwischen `Employee` und `EmployeeContract` geben.

Refaktoriere diese Daten sauber:

- persönliche Stammdaten bleiben auf `Employee`
- vertragsbezogene Beschäftigungsdaten gehören künftig zu `EmployeeContract`
- vorhandene Daten dürfen durch Migration nicht unnötig verloren gehen
- keine doppelte dauerhaft widersprüchliche Pflege derselben Vertragsdaten

Dokumentiere die konkrete Entscheidung in `docs/DECISIONS.md`.

# Regeln

## ContractNumber

- erforderlich
- tenantlokal eindeutig
- änderbar
- Trim
- sinnvolle Maximallänge

## EmployeeId

- erforderlich
- Employee muss existieren

## Zeitraum

- StartDate erforderlich
- EndDate optional
- `IsPermanent = true` bedeutet grundsätzlich kein reguläres EndDate
- EndDate darf nicht vor StartDate liegen
- ProbationEndDate darf nicht vor StartDate liegen

## Vertragsbedingungen

- EmploymentType frei konfigurierbarer Text wie bisher
- WeeklyHours >= 0
- MonthlyTargetHours >= 0
- VacationDaysPerYear >= 0

Keine Lohn-/Gehaltsfelder in diesem Prompt.

# Mehrere Verträge

Historische Verträge müssen erhalten bleiben.

Verhindere fachlich widersprüchliche gleichzeitig aktive Verträge, sofern die bestehende Domain keinen legitimen Grund dafür besitzt.

Ein neuer Folge-/Änderungsvertrag soll möglich sein, ohne den alten Datensatz zu überschreiben.

Alte Verträge werden beendet/deaktiviert, nicht umgeschrieben.

# Löschen

Ein Vertrag darf physisch nur gelöscht werden, solange er noch keine späteren historischen Referenzen besitzt.

Für den aktuellen Stand ist Löschen unreferenzierter Verträge erlaubt.

Dokumentiere:

Sobald später Arbeitszeiten, Abrechnungen oder andere historische Vorgänge einen Vertrag referenzieren, darf er nur noch historisch beendet/deaktiviert werden.

# Persistenz

Erweitere `CleanificoDbContext`.

Erstelle Fluent Configuration für `EmployeeContract`.

Konfiguriere mindestens:

- Tabelle
- Primärschlüssel
- FK zu Employee
- eindeutige ContractNumber
- Required-Felder
- Längen
- sinnvolle Indizes
- Restrict/NoAction beim Employee-Löschen
- Auditfelder

Erzeuge eine neue EF-Core-Migration.

Bestehende Migrationen nicht verändern.

Falls Beschäftigungsfelder von `Employee` verschoben werden, führe die Schemaänderung und Datenübernahme sauber in dieser Migration durch.

# Application / API

Implementiere:

- GetAll
- GetById
- Create
- Update
- Activate
- Deactivate / End
- Delete

GET-Liste unterstützt mindestens:

- `search`
- `isActive`
- `employeeId`

API bevorzugt:

- GET `/api/employee-contracts`
- GET `/api/employee-contracts/{id}`
- POST `/api/employee-contracts`
- PUT `/api/employee-contracts/{id}`
- POST `/api/employee-contracts/{id}/activate`
- POST `/api/employee-contracts/{id}/deactivate`
- DELETE `/api/employee-contracts/{id}`

Nutze bestehende Validation-, Error- und Contract-Patterns.

Keine Domain-/EF-Entities direkt über HTTP ausgeben.

# Contracts

Mindestens:

- `EmployeeContractResponse`
- `CreateEmployeeContractRequest`
- `UpdateEmployeeContractRequest`

Response soll sinnvolle Employee-Daten enthalten:

- EmployeeId
- EmployeeNumber
- EmployeeName

# Autorisierung

Nutze das bestehende Policy-System.

Bei Bedarf:

- `ViewEmployeeContracts`
- `ManageEmployeeContracts`

Rechte:

- Owner: lesen + verwalten
- Administrator: lesen + verwalten
- Dispatcher: lesen
- ObjectManager: lesen
- Employee: kein administrativer Zugriff

Bestehende Lizenzprüfung bleibt aktiv.

# Web

Erstelle die Office-Seite:

`/mitarbeitervertraege`

Mindestens:

- Suche
- Statusfilter
- Mitarbeiterfilter
- Vertragsliste
- Anlegen
- Bearbeiten
- Deaktivieren/Beenden
- Reaktivieren, sofern fachlich sinnvoll
- Löschen, solange zulässig
- Detailansicht

Spalten mindestens:

- Vertragsnummer
- Mitarbeiter
- Beschäftigungsart
- Beginn
- Ende / Unbefristet
- Wochenstunden
- Urlaubstage
- Status
- Aktionen

Bearbeitungsmaske mindestens:

- Mitarbeiter
- Vertragsnummer
- Beginn
- Ende
- Unbefristet
- Beschäftigungsart
- Wochenstunden
- monatliche Sollstunden
- Urlaubstage pro Jahr
- Probezeit bis
- Notizen

UI-Texte auf Deutsch.

# Mitarbeiterdetail erweitern

Erweitere `/mitarbeiter` bzw. die Mitarbeiterdetailansicht sinnvoll:

- aktuellen Vertrag anzeigen
- Vertragsanzahl anzeigen
- historische Verträge des Mitarbeiters auflisten
- Links zu Vertragsdetails

Kein großes Redesign.

# Tests

Mindestens testen:

- gültiger Vertrag kann angelegt werden
- Employee erforderlich
- unbekannter Employee abgelehnt
- ContractNumber erforderlich/eindeutig
- ungültige Datumsreihenfolge verhindert
- negative Stunden/Urlaubstage verhindert
- Folge-/historische Verträge bleiben erhalten
- widersprüchliche aktive Vertragszeiträume werden verhindert, sofern so modelliert
- Update funktioniert
- Mitarbeiterfilter funktioniert
- Employee mit Vertrag kann nicht physisch gelöscht werden
- Employee ohne Vertrag bleibt löschbar
- Autorisierung 401/403 korrekt
- Lizenzprüfung bleibt wirksam

Bestehende Tests müssen grün bleiben.

# Dokumentation

Nur dauerhaft relevantes Wissen aktualisieren:

- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- ggf. `AGENTS.md`
- ggf. `README.md`

Wichtig dokumentieren:

- Employee = persönliche Stammdaten
- EmployeeContract = historienfähige Beschäftigungsbedingungen
- Employee 1:n EmployeeContracts
- kein Cascade Delete
- Mitarbeiter mit Vertrag nicht physisch löschbar
- ContractNumber tenantlokal eindeutig

# Nicht Bestandteil

Nicht implementieren:

- Gehalt / Stundenlohn
- Lohnabrechnung
- PDF-/Dokumentenablage
- Arbeitszeiten / TimeEntry
- Urlaubsw workflow
- Krankmeldungen
- Objektzuweisungen
- Dienstplanung
- MAUI
- App-Zugang
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

`Reports/YYYY-MM-DD_HH-mm_Prompt-010_Employee-Contracts.md`

Prüfe:

```bash
git status
git diff --stat
git diff --check
```

Nicht automatisch committen oder pushen.

Antworte kompakt mit:

- umgesetzt
- Migration / eventuelle Employee-Refaktorierung
- Historienmodell
- API
- Autorisierung
- Web
- Build
- Tests
- Report
- Git-Status
- Empfehlung für Prompt 011
