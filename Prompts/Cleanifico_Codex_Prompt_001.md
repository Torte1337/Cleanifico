# Cleanifico – Codex Prompt 001
## Projektstart, Architektur, Wissensbasis und Arbeitsregeln

Du arbeitest ab jetzt am Projekt **Cleanifico**.

## Ausgangssituation

Es existiert **noch kein Cleanifico-Projekt und keine Solution-Struktur**.

Auf dem MacBook existiert lediglich dieser leere Arbeitsordner:

```text
~/Documents/Projekte/FergenixLabs/Cleanifico
```

bzw. im Finder:

```text
Dokumente > Projekte > FergenixLabs > Cleanifico
```

**Dieser Ordner ist das Root-Verzeichnis des gesamten Cleanifico-Repositories.**

Alle Dateien, Projekte, Solutions, Dokumentationen, Tests und späteren Deployment-Dateien müssen innerhalb dieses Ordners aufgebaut werden.

Es darf nicht davon ausgegangen werden, dass bereits Code, Projektdateien, Git-Struktur, EF-Core-Konfiguration, Datenbankcode oder sonstige Cleanifico-Bestandteile existieren.

Cleanifico soll eine professionelle, mandantenfähige Software für Gebäudereinigungsunternehmen werden.

Das Produkt besteht langfristig aus:

- einer **Web-App für das Büro**
- einer **ASP.NET Core API**
- einer **.NET MAUI App für Mitarbeiter im Außendienst**
- einer **eigenen MySQL-Datenbank pro Kunde/Tenant**
- einer Lizenzierung und Produktverwaltung über **FergensHub**
- einer Tenant-/Endpoint-Auflösung über die bestehende **Discovery API**

Cleanifico soll später kommerziell als SaaS-/Lizenzprodukt angeboten werden.

---

# 1. Wichtige Arbeitsregel

Arbeite **nicht blind los**.

Bevor du größere Änderungen machst:

1. Analysiere die vorhandene Solution und Repository-Struktur.
2. Prüfe vorhandene Patterns, Namenskonventionen und Architekturentscheidungen.
3. Vermeide unnötige neue Abhängigkeiten.
4. Bevorzuge bestehende Patterns gegenüber neuen Eigenlösungen.
5. Schreibe produktionsnahen, wartbaren und testbaren Code.
6. Keine Platzhalterimplementierungen, sofern sie nicht ausdrücklich als solche markiert und begründet werden.
7. Keine Features erfinden, die in diesem Prompt nicht verlangt werden.
8. Bestehenden Code nicht unnötig umbauen.
9. Keine Breaking Changes ohne zwingenden Grund.
10. Änderungen müssen nachvollziehbar dokumentiert werden.

---

# 2. Projektwissen dauerhaft speichern

Erstelle im Repository einen Ordner:

```text
/docs
```

Darin sollen folgende Dateien existieren:

```text
/docs/PROJECT_MEMORY.md
/docs/ARCHITECTURE.md
/docs/DECISIONS.md
/docs/TODO.md
```

Diese Dateien dienen als **dauerhafte Wissensbasis für Codex**.

Ziel ist, dass bei späteren Aufgaben nicht unnötig das komplette Projekt erneut analysiert werden muss.

## PROJECT_MEMORY.md

Enthält kompakt:

- Zweck von Cleanifico
- wichtige fachliche Begriffe
- aktuelle Module
- bekannte Geschäftsregeln
- vorhandene Services
- wichtige Klassen/Entities
- relevante Ordner
- zentrale Abhängigkeiten
- bekannte technische Besonderheiten
- aktuelle Projektkonventionen

Die Datei soll kompakt bleiben.

Keine langen Verlaufsberichte hineinschreiben.

Nur Wissen aufnehmen, das bei zukünftigen Aufgaben wahrscheinlich erneut benötigt wird.

---

## ARCHITECTURE.md

Dokumentiert die aktuelle technische Architektur.

Unter anderem:

- Solution-Struktur
- Projekte
- Abhängigkeiten zwischen Projekten
- Domain Layer
- Application Layer
- Infrastructure Layer
- API
- Web-App
- später MAUI
- Datenbank
- Authentifizierung
- Tenant-Konzept
- Discovery
- FergensHub-Lizenzierung
- Storage
- Hintergrunddienste
- wichtige Datenflüsse

Wenn Architekturentscheidungen geändert werden, muss diese Datei aktualisiert werden.

---

## DECISIONS.md

