# Cleanifico – Codex Prompt 002
## AGENTS.md, Persistenzfundament und Reinigungstypen End-to-End

Du arbeitest im bestehenden Cleanifico-Repository:

```text
~/Documents/Projekte/FergenixLabs/Cleanifico
```

Prompt 001 wurde bereits abgeschlossen.

Aktueller Stand:

- `Cleanifico.slnx`
- sechs Produktprojekte
- vier Testprojekte
- `/docs`
- `/Reports`
- .NET 10.0.102
- Restore erfolgreich
- Build: 0 Warnungen / 0 Fehler
- 8/8 Tests bestanden
- Git-Repository wurde lokal initialisiert und der Stand von Prompt 001 committed
- EF Core, Pomelo, MySQL-Persistenz und Identity sind noch nicht eingerichtet
- es existieren noch keine Businessmodule

Dieser Prompt baut den **ersten echten vertikalen End-to-End-Schnitt** von Cleanifico:

> Verwaltung von Reinigungstypen

Gleichzeitig wird das Persistenzfundament mit Entity Framework Core und MySQL sauber eingerichtet.

---

# 1. Vor Beginn zwingend lesen

Bevor du Code änderst, lies mindestens:

```text
README.md
docs/PROJECT_MEMORY.md
docs/ARCHITECTURE.md
docs/DECISIONS.md
docs/TODO.md
```

Falls bereits eine `AGENTS.md` existiert, lies und beachte sie ebenfalls.

Analysiere anschließend nur die für diesen Auftrag relevanten Projektbereiche. Das komplette Repository soll nicht bei jedem Prompt unnötig vollständig neu analysiert werden.

---

# 2. AGENTS.md im Projekt-Root anlegen

Erstelle im Root:

```text
AGENTS.md
```

Die Datei ist die dauerhafte Arbeitsanweisung für Codex innerhalb des gesamten Cleanifico-Repositories.

Sie soll **kompakt, eindeutig und wartbar** bleiben und mindestens diese Regeln enthalten:

## Projektkontext

- Produktname: Cleanifico
- B2B-Software für Gebäudereinigungsunternehmen
- Web-App für Büro/Administration
- ASP.NET Core API
- später .NET MAUI für Außendienst
- jeder Kunde erhält eine eigene API/Instanz
- jeder Kunde erhält eine eigene MySQL-Datenbank
- Lizenzierung zentral über FergensHub
- Tenant-Auflösung später über die Discovery API

## Architekturregeln

- Domain darf nicht von Infrastructure, API oder Web abhängen.
- Application darf keine konkrete Infrastructure-Implementierung benötigen.
- Contracts enthalten DTOs, Requests und Responses, aber keine EF-Entities.
- Infrastructure implementiert Persistenz und technische Adapter.
- API enthält HTTP-Orchestrierung, aber keine unnötige Businesslogik.
- Web enthält UI-Verhalten, aber keine zentrale Geschäftslogik.
- Keine zyklischen Projektabhängigkeiten.
- Keine neue Architektur einführen, wenn vorhandene Patterns funktionieren.

## Entwicklungsregeln

- Nullable Reference Types beibehalten.
- Moderne C#-Konventionen verwenden.
- Keine Template-Demo-Klassen oder Demo-Endpunkte.
- Kein toter Code.
- Keine unnötigen Packages.
- Keine Breaking Changes ohne zwingenden Grund.
- Keine Geschäftsanforderungen erfinden.
- Fachliche Stammdaten mit Historienbezug bevorzugt deaktivieren statt löschen.
- Technische Audit-Zeitstempel grundsätzlich in UTC.

## Datenbankregeln

- EF Core Fluent Configuration bevorzugen.
- Schemaänderungen über Migrationen verwalten.
- Keine automatische destruktive Schemaerstellung beim Produktionsstart.
- Domain-/EF-Entities niemals direkt als API-Contracts verwenden.
- MySQL-spezifische Entscheidungen dokumentieren.

