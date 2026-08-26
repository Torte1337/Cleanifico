# Cleanifico – Codex Prompt 006
## Objektverwaltung End-to-End

Arbeite im bestehenden Repository:

`~/Documents/Projekte/FergenixLabs/Cleanifico`

Nutze die vorhandenen Regeln und Patterns aus `AGENTS.md` sowie den relevanten Dateien unter `docs/`.
Analysiere nur die für diesen Auftrag relevanten Bereiche.

Verwende bestehende Patterns aus:
- Customer
- CleaningType
- TimeType
- Identity
- Authorization

# Ziel

Implementiere ein vollständiges Modul für **Objekte**.

Ein Objekt ist ein konkreter Reinigungsstandort und gehört zwingend zu genau einem `Customer`.

Ein Kunde kann mehrere Objekte besitzen.

# Entity

Bevorzugter Name:

`CleaningObject`

Verwende **nicht** einfach `Object` als Klassenname.

Mindestens:
- Id
- ObjectNumber
- CustomerId
- Name
- Street
- PostalCode
- City
- Country
- ContactFirstName
- ContactLastName
- ContactEmail
- ContactPhone
- AccessNotes
- CleaningNotes
- IsActive
- CreatedAtUtc
- UpdatedAtUtc

Optional nur wenn sinnvoll:
- BuildingAreaSquareMeters
- Floors

Keine unnötigen Felder hinzufügen.

# Regeln

`ObjectNumber`
- erforderlich
- tenantlokal eindeutig
- vom Benutzer änderbar
- Trim
- sinnvolle Maximallänge

`CustomerId`
- erforderlich
- muss auf existierenden Customer zeigen
- Objekt darf nicht ohne Customer existieren

`Name`
- erforderlich
- Trim
- sinnvolle Maximallänge

Die Objektadresse ist unabhängig von der Verwaltungsadresse des Kunden.

Kontaktinformationen dürfen zunächst direkt am Objekt liegen.

Neue Objekte sind standardmäßig aktiv und können deaktiviert/reaktiviert werden.

# Beziehung Customer -> CleaningObject

Implementiere die echte 1:n-Beziehung:

`Customer 1 -> n CleaningObjects`

EF Core soll einen echten Foreign Key besitzen.

Löschverhalten:

> Ein Customer mit mindestens einem referenzierten CleaningObject darf nicht physisch gelöscht werden.

Der bestehende Customer-Löschworkflow muss entsprechend erweitert werden.

Bei einem Löschversuch eines referenzierten Customers soll eine verständliche fachliche Fehlermeldung bzw. `409 Conflict` entstehen.

Keine Cascade-Löschung von Objekten beim Löschen eines Customers.

# Löschen von Objekten

Solange ein Objekt noch keine weiteren fachlichen Referenzen besitzt, darf physisches Löschen erlaubt sein.

Dokumentiere:

Sobald später Verträge, Einsätze, Arbeitszeiten, Qualitätskontrollen oder historische Daten auf ein Objekt verweisen, darf es nicht mehr physisch gelöscht werden.

Dann nur deaktivieren.

Noch keine künstlichen Referenzen auf zukünftige Module bauen.

# Persistenz

Erweitere `CleanificoDbContext`.

Erstelle Fluent Configuration für `CleaningObject`.

Konfiguriere mindestens:
- Tabelle
- Primärschlüssel
- Foreign Key zu Customer
- Required-Felder
- Längen
- eindeutige ObjectNumber
- sinnvolle Indizes
- Auditfelder
- Restrict/NoAction beim Customer-Löschen

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
- `customerId`

`search` soll mindestens über:
- ObjectNumber
- Name
- City
- ContactFirstName
- ContactLastName
- Customer CompanyName

suchen.

API:
- GET `/api/objects`
- GET `/api/objects/{id}`
- POST `/api/objects`
- PUT `/api/objects/{id}`
- POST `/api/objects/{id}/activate`
- POST `/api/objects/{id}/deactivate`
- DELETE `/api/objects/{id}`

Nutze bestehende Validation-, Error- und Contract-Patterns.
Keine Domain-/EF-Entities direkt über HTTP ausgeben.

# Contracts

Mindestens:
- `CleaningObjectResponse`
- `CreateCleaningObjectRequest`
- `UpdateCleaningObjectRequest`