Hier werden wichtige Architektur- und Produktentscheidungen als kurze Decision Records dokumentiert.

Beispiel:

```markdown
## DEC-001 – Eigene Datenbank pro Tenant

Status: Accepted

Entscheidung:
Jeder Cleanifico-Kunde erhält eine eigene MySQL-Datenbank und eine eigene API-/Tenant-Instanz.

Grund:
- bessere Isolation
- vereinfachte Backups
- einfachere Kundenmigration
- geringeres Risiko tenantübergreifender Datenzugriffe

Datum:
2026-08-25
```

Nur echte relevante Entscheidungen aufnehmen.

---

## TODO.md

Enthält offene technische und fachliche Aufgaben.

Format beispielsweise:

```markdown
## Offen

- [ ] Mitarbeiterverwaltung
- [ ] Objektverwaltung

## Später

- [ ] MAUI Offline-Synchronisierung
```

Erledigte Punkte dürfen entfernt oder in einen kurzen Abschnitt `Erledigt` verschoben werden.

Die Datei darf nicht zu einem vollständigen Arbeitsprotokoll anwachsen.

---

# 3. Reports nach jedem Prompt

Erstelle im Repository zusätzlich:

```text
/Reports
```

Nach **jedem abgeschlossenen Benutzer-Prompt / Arbeitsauftrag** muss ein neuer Report als Markdown-Datei abgelegt werden.

Namensschema:

```text
Reports/YYYY-MM-DD_HH-mm_Prompt-XXX_Kurzbeschreibung.md
```

Beispiel:

```text
Reports/2026-08-25_11-30_Prompt-001_Project-Setup.md
```

Der Report soll enthalten:

```markdown
# Report

## Auftrag

Kurze Zusammenfassung der Aufgabe.

## Analyse

Was wurde vor der Implementierung geprüft?

## Änderungen

Welche Dateien wurden erstellt/geändert/gelöscht?

## Architekturentscheidungen

Welche wichtigen Entscheidungen wurden getroffen?

## Tests

Welche Tests wurden ausgeführt?

Ergebnis:

- Build:
- Unit Tests:
- Integration Tests:

## Probleme / Risiken

Bekannte Probleme, Einschränkungen oder technische Schulden.

## Offene Punkte

Was ist bewusst noch nicht umgesetzt?

## Aktualisierte Wissensdateien

Welche Dateien unter `/docs` wurden aktualisiert?
```

Reports sind **historische Arbeitsnachweise**.

Projektwissen, das für zukünftige Aufgaben wichtig ist, gehört zusätzlich in die passenden Dateien unter `/docs`.

---

# 4. Cleanifico – Produktvision

Cleanifico ist eine Betriebssoftware für Gebäudereinigungsunternehmen.

Die Web-App richtet sich hauptsächlich an:

- Geschäftsführung
- Verwaltung
- Disposition
- Objektleitung
- Personalverwaltung

Die spätere .NET-MAUI-App richtet sich hauptsächlich an:

- Reinigungskräfte
- Vorarbeiter
- Objektleiter im Außendienst

---

# 5. Cleanifico Office – geplante Module

Die Web-App soll langfristig mindestens folgende Bereiche besitzen.

## Mitarbeiter

Mitarbeiter müssen:

- angelegt
- angezeigt
- bearbeitet
- deaktiviert
- reaktiviert
- archiviert

werden können.

Ein echtes Löschen darf nur möglich sein, wenn dadurch keine historischen Daten beschädigt werden.

Mögliche Mitarbeiterdaten:

- Personalnummer
- Vorname
- Nachname
- Adresse
- Telefonnummer
- E-Mail
- Eintrittsdatum
- Austrittsdatum
- Status
- Beschäftigungsart
- Wochenstunden
- Sollstunden
- Notizen
- zugewiesene Objekte
- App-Zugang
- Rollen
- Verträge
- Arbeitszeiten
- Abwesenheiten

---

## Kunden

Cleanifico verwaltet Auftraggeber/Kunden.

Ein Kunde kann mehrere Objekte besitzen.

Beispiel:

```text
Müller Gesundheits GmbH
├── Praxis Bernau
├── Praxis Eberswalde
└── Verwaltung Berlin
```

---

## Objekte

Objekte sind konkrete Einsatzorte der Reinigung.

Objekte müssen:

- angelegt
- angezeigt
- bearbeitet
- deaktiviert
- reaktiviert
- archiviert

werden können.

Mögliche Informationen:

- Objektnummer
- Name
- Kunde
- Objektadresse
- Ansprechpartner
- Kontaktdaten
- Objektleiter
- Reinigungstage
- Zutrittszeiten
- Besonderheiten
- Schlüssel
- Dokumente
- Mitarbeiterzuweisungen
- Verträge
- Leistungen
- Reinigungstypen

---

## Reinigungstypen

Der Cleanifico-Kunde kann eigene Reinigungstypen verwalten.

Beispiele:

- Unterhaltsreinigung
- Grundreinigung
- Glasreinigung
- Sonderreinigung
- Baureinigung
- Teppichreinigung
- Außenreinigung
- Desinfektionsreinigung

Reinigungstypen müssen mindestens:

- angelegt
- bearbeitet
- deaktiviert
- reaktiviert
- sofern möglich gelöscht

werden können.

Mögliche Eigenschaften:

- Name
- Kürzel
- Beschreibung
- Aktiv
- optionale Darstellungsinformationen

---

## Zeittypen

Zeittypen werden vom Unternehmen selbst definiert.

Beispiele:

- Arbeitszeit
- Pause
- Fahrzeit
- Rüstzeit
- Bürozeit
- Schulung
- Besprechung
- Urlaub
- Krankheit
- Feiertag
- Überstundenabbau
- Sonstiges

Mögliche Eigenschaften:

- Name
- Kürzel
- Aktiv
- zählt als Arbeitszeit
- wird bezahlt
- benötigt Objektbezug
- beeinflusst Soll-/Ist-Berechnung
- Sortierung

Die genaue fachliche Logik wird später festgelegt.

Nicht voreilig komplexe Lohnabrechnungslogik implementieren.

---

## Mitarbeiterverträge

Cleanifico soll Mitarbeiterverträge verwalten können.

Dabei geht es zunächst um Vertragsdaten und Dokumentation, **nicht um vollständige Lohnabrechnung**.

Mögliche Informationen:

- Vertragsnummer
- Mitarbeiter
- Vertragsbeginn
- Vertragsende
- unbefristet/befristet
- Wochenstunden
- Sollstunden
- Beschäftigungsart
- Probezeit
- Urlaubstage
- optionale Dokumente
- Status

Historische Verträge müssen nachvollziehbar bleiben.

---

## Kunden-/Objektverträge

Verträge zwischen Reinigungsunternehmen und Auftraggebern sollen verwaltet werden.

Ein Vertrag kann je nach späterer Fachentscheidung:

- einem Kunden
- einem Objekt
- oder mehreren Objekten

zugeordnet sein.

Mögliche Informationen:

- Vertragsnummer
- Kunde
- Objekt
- Beginn
- Ende
- Kündigungsfrist
- Status
- vereinbarte Leistungen
- vereinbarte Stunden
- Reinigungstypen
- Dokumente
- Notizen

Finanzielle Felder können später ergänzt werden.

---

## Arbeitszeiten

Das Büro benötigt Einsicht in die Arbeitszeiten aller Mitarbeiter.

Erforderlich sind langfristig mindestens:

- Tagesansicht
- Wochenansicht
- Monatsansicht
- Sollzeit
- Istzeit
- Zeittypen
- Objektbezug
- Korrekturen
- manuelle Einträge
- Kommentare
- Änderungsverlauf
- Freigabe/Sperrung
- Export

Zeitänderungen sollen langfristig auditierbar sein.

---

# 6. Spätere Module

Folgende Bereiche gehören zur Produktvision, sind aber **nicht Bestandteil dieses ersten Implementierungsauftrags**, sofern sie noch nicht existieren:

- Einsatzplanung
- Dienstplanung
- Leistungsverzeichnisse
- Arbeitsaufgaben
- Qualitätskontrollen
- Mängel
- Reklamationen
- Schlüsselverwaltung
- Material-/Lagerverwaltung
- Dokumentenmanagement
- Urlaubsverwaltung
- Krankmeldungen
- Berichte
- Dashboards
- Kundenportal
- Angebote
- Rechnungen
- Benachrichtigungen
- MAUI-App
- Offline-Synchronisierung
- SQLite Offline Cache
- Foto-Upload

Diese Features nicht ungefragt implementieren.

---

# 7. Tenant-Architektur

Eine zentrale Cleanifico-Datenbank für alle Kunden ist **nicht gewünscht**.

Stattdessen:

```text
FergensHub
    |
    +-- Cleanifico Kunde A
    |      +-- Cleanifico API
    |      +-- MySQL DB A
    |
    +-- Cleanifico Kunde B
           +-- Cleanifico API
           +-- MySQL DB B
```