## Arbeitsablauf für jeden zukünftigen Prompt

1. `AGENTS.md` lesen.
2. Relevante Dateien unter `/docs` lesen.
3. Nur relevante Codebereiche untersuchen.
4. Aufgabe implementieren.
5. Tests ergänzen/aktualisieren.
6. Restore nur wenn nötig.
7. Build ausführen.
8. Tests ausführen.
9. `/docs` nur bei dauerhaft relevantem neuen Wissen aktualisieren.
10. Report unter `/Reports` erzeugen.
11. `git status` prüfen.
12. Benutzer kompakt über Ergebnis und offene Punkte informieren.

## Wissensbasis

`PROJECT_MEMORY.md` ist kein Logbuch. Nur dauerhaft relevantes Wissen aufnehmen. Historische Arbeitsdetails gehören in `/Reports`.

## Reports

Nach jedem abgeschlossenen Benutzerauftrag muss ein eigener Markdown-Report unter `/Reports` angelegt werden.

## Git

- Bestehende Commits nicht verändern.
- Keine History-Rewrites.
- Keine Branches ohne Auftrag.
- Nicht automatisch committen.
- Nicht automatisch pushen.
- Am Ende `git status` melden.

Die `AGENTS.md` soll nicht zu einer zweiten riesigen Projektdokumentation anwachsen. Detailwissen gehört in `/docs`.

---

# 3. Ziel des ersten Businessmoduls

Ein **Reinigungstyp** beschreibt eine fachliche Kategorie von Reinigungsleistungen.

Beispiele:

```text
Unterhaltsreinigung
Grundreinigung
Glasreinigung
Sonderreinigung
Baureinigung
Teppichreinigung
Außenreinigung
Desinfektionsreinigung
```

Der Cleanifico-Kunde muss eigene Reinigungstypen verwalten können.

Prompt 002 umfasst:

- anlegen
- anzeigen
- bearbeiten
- deaktivieren
- reaktivieren
- löschen, wenn fachlich zulässig
- suchen
- nach Status filtern
- sortieren

Noch keine Zuordnung zu Objekten, Verträgen, Einsätzen oder Mitarbeitern implementieren.

---

# 4. Domain-Modell

Erstelle eine fachlich saubere Entity:

```csharp
CleaningType
```

Mindestens benötigte Informationen:

```text
Id
Name
Code
Description
IsActive
SortOrder
CreatedAtUtc
UpdatedAtUtc
```

## Regeln

### Id

Verwende einen zum Projekt passenden Schlüsseltyp. Keine unnötig exotische ID-Strategie einführen.

### Name

- erforderlich
- nach Trim nicht leer
- sinnvolle Maximallänge
- innerhalb der Tenant-Datenbank eindeutig

### Code

Beispiele:

```text
UR
GR
GL
SR
```

Regeln:

- erforderlich
- kurze menschenlesbare Kennung
- Trim
- normalisierte Schreibweise, bevorzugt Großbuchstaben
- innerhalb der Tenant-Datenbank eindeutig

### Description

- optional
- sinnvolle Maximallänge

### IsActive

Neue Reinigungstypen sind standardmäßig aktiv.

### SortOrder

Ganzzahl für die UI-Sortierung. Keine negativen Werte zulassen, sofern kein fachlicher Grund besteht.

### Auditfelder

```text
CreatedAtUtc
UpdatedAtUtc
```

Technische Zeitstempel in UTC. Noch keine vollständige Audit-History bauen.

---

# 5. Löschregeln

Aktuell existieren noch keine Referenzen von Objekten, Verträgen oder Einsätzen auf `CleaningType`.

Daher darf ein unreferenzierter Reinigungstyp aktuell physisch gelöscht werden.

Architekturregel für später:

> Sobald ein Reinigungstyp von Objekten, Verträgen, Einsätzen oder historischen Daten referenziert wird, darf er nicht mehr physisch gelöscht werden. Dann ist Deaktivierung der normale Weg.

Diese Entscheidung in `docs/DECISIONS.md` dokumentieren.

Keine künstlichen Fremdschlüssel auf noch nicht existierende Module erzeugen.

---

# 6. Persistenzfundament

Richte jetzt die echte Persistenzschicht ein.

Verwende:

- Entity Framework Core
- passenden stabilen Pomelo MySQL Provider
- MySQL
- Paketversionen, die zum tatsächlich verwendeten .NET-/EF-Core-Stand passen

Prüfe die Kompatibilität vor Installation. Keine Preview-Pakete verwenden, wenn stabile kompatible Versionen verfügbar sind.

---

# 7. CleanificoDbContext

Erstelle in `Cleanifico.Infrastructure` einen produktionsgeeigneten DbContext:

```csharp
CleanificoDbContext
```

Er muss mindestens `CleaningType` persistieren können.

Sinngemäß:

```text
DbSet<CleaningType> CleaningTypes
```

Keine Businesslogik in den DbContext legen.

---

# 8. EF-Core-Konfiguration

Konfiguriere `CleaningType` über eine eigene Fluent-API-Konfigurationsklasse, z. B.:

```text
CleaningTypeConfiguration
```

Konfiguriere mindestens:

- Tabelle
- Primärschlüssel
- Name
- Code
- Description
- IsActive
- SortOrder
- CreatedAtUtc
- UpdatedAtUtc
- Feldlängen
- Indizes
- Eindeutigkeit für Name
- Eindeutigkeit für Code

Da jeder Cleanifico-Kunde eine eigene Datenbank erhält, ist eine zusätzliche `TenantId` auf jeder Business-Entity zunächst **nicht erforderlich**, sofern die bestehende Architektur nichts anderes verlangt.

Diese Entscheidung dokumentieren.

---

# 9. Datenbankkonfiguration

Konfiguriere die API so, dass der Connection String über Configuration geladen wird, bevorzugt:

```text
ConnectionStrings:Cleanifico
```

Keine echten Zugangsdaten committen.

Secrets gehören nicht ins Repository.

Eine ungefährliche Beispielkonfiguration darf in README oder einer Sample-Konfiguration dokumentiert werden.

---

# 10. Initiale Migration

Erstelle eine echte EF-Core-Migration für den aktuellen Persistenzstand.

Sinnvoller Name z. B.:

```text
InitialCleanificoPersistence
```

Die Migration muss die Tabelle für Reinigungstypen korrekt erzeugen.

Prüfe die erzeugte Migration auf offensichtliche Fehler.

Keine vorhandenen fremden Datenbanken löschen oder verändern.

---

# 11. Migrationen beim Start

Die Anwendung darf beim normalen Produktionsstart nicht ungefragt destruktive Datenbankaktionen durchführen.

Falls automatische Migrationen im Development-Modus sinnvoll eingebaut werden, muss das Verhalten klar getrennt und dokumentiert sein.

Produktive Tenant-Migrationen müssen später kontrollierbar bleiben.

---

# 12. Application Layer

Die API darf nicht direkt Businesslogik im DbContext oder in Endpoints verteilen.

Erzeuge geeignete Application-Abstraktionen und einen einfachen testbaren Application-Service/Use-Case für Reinigungstypen.

Benötigte Operationen:

```text
GetAll
GetById
Create
Update
Activate
Deactivate
Delete
```

Zusätzlich:

- Suche nach Name oder Code
- optionaler Statusfilter
- Standardsortierung nach `SortOrder`, danach `Name`

Keine unnötig komplexe CQRS-/Mediator-Infrastruktur einführen, sofern diese im Projekt noch nicht existiert.

---

# 13. Contracts

Erstelle getrennte HTTP-/API-Contracts, z. B.:

```text
CleaningTypeResponse
CreateCleaningTypeRequest
UpdateCleaningTypeRequest
```

Keine Domain-/EF-Entity direkt über HTTP ausgeben.

Requests sollen nur die jeweils tatsächlich änderbaren Felder enthalten.

---

# 14. Serverseitige Validierung

Mindestens validieren:

- Name erforderlich
- Code erforderlich
- Maximallängen
- SortOrder gültig
- Name eindeutig
- Code eindeutig
- Whitespace normalisieren

Bei Konflikten verständliche Fehler zurückgeben.

Validierung darf nicht ausschließlich im Frontend stattfinden.

---

# 15. REST API

Implementiere bevorzugt:

```http
GET    /api/cleaning-types
GET    /api/cleaning-types/{id}
POST   /api/cleaning-types
PUT    /api/cleaning-types/{id}
POST   /api/cleaning-types/{id}/activate
POST   /api/cleaning-types/{id}/deactivate
DELETE /api/cleaning-types/{id}
```

Die Listenroute soll mindestens unterstützen:

```text
search
isActive
```

Beispiel:

```http
GET /api/cleaning-types?search=glas&isActive=true
```

Passende Statuscodes verwenden, u. a.:

- `200 OK`
- `201 Created`
- `204 No Content`
- `400 Bad Request`
- `404 Not Found`
- `409 Conflict`

Keine internen Stacktraces oder Exception-Details an Clients ausgeben.

---

# 16. Identity noch nicht vortäuschen

Falls noch keine echte Authentifizierung vorhanden ist:

- kein Fake-Security-System bauen
- keine Scheinsicherheit erzeugen
- dokumentieren, dass die Endpunkte vor Produktiveinsatz abgesichert werden müssen
- `TODO.md` entsprechend ergänzen

Identity kommt in einer eigenen späteren Phase.

---

# 17. FergensHub-Lizenzierung

Cleanifico muss später wie Assetfico über FergensHub lizenziert werden.

In Prompt 002 noch keine erfundene HTTP-Integration zu FergensHub bauen, solange reale Contracts/Endpoints hier nicht vorliegen.

Aber:

- Architekturpunkt erhalten
- in `ARCHITECTURE.md` dokumentieren
- in `TODO.md` als zwingenden Pre-Production-Punkt führen
- keine konkurrierende lokale Lizenzarchitektur einführen

---

# 18. Web-App – Reinigungstypen

Baue die erste echte Cleanifico-Office-Seite.

Route:

```text
/reinigungstypen
```

Die Oberfläche soll professionell, modern, übersichtlich und desktoporientiert sein und sich in die bestehende Cleanifico-Web-Hülle einfügen.

Keine große UI-Bibliothek nur für dieses Modul hinzufügen.

## Seitenkopf

```text
Reinigungstypen
Verwalten Sie die Reinigungsarten Ihres Unternehmens.
```

Aktion:

```text
+ Reinigungstyp anlegen
```

## Suche und Filter

```text
Reinigungstyp suchen...
```

Status:

```text
Alle
Aktiv
Inaktiv
```

## Tabelle/Liste

Mindestens:

```text
Kürzel
Name
Beschreibung
Sortierung
Status
Aktionen
```

Beispieldarstellung:

```text
UR | Unterhaltsreinigung | Regelmäßige Reinigung | 10 | Aktiv
GR | Grundreinigung      | Intensive Reinigung   | 20 | Aktiv
GL | Glasreinigung       | Glas und Fenster       | 30 | Aktiv
```

---

# 19. Anlegen und Bearbeiten

Anlegen und Bearbeiten sollen über eine gute Benutzerführung erfolgen, z. B. Dialog, Drawer oder eigene Bearbeitungsseite – passend zur bestehenden Web-Struktur.

Felder:

```text
Name
Kürzel
Beschreibung
Sortierung
```