Response soll mindestens enthalten:
- CustomerId
- CustomerNumber
- CustomerCompanyName

# Autorisierung

Nutze das bestehende Rollen-/Policy-System.

Bei Bedarf:
- `ViewObjects`
- `ManageObjects`

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

`/objekte`

Optisch am bestehenden Cleanifico-Stil orientieren.

Mindestens:
- Suche
- Statusfilter
- Kundenfilter
- Objektliste
- Anlegen
- Bearbeiten
- Deaktivieren
- Reaktivieren
- Löschen, solange zulässig

Spalten mindestens:
- Objektnummer
- Objektname
- Kunde
- Ort
- Ansprechpartner
- Status
- Aktionen

Bearbeitungsmaske mindestens:
- Kunde
- Objektnummer
- Name
- Straße
- PLZ
- Ort
- Land
- Vorname Ansprechpartner
- Nachname Ansprechpartner
- E-Mail
- Telefon
- Zugangshinweise
- Reinigungshinweise

Kundenauswahl muss aus echten aktiven Customers kommen.

Keine freie Texteingabe für CustomerId.

# Detailansicht

Erstelle eine echte Objekt-Detailansicht oder einen Detailbereich.

Mindestens anzeigen:
- Stammdaten
- zugehöriger Kunde
- Objektadresse
- Ansprechpartner
- Zugangshinweise
- Reinigungshinweise
- Status

Optional darf der Kunde verlinkt werden.

Noch keine Fake-Inhalte für Verträge, Mitarbeiter, Reinigungstypen, Schlüssel, Leistungen, Einsätze oder Qualität bauen.

# Customer-Webseite erweitern

Erweitere die vorhandene Kundendetailansicht sinnvoll:
- Anzahl der zugeordneten Objekte anzeigen
- echte Objektliste des Kunden anzeigen, wenn architektonisch passend
- Link zum jeweiligen Objekt

Kein großes Redesign.

# Validierung

Mindestens:
- ObjectNumber erforderlich
- ObjectNumber eindeutig
- Name erforderlich
- Customer erforderlich
- Customer muss existieren
- E-Mail gültig, wenn angegeben
- sinnvolle Maximallängen
- Whitespace normalisieren

# Tests

Mindestens testen:
- gültiges Objekt kann angelegt werden
- Customer ist erforderlich
- unbekannter Customer wird abgelehnt
- ObjectNumber erforderlich
- Name erforderlich
- doppelte ObjectNumber verhindert
- Update funktioniert
- Customer-Zuordnung kann geändert werden
- Suche funktioniert
- Customer-Filter funktioniert
- Aktivieren/Deaktivieren funktioniert
- unreferenziertes Objekt kann gelöscht werden
- Customer mit Objekt kann nicht gelöscht werden
- Customer ohne Objekt kann weiterhin gelöscht werden
- anonym -> 401
- Owner/Admin dürfen verwalten
- Dispatcher/ObjectManager dürfen lesen, aber nicht schreiben
- Employee kein administrativer Zugriff

Bestehende Tests müssen grün bleiben.
Security nicht für Tests deaktivieren.

# Dokumentation

Nur dauerhaft relevantes Wissen aktualisieren:
- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- `AGENTS.md`
- `README.md`

Wichtig dokumentieren:
- `CleaningObject` gehört zwingend zu `Customer`
- Customer 1:n CleaningObjects
- kein Cascade Delete
- Customer mit Objekten kann nicht physisch gelöscht werden
- ObjectNumber tenantlokal eindeutig

# Nicht Bestandteil

Nicht implementieren:
- Verträge
- Reinigungstyp-Zuordnung zum Objekt
- Mitarbeiter-Zuordnung
- Leistungsverzeichnisse
- Schlüssel
- Arbeitszeiten
- Einsatzplanung
- Qualitätsmanagement
- Dokumente
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

`Reports/YYYY-MM-DD_HH-mm_Prompt-006_Objects.md`

Prüfe:

```bash
git status
git diff --stat
```

Nicht automatisch committen oder pushen.

Antworte kompakt mit:
- umgesetzt
- Migration
- Customer-Beziehung
- Customer-Löschschutz
- API
- Autorisierung
- Web
- Build
- Tests
- Report
- Git-Status
- Empfehlung für Prompt 007