Jeder Kunde erhält:

- eigene Tenant-ID
- eigene API-/Instanz
- eigene MySQL-Datenbank
- eigene Konfiguration
- eigene Lizenz

Die genaue Deploymentstrategie darf anhand bestehender FergensHub-/Assetfico-Patterns übernommen werden.

Bestehende funktionierende Mechanismen bevorzugen.

---

# 8. Lizenzierung

Cleanifico muss wie Assetfico über **FergensHub** lizenziert werden.

FergensHub ist die zentrale Quelle für:

- Tenant
- Kunde
- Produkt
- Lizenzstatus
- Lizenzlaufzeit
- Tarif
- Features
- Limits
- Tenant Endpoint

Cleanifico darf langfristig nicht ausschließlich auf lokale Konfiguration vertrauen, um zu entscheiden, ob eine Lizenz gültig ist.

Mögliche spätere Feature Flags:

```text
Employees
Customers
Objects
Contracts
TimeTracking
Scheduling
QualityManagement
Warehouse
CustomerPortal
MobileApp
```

Mögliche Limits:

```text
MaxEmployees
MaxObjects
MaxMobileUsers
```

Die konkrete technische Umsetzung soll sich möglichst an Assetfico/FergensHub orientieren, falls deren Code oder Patterns im Repository verfügbar sind.

Keine zweite vollständig unabhängige Lizenzarchitektur entwickeln, wenn vorhandene Mechanismen wiederverwendet werden können.

---

# 9. Discovery

Die bestehende Discovery API soll später für Cleanifico genutzt werden.

Grundidee:

```text
Firmencode + Produkt
        |
        v
Discovery API
        |
        v
TenantId
CompanyName
ApiBaseUrl
ApiVersion
```

Die spätere MAUI-App kann darüber den richtigen Cleanifico-Tenant auflösen.

Bestehende Discovery-Verträge und Patterns sollen nach Möglichkeit wiederverwendet werden.

---

# 10. Technologie

Bevorzugter Stack:

- C#
- aktuelles passendes .NET des Repositorys
- ASP.NET Core
- Blazor
- Entity Framework Core
- MySQL
- Pomelo, sofern bereits eingesetzt
- ASP.NET Core Identity
- Docker
- xUnit oder vorhandenes Testframework

Später:

- .NET MAUI
- SQLite
- REST API
- Offline Sync

Keine Technologie austauschen, nur weil eine andere Lösung ebenfalls möglich wäre.

---

# 11. Verbindliche initiale Projektstruktur

Da der Cleanifico-Ordner aktuell leer ist, muss in diesem ersten Prompt die grundlegende Repository- und Solution-Struktur neu aufgebaut werden.

Root:

```text
~/Documents/Projekte/FergenixLabs/Cleanifico
```

Erzeuge dort mindestens:

```text
Cleanifico/
├── Cleanifico.slnx
├── README.md
├── .gitignore
├── src/
│   ├── Cleanifico.Domain/
│   ├── Cleanifico.Application/
│   ├── Cleanifico.Contracts/
│   ├── Cleanifico.Infrastructure/
│   ├── Cleanifico.Api/
│   └── Cleanifico.Web/
├── tests/
│   ├── Cleanifico.Domain.Tests/
│   ├── Cleanifico.Application.Tests/
│   ├── Cleanifico.Infrastructure.Tests/
│   └── Cleanifico.Api.Tests/
├── docs/
│   ├── PROJECT_MEMORY.md
│   ├── ARCHITECTURE.md
│   ├── DECISIONS.md
│   └── TODO.md
└── Reports/
```

## Solution

Verwende das moderne Solution-Format:

```text
Cleanifico.slnx
```

Alle initialen Projekte müssen in die Solution aufgenommen werden.

## Initiale Projekte

Erzeuge echte .NET-Projekte mit passenden Projekt-Referenzen.

### Cleanifico.Domain

Enthält langfristig:

- Entities
- Value Objects
- Enums
- Domain-Regeln
- Domain Exceptions

Soll möglichst keine Abhängigkeiten auf Infrastruktur besitzen.

### Cleanifico.Application

Enthält langfristig:

- Use Cases
- Application Services
- Interfaces
- Validierung
- fachliche Orchestrierung

Referenz:

```text
Cleanifico.Domain
Cleanifico.Contracts
```

### Cleanifico.Contracts

Enthält langfristig:

- API Contracts
- DTOs
- Requests
- Responses
- gemeinsam genutzte öffentliche Verträge

Keine Abhängigkeit auf Infrastructure oder Web.

### Cleanifico.Infrastructure

Enthält langfristig:

- EF Core
- MySQL
- DbContext
- Identity-Persistenz
- Repositories
- externe technische Implementierungen

Referenzen mindestens:

```text
Cleanifico.Domain
Cleanifico.Application
```

### Cleanifico.Api

ASP.NET Core Web API.

Referenzen mindestens:

```text
Cleanifico.Application
Cleanifico.Contracts
Cleanifico.Infrastructure
```

### Cleanifico.Web

Blazor-basierte Büro-Webanwendung.

Sie ist die spätere Verwaltungsoberfläche für Geschäftsführung, Verwaltung, Disposition, Objektleitung und Personalverwaltung.

Die genaue Hosting-/API-Kopplung soll sauber dokumentiert werden.

## Tests

Erzeuge mindestens:

```text
Cleanifico.Domain.Tests
Cleanifico.Application.Tests
Cleanifico.Infrastructure.Tests
Cleanifico.Api.Tests
```

Verwende das im aktuellen .NET-Ökosystem passende Testframework; bevorzugt xUnit, sofern kein begründeter technischer Grund dagegenspricht.

## .NET-Version

Prüfe zuerst die auf dem Mac installierten .NET-SDKs.

Bevorzuge für ein neues Projekt eine aktuelle stabile und langfristig geeignete .NET-Version.

Dokumentiere die tatsächlich gewählte Version in:

```text
/docs/ARCHITECTURE.md
```

und begründe eine Abweichung, falls nicht die erwartete aktuelle LTS-Version verwendet wird.

## Noch kein MAUI-Projekt in Prompt 001 erzwingen

Die spätere App heißt:

```text
Cleanifico.Mobile
```

und wird mit .NET MAUI umgesetzt.

Sie muss in der Architektur bereits vorgesehen und dokumentiert werden.

**In Prompt 001 muss das MAUI-Projekt aber noch nicht erzeugt werden**, wenn dafür zusätzliche Workloads fehlen oder dadurch der initiale Build unnötig erschwert würde.

Keine Fake-/Placeholder-MAUI-App erzeugen.

## Projekt-Referenzen

Setze die Projektabhängigkeiten bewusst und zyklusfrei auf.

Die Domain darf keine Referenz auf Application, Infrastructure, API oder Web besitzen.

Die Application-Schicht darf keine konkrete Infrastructure-Implementierung benötigen.

Dokumentiere die Abhängigkeitsrichtung in `ARCHITECTURE.md`.

## Basisqualität

Der initial erzeugte Stand muss:

- kompilieren
- eine saubere Solution-Struktur besitzen
- keine unnötigen Beispielklassen aus Templates enthalten
- keine ungenutzten Demo-Endpunkte enthalten
- keine WeatherForecast-Beispiele enthalten
- keine unnötigen Platzhalterdateien enthalten
- sinnvolle Namespace-Strukturen verwenden
- Nullable Reference Types aktiviert lassen
- moderne C#-Konventionen verwenden
- testbar bleiben

---

# 12. Erster Arbeitsauftrag

Der Ordner

```text
~/Documents/Projekte/FergenixLabs/Cleanifico
```

ist aktuell leer.

Baue deshalb **das technische Fundament von Cleanifico von Grund auf neu auf**.

Arbeite in dieser Reihenfolge:

1. Prüfe:
   - aktuelles Arbeitsverzeichnis
   - vorhandene Dateien
   - installierte .NET-SDKs
   - verfügbare Workloads
   - Git-Status, falls bereits ein Repository initialisiert wurde

2. Stelle sicher, dass ausschließlich innerhalb des Cleanifico-Root-Verzeichnisses gearbeitet wird.

3. Erzeuge:
   - `Cleanifico.slnx`
   - `/src`
   - `/tests`
   - `/docs`
   - `/Reports`
   - `README.md`
   - passende `.gitignore`

4. Erzeuge die initialen Projekte:
   - `Cleanifico.Domain`
   - `Cleanifico.Application`
   - `Cleanifico.Contracts`
   - `Cleanifico.Infrastructure`
   - `Cleanifico.Api`
   - `Cleanifico.Web`
   - die vier beschriebenen Testprojekte

5. Nimm alle Projekte in `Cleanifico.slnx` auf.

