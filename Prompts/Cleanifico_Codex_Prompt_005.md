# Cleanifico – Codex Prompt 005
## Kundenverwaltung End-to-End

Arbeite im bestehenden Repository:

`~/Documents/Projekte/FergenixLabs/Cleanifico`

Nutze die vorhandenen Regeln und Patterns aus `AGENTS.md` sowie den relevanten Dateien unter `docs/`. Analysiere nur die für diesen Auftrag relevanten Bereiche. Verwende bestehende Patterns aus `CleaningType`, `TimeType`, Identity und Authorization.

# Ziel

Implementiere ein vollständiges Modul für **Kunden**.

Ein Kunde ist der Auftraggeber des Reinigungsunternehmens und kann später mehrere Objekte besitzen. Noch keine Objekt-Entity implementieren.

# Entity

Bevorzugter Name: `Customer`

Mindestens:
- Id
- CustomerNumber
- CompanyName
- ContactFirstName
- ContactLastName
- Email
- Phone
- Street
- PostalCode
- City
- Country
- Notes
- IsActive
- CreatedAtUtc
- UpdatedAtUtc

Optional nur wenn sauber begründet: MobilePhone, Website.

# Regeln

`CustomerNumber`
- erforderlich
- tenantlokal eindeutig
- vom Benutzer änderbar
- Trim
- sinnvolle Maximallänge

`CompanyName`
- erforderlich
- Trim
- sinnvolle Maximallänge

Ansprechpartner darf zunächst direkt am Kunden liegen. Noch kein separates Contact-Modul.

Adresse ist die Kunden-/Verwaltungsadresse. Spätere Objektadressen gehören an `Object`.

Neue Kunden sind standardmäßig aktiv und können deaktiviert/reaktiviert werden.

# Löschen

Solange ein Kunde noch keine fachlichen Referenzen besitzt, darf physisches Löschen erlaubt sein.

Dokumentiere jedoch:
Sobald ein Kunde später Objekte, Verträge, Rechnungen oder historische Daten besitzt, darf er nicht mehr physisch gelöscht werden. Dann nur deaktivieren.

Noch keine künstlichen Fremdschlüssel auf nicht vorhandene Module bauen.

# Persistenz

Erweitere `CleanificoDbContext` und erstelle Fluent Configuration für `Customer`.

Konfiguriere mindestens:
- Tabelle
- Primärschlüssel
- Required-Felder
- Längen
- eindeutige CustomerNumber
- sinnvolle Indizes
- Auditfelder

Erzeuge eine neue EF-Core-Migration. Bestehende Migrationen nicht verändern.

# Application / API

Implementiere analog zu bestehenden Modulen:
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
- CustomerNumber
- CompanyName
- ContactFirstName
- ContactLastName
- City

API:
- GET `/api/customers`
- GET `/api/customers/{id}`
- POST `/api/customers`
- PUT `/api/customers/{id}`
- POST `/api/customers/{id}/activate`
- POST `/api/customers/{id}/deactivate`
- DELETE `/api/customers/{id}`

Nutze bestehende Error-, Validation- und Contract-Patterns. Keine Domain-/EF-Entities direkt über HTTP ausgeben.

# Contracts

Mindestens:
- `CustomerResponse`
- `CreateCustomerRequest`
- `UpdateCustomerRequest`

# Autorisierung

Nutze das bestehende Rollen-/Policy-System. Bei Bedarf:
- `ViewCustomers`
- `ManageCustomers`

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

`/kunden`

Optisch am bestehenden Cleanifico-Stil orientieren.

Mindestens:
- Suche
- Statusfilter
- Kundenliste/Tabelle
- Anlegen
- Bearbeiten
- Deaktivieren
- Reaktivieren
- Löschen, solange zulässig

Spalten:
- Kundennummer
- Firmenname
- Ansprechpartner
- Ort
- Kontakt
- Status
- Aktionen

Bearbeitungsmaske:
- Kundennummer
- Firmenname
- Vorname Ansprechpartner
- Nachname Ansprechpartner
- E-Mail
- Telefon
- Straße
- PLZ
- Ort
- Land
- Notizen

UI-Texte auf Deutsch.

# Detailansicht

Baue eine einfache, saubere Kundendetailansicht oder einen Detailbereich.

Mindestens:
- Stammdaten
- Kontakt
- Adresse
- Status

Noch keine Fake-Tabs für Objekte, Verträge oder Rechnungen bauen.

# Validierung

Mindestens:
- CustomerNumber erforderlich
- CustomerNumber eindeutig
- CompanyName erforderlich
- gültiges E-Mail-Format, wenn angegeben
- sinnvolle Maximallängen
- Whitespace normalisieren

Verständliche deutsche Fehlermeldungen.

# Tests

Mindestens testen:
- gültiger Kunde kann angelegt werden
- CustomerNumber erforderlich
- CompanyName erforderlich
- doppelte CustomerNumber verhindert
- Update funktioniert
- Suche funktioniert
- Statusfilter funktioniert
- Aktivieren/Deaktivieren funktioniert
- Löschen unreferenzierter Kunden funktioniert
- anonym -> 401
- Owner/Admin dürfen verwalten
- Dispatcher/ObjectManager dürfen lesen, aber nicht schreiben
- Employee darf administrativ nicht zugreifen

Bestehende Tests müssen grün bleiben. Security nicht für Tests deaktivieren.

# Dokumentation

Nur bei dauerhaft relevantem Wissen aktualisieren:
- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- `AGENTS.md`
- `README.md`

Wichtig dokumentieren:
- Customer ist Auftraggeber
- Customer kann später mehrere Objects besitzen
- physisches Löschen nur solange unreferenziert
- CustomerNumber tenantlokal eindeutig

# Nicht Bestandteil

Nicht implementieren:
- Objekte
- Verträge
- Rechnungen
- Angebote
- separates Ansprechpartner-Modul
- Mitarbeiter
- Arbeitszeiten
- Dienstplanung
- MAUI
- FergensHub
- Discovery
- MFA
- Kundenportal

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

`Reports/YYYY-MM-DD_HH-mm_Prompt-005_Customers.md`

Prüfe:
```bash
git status
git diff --stat
```

Nicht automatisch committen oder pushen.

Antworte kompakt mit:
- umgesetzt
- Migration
- API
- Autorisierung
- Web
- Build
- Tests
- Report
- Git-Status
- Empfehlung für Prompt 006