Bei Validierungsfehlern:

- Feld markieren
- verständliche deutsche Fehlermeldung
- Benutzereingaben nicht unnötig verlieren

---

# 20. Deaktivieren und Reaktivieren

Deaktivieren darf nicht wie Löschen wirken.

Vor Deaktivierung eine verständliche Bestätigung anzeigen, z. B.:

```text
Möchten Sie den Reinigungstyp „Unterhaltsreinigung“ wirklich deaktivieren?
```

Inaktive Reinigungstypen bleiben über den Statusfilter sichtbar.

Reaktivierung muss möglich sein.

---

# 21. Löschen

Löschen erhält eine deutliche Bestätigung.

Die UI muss klar unterscheiden zwischen:

```text
Deaktivieren
```

und

```text
Endgültig löschen
```

Aktuell dürfen nur unreferenzierte Reinigungstypen endgültig gelöscht werden.

---

# 22. Seed-Daten

Keine festen Demo-Reinigungstypen ungefragt in jede produktive Datenbank schreiben.

Testdaten gehören ausschließlich in Tests.

Development-Demodaten sind nur zulässig, wenn sie ausdrücklich Development-only sind und klar dokumentiert werden.

---

# 23. Fehlerbehandlung im Web

Die Web-App soll Backend-/API-Fehler sinnvoll behandeln.

Beispiele:

- Laden fehlgeschlagen
- Speichern fehlgeschlagen
- Kürzel bereits vorhanden
- Name bereits vorhanden
- Datensatz existiert nicht mehr

Keine rohen Stacktraces im UI.

---

# 24. Tests

Prompt 002 benötigt substanzielle Tests.

## Domain/Application

Mindestens testen:

- gültiger Reinigungstyp kann angelegt werden
- leerer/ungültiger Name wird abgelehnt
- leerer/ungültiger Code wird abgelehnt
- Code wird normalisiert
- doppelter Name wird verhindert
- doppelter Code wird verhindert
- Update funktioniert
- Deaktivierung funktioniert
- Reaktivierung funktioniert
- Suche/Filter funktionieren sinnvoll

## Infrastructure

EF-Core-Konfiguration sinnvoll testen.

Wenn echte MySQL-Integrationstests lokal zuverlässig möglich sind, können sie verwendet werden.

Wenn dafür Infrastruktur fehlt, keine Tests bauen, die nur vortäuschen MySQL zu testen. Grenze sauber im Report dokumentieren.

## API

Mindestens testen:

- GET-Liste
- GET by id
- Create
- Update
- Activate
- Deactivate
- Delete
- Not Found
- Validation
- Duplicate Conflict

Keine wertlosen `Assert.True(true)`-Tests.

---

# 25. Schutz der Testdaten

Tests dürfen niemals versehentlich eine normale Entwicklungs- oder produktive Datenbank verändern.

Bei Integrationstests:

- eigener Testdatenbankname
- isolierte Konfiguration
- sichere Bereinigung

Wenn dies nicht zuverlässig möglich ist, keine entsprechenden Tests vortäuschen.

---

# 26. Build-Qualität

Nach Implementierung mindestens ausführen:

```bash
dotnet restore
dotnet build
dotnet test
```

Ziel:

```text
0 Fehler
0 Warnungen
alle Tests grün
```

Neue Analyzer-Warnungen nach Möglichkeit beheben statt ignorieren.

---

# 27. Dokumentation aktualisieren

Nach Abschluss prüfen und bei Bedarf aktualisieren:

```text
AGENTS.md
README.md
docs/PROJECT_MEMORY.md
docs/ARCHITECTURE.md
docs/DECISIONS.md
docs/TODO.md
```

## PROJECT_MEMORY.md

Nur dauerhaft relevantes neues Wissen aufnehmen, z. B.:

- `CleaningType` existiert
- DbContext-Name
- Connection-String-Key
- API-Routen
- Persistenzpattern