6. Richte die Projekt-Referenzen entsprechend der vorgesehenen Architektur ein.

7. Entferne Template-Demo-Code, der für Cleanifico keine Bedeutung hat.

8. Richte eine minimale, saubere technische Basis ein, sodass Solution und Projekte erfolgreich gebaut werden können.

9. Bereite die Infrastructure-Schicht für die spätere Nutzung von:
   - Entity Framework Core
   - MySQL
   - ASP.NET Core Identity
   vor.

   Aber implementiere noch **keine umfangreichen Business-Entities**.

10. Entscheide begründet, ob EF Core/MySQL-Pakete bereits in Prompt 001 eingebunden werden oder erst mit dem ersten echten Persistence-Modul. Keine unnötigen Packages installieren.

11. Initialisiere die Wissensdateien:
    - `docs/PROJECT_MEMORY.md`
    - `docs/ARCHITECTURE.md`
    - `docs/DECISIONS.md`
    - `docs/TODO.md`

12. Dokumentiere in `ARCHITECTURE.md` mindestens:
    - Root-Pfad
    - Solution-Struktur
    - verwendete .NET-Version
    - Projektverantwortlichkeiten
    - Projektabhängigkeiten
    - geplante Tenant-Architektur
    - geplante MySQL-/EF-Core-Nutzung
    - geplante Blazor-Web-App
    - geplante MAUI-App
    - FergensHub-Lizenzierung
    - Discovery-Integration

13. Dokumentiere in `DECISIONS.md` mindestens die bereits feststehenden Entscheidungen:
    - eigene API/Instanz pro Kunde
    - eigene MySQL-Datenbank pro Kunde
    - Lizenzierung über FergensHub
    - Discovery für Tenant-Auflösung
    - Web-App fürs Büro
    - spätere .NET-MAUI-App für Außendienst
    - Clean Architecture / klare Schichtentrennung
    - `.slnx` als Solution-Format

14. Füge erste sinnvolle technische Tests hinzu, die beweisen, dass die Grundstruktur korrekt referenziert und lauffähig ist. Keine wertlosen `Assert.True(true)`-Tests.

15. Führe mindestens aus:
    - Restore
    - Build
    - Tests

16. Behebe alle Fehler, die durch den neu erzeugten Projektstand verursacht werden.

17. Erstelle nach Abschluss den Report für Prompt 001 unter `/Reports`.

18. Aktualisiere alle Wissensdateien auf den tatsächlich erzeugten Stand.

## Wichtig

In Prompt 001 sollen **noch nicht** Mitarbeiterverwaltung, Objekte, Verträge, Zeiterfassung oder andere große Businessmodule implementiert werden.

Prompt 001 ist das belastbare technische Fundament, auf dem die folgenden Prompts aufbauen.

# 13. Ergebnis dieses Prompts

Nach Abschluss dieses Prompts soll mindestens folgendes vorhanden sein:

```text
Cleanifico/
├── Cleanifico.slnx
├── README.md
├── .gitignore
├── src/
│   ├── Cleanifico.Domain/
│   ├── Cleanifico.Application/
│   ├── Cleanifico.Contracts/
│   ├── Cleanifico.Infrastructure/
│   ├── Cleanifico.Api/
│   └── Cleanifico.Web/
├── tests/
│   ├── Cleanifico.Domain.Tests/
│   ├── Cleanifico.Application.Tests/
│   ├── Cleanifico.Infrastructure.Tests/
│   └── Cleanifico.Api.Tests/
├── docs/
│   ├── PROJECT_MEMORY.md
│   ├── ARCHITECTURE.md
│   ├── DECISIONS.md
│   └── TODO.md
└── Reports/
    └── <Report für Prompt 001>
```

Zusätzlich müssen:

- Restore erfolgreich sein
- Build erfolgreich sein
- Tests erfolgreich sein
- die Architektur dokumentiert sein
- die wichtigen Entscheidungen dokumentiert sein
- der nächste sinnvolle Entwicklungsschritt klar benannt sein

---

# 14. Abschlussantwort von Codex

Die Antwort an den Benutzer soll kompakt sein und enthalten:

1. Was analysiert wurde
2. Was erstellt/geändert wurde
3. Build-/Testergebnis
4. wichtige Erkenntnisse
5. bekannte Probleme
6. Empfehlung für Prompt 002
7. Pfad zum erzeugten Report

Keine riesigen Codeblöcke in die Abschlussantwort kopieren.

Details gehören in die Repository-Dokumentation und den Report.