Keine komplette Änderungshistorie hineinkopieren.

## ARCHITECTURE.md

Persistenzarchitektur und Datenfluss ergänzen, sinngemäß:

```text
Cleanifico.Web
      |
      v
Cleanifico.Api
      |
      v
Cleanifico.Application
      |
      v
Abstraktionen
      |
      v
Cleanifico.Infrastructure
      |
      v
EF Core / MySQL
```

## DECISIONS.md

Mindestens dokumentieren:

- EF Core + Pomelo/MySQL
- eigene DB pro Tenant, daher zunächst keine TenantId auf jeder Business-Entity
- CleaningTypes deaktivierbar
- physisches Löschen nur solange unreferenziert
- API-Contracts getrennt von Domain-Entities

---

# 28. Report für Prompt 002

Erstelle:

```text
Reports/YYYY-MM-DD_HH-mm_Prompt-002_Cleaning-Types.md
```

Inhalt mindestens:

```markdown
# Report – Prompt 002

## Auftrag
## Vorheriger Stand
## Analyse
## Implementierung
## Neu erstellte Dateien
## Geänderte Dateien
## Datenbank / Migrationen
## API
## Web
## Tests
## Build
## Architekturentscheidungen
## Sicherheits-/Lizenzstatus
## Bekannte Einschränkungen
## Offene Punkte
## Aktualisierte Wissensdateien
## Git-Status
```

---

# 29. Git-Regeln für diesen Prompt

Der Benutzer hat den Stand von Prompt 001 bereits lokal committed.

Daher:

- keine bestehende Git-History verändern
- kein Force Push
- keinen Push ausführen
- keinen neuen Branch erzeugen
- nicht automatisch committen

Git nur zur Kontrolle verwenden:

```bash
git status
git diff
```

Am Ende klar angeben, welche Änderungen noch uncommitted sind.

---

# 30. Nicht Bestandteil von Prompt 002

Noch **nicht** implementieren:

- Mitarbeiter
- Kunden
- Objekte
- Mitarbeiterverträge
- Objektverträge
- Zeittypen
- Arbeitszeiten
- Einsatzplanung
- Dienstplanung
- MAUI
- Offline Sync
- Qualitätsmanagement
- Reklamationen
- Lager
- Schlüsselverwaltung
- Rechnungen
- Kundenportal
- vollständiges Identity-System
- erfundene FergensHub-Integration

Keine Feature-Ausweitung.

---

# 31. Definition of Done

Prompt 002 ist erst abgeschlossen, wenn:

- `AGENTS.md` vorhanden und sinnvoll gefüllt ist
- EF-Core-/MySQL-Persistenzfundament eingerichtet ist
- `CleanificoDbContext` existiert
- eine gültige Migration existiert
- `CleaningType` vollständig modelliert ist
- Application-Funktionalität vorhanden ist
- API CRUD + Aktivieren/Deaktivieren funktioniert
- Web-Verwaltung für Reinigungstypen vorhanden ist
- serverseitige Validierung funktioniert
- substanzielle Tests vorhanden sind
- Restore erfolgreich ist
- Build ohne Fehler ist
- Tests erfolgreich sind
- Dokumentation aktuell ist
- Prompt-002-Report existiert
- Git-Status geprüft wurde

---

# 32. Abschlussantwort von Codex

Antworte nach Abschluss kompakt auf Deutsch mit:

1. Dauer
2. was implementiert wurde
3. wichtigste Architekturentscheidungen
4. Migration-/Datenbankstatus
5. API-Status
6. Web-Status
7. Build-Ergebnis
8. Testergebnis
9. Sicherheits-/Lizenzhinweis
10. Pfad zum Report
11. Git-Status
12. Empfehlung für Prompt 003

Keine riesigen Codeblöcke in die Abschlussantwort kopieren.

Details gehören in Code, `/docs` und `/Reports`.
