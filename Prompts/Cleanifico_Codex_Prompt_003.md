# Cleanifico – Codex Prompt 003
## Tenantlokale Identity, Rollen, Autorisierung und Absicherung der vorhandenen Funktionen

Du arbeitest im bestehenden Cleanifico-Repository:

```text
~/Documents/Projekte/FergenixLabs/Cleanifico
```

Prompt 001 und Prompt 002 wurden bereits abgeschlossen.

Der aktuelle Stand besitzt unter anderem:

- `Cleanifico.slnx`
- .NET 10.0.102
- `AGENTS.md`
- `/docs`
- `/Reports`
- Clean Architecture mit Domain, Application, Contracts, Infrastructure, API und Web
- `CleanificoDbContext`
- EF Core 9.0.19
- Pomelo.EntityFrameworkCore.MySql 9.0.0
- MySQL-Persistenz
- Migration `InitialCleanificoPersistence`
- vollständiges `CleaningType`-Modul
- REST-Endpunkte unter `/api/cleaning-types`
- Web-Verwaltung unter `/reinigungstypen`
- 41/41 Tests grün
- 0 Build-Warnungen / 0 Build-Fehler
- noch keine produktionsfähige Authentifizierung
- noch keine Autorisierung
- noch keine FergensHub-Lizenzprüfung
- noch keine MAUI-Anmeldung

Dieser Prompt führt das **Security-Fundament für Cleanifico Office und API** ein.

---

# 1. Vor Beginn zwingend lesen

Lies vor Änderungen mindestens:

```text
AGENTS.md
README.md
docs/PROJECT_MEMORY.md
docs/ARCHITECTURE.md
docs/DECISIONS.md
docs/TODO.md
```

Lies außerdem den Report von Prompt 002.

Analysiere danach nur die für diesen Auftrag relevanten Bereiche.

Bestehende Architektur und Patterns haben Vorrang vor unnötigen neuen Konstruktionen.

---

# 2. Ziel von Prompt 003

Nach Abschluss muss Cleanifico mindestens besitzen:

- tenantlokale ASP.NET Core Identity
- eigene Identity-Tabellen in der jeweiligen Tenant-Datenbank
- Login
- Logout
- aktueller Benutzer
- Rollen
- Policies
- Schutz der vorhandenen Web-App
- Schutz der vorhandenen CleaningType-API
- sichere Behandlung nicht authentifizierter und nicht autorisierter Zugriffe
- initiale Benutzerbereitstellung ohne fest codierte Passwörter
- Tests
- Migration
- aktualisierte Dokumentation
- Report

Noch nicht Bestandteil:

- FergensHub-Lizenzvalidierung
- Tenant Discovery Login
- .NET MAUI Auth
- öffentliche Registrierung
- Passwort-Reset per E-Mail
- MFA/TOTP
- SSO
- OAuth/OIDC mit Drittanbietern
- Kundenportal

---

# 3. Grundentscheidung – Identity ist tenantlokal

Jede Cleanifico-Kundeninstanz besitzt ihre eigene API und eigene MySQL-Datenbank.

Deshalb wird auch die Identity-Persistenz **innerhalb der jeweiligen Tenant-Datenbank** geführt.

Beispiel:

```text
Cleanifico Kunde A
├── Cleanifico API
├── Cleanifico Web
└── MySQL DB A
    ├── CleaningTypes
    ├── AspNetUsers
    ├── AspNetRoles
    ├── AspNetUserRoles
    └── weitere Identity-Tabellen

Cleanifico Kunde B
├── Cleanifico API
├── Cleanifico Web
└── MySQL DB B
    ├── CleaningTypes
    ├── AspNetUsers
    ├── AspNetRoles
    ├── AspNetUserRoles
    └── weitere Identity-Tabellen
```

Da jede Datenbank genau einem Tenant gehört, soll auch hier nicht künstlich auf jede Identity-Tabelle eine zusätzliche `TenantId` gesetzt werden.

Dokumentiere diese Entscheidung in `docs/DECISIONS.md`, sofern sie noch nicht dokumentiert ist.

---

# 4. Identity-Modell

Führe eine eigene Identity-User-Klasse ein.

Bevorzugter Name:

```csharp
ApplicationUser
```

oder ein bereits zum Projekt passender klarer Name.

Sie soll auf ASP.NET Core Identity basieren.

Keine eigene Passwort- oder Hashing-Implementierung bauen.

Verwende die Sicherheitsmechanismen von ASP.NET Core Identity.

## Mindestens benötigte Eigenschaften

Zusätzlich zu den normalen Identity-Feldern soll der Benutzer mindestens besitzen:

```text
FirstName
LastName
IsActive
CreatedAtUtc
UpdatedAtUtc
```

Optional, wenn architektonisch sinnvoll:

```text
LastLoginAtUtc
```

Aber keine unnötigen Profilfelder hinzufügen.

## Regeln

- Vorname erforderlich
- Nachname erforderlich
- `IsActive` standardmäßig `true`
- deaktivierte Benutzer dürfen sich nicht erfolgreich anmelden
- technische Auditzeitstempel in UTC
- E-Mail soll als Benutzerkennung verwendet werden, sofern bestehende Architektur nichts anderes erfordert
- Benutzername und E-Mail dürfen nicht widersprüchlich geführt werden

---

# 5. Rollenmodell

Führe zunächst genau diese Rollen ein:

```text
Owner
Administrator
Dispatcher
ObjectManager
Employee
```

Diese internen technischen Rollennamen dürfen stabil auf Englisch bleiben.

Die UI soll deutsche Anzeigenamen verwenden:

```text
Owner           -> Inhaber
Administrator   -> Administrator
Dispatcher      -> Disposition
ObjectManager   -> Objektleitung
Employee        -> Mitarbeiter
```

Keine weiteren Rollen in Prompt 003 erfinden.

---

# 6. Bedeutung der Rollen

## Owner / Inhaber

Höchste tenantlokale Rolle.

Darf:

- alle Office-Funktionen verwenden
- Benutzer verwalten
- Rollen zuweisen
- administrative Einstellungen verwenden
- Reinigungstypen vollständig verwalten

Ein Tenant muss langfristig mindestens einen aktiven Owner besitzen.

Prompt 003 soll verhindern, dass der **letzte aktive Owner** versehentlich deaktiviert oder seiner Owner-Rolle beraubt wird, sofern Benutzerverwaltung bereits Teil dieses Prompts wird.

## Administrator

Darf:

- Benutzer verwalten
- Rollen verwalten, außer kritische Owner-Sonderregeln zu verletzen
- Stammdaten verwalten
- Reinigungstypen vollständig verwalten
- administrative Office-Bereiche nutzen

Ein Administrator darf nicht ohne ausdrückliche fachliche Regel den letzten Owner entfernen/deaktivieren.

## Dispatcher / Disposition

Für spätere:

- Einsatzplanung
- Mitarbeiterplanung
- Objektzuweisung
- Arbeitszeiten

In Prompt 003 existieren diese Module noch nicht.

Für das bestehende CleaningType-Modul:

- lesen erlaubt
- erstellen/bearbeiten/löschen/deaktivieren standardmäßig **nicht erlaubt**

## ObjectManager / Objektleitung

Für spätere:

- zugewiesene Objekte
- Qualitätskontrollen
- Mängel
- Mitarbeiter im Objektkontext

Für CleaningType:

- lesen erlaubt
- Änderungen nicht erlaubt

## Employee / Mitarbeiter

Für spätere MAUI-/Außendienstfunktionen.

Für die Office-Web-App:

- standardmäßig kein Zugriff auf administrative Cleanifico-Office-Funktionen
- kein Änderungszugriff auf CleaningTypes
- kein Benutzerverwaltungszugriff

Nicht künstlich eine Mitarbeiter-Web-App bauen.

---

# 7. Policies statt Rollenchecks überall

Definiere zentrale Autorisierungspolicies.

Keine verteilten String-Vergleiche über das ganze Projekt verteilen, wenn Policies sinnvoller sind.

Mindestens geeignete Policies:

```text
OfficeAccess
ManageCleaningTypes
ViewCleaningTypes
ManageUsers
ManageRoles
AdministrationAccess
```

Eine sinnvolle Zuordnung:

```text
OfficeAccess:
Owner
Administrator
Dispatcher
ObjectManager

ManageCleaningTypes:
Owner
Administrator

ViewCleaningTypes:
Owner
Administrator
Dispatcher
ObjectManager

ManageUsers:
Owner
Administrator

ManageRoles:
Owner
Administrator

AdministrationAccess:
Owner
Administrator
```

Rollen- und Policy-Namen möglichst als Konstanten führen, damit keine Magic Strings verteilt werden.

---

# 8. Identity-Integration mit EF Core

Erweitere die Persistenz sauber um ASP.NET Core Identity.

Prüfe, ob `CleanificoDbContext` sinnvoll von einem passenden Identity-DbContext ableiten soll oder ob eine getrennte Identity-Persistenz fachlich/technisch besser ist.

Bevorzuge für den aktuellen Umfang **eine gemeinsame Tenant-Datenbank und einen sauber integrierten DbContext**, sofern keine gewichtigen Gründe dagegensprechen.

Dokumentiere die Entscheidung.

Bestehende `CleaningType`-Migrationen und Tabellen dürfen nicht beschädigt werden.

---

# 9. Neue Migration

Erzeuge eine neue Migration ausschließlich für die Identity-/Security-Erweiterung.

Beispielname:

```text
AddTenantIdentity
```

Die Migration soll:

- bestehende Tabellen erhalten
- Identity-Tabellen ergänzen
- eigene `ApplicationUser`-Felder korrekt abbilden
- benötigte Indizes enthalten

Keine bestehende Migration rückwirkend manipulieren.

---

# 10. Keine Datenbank automatisch verändern

Wie bereits in Prompt 002:

- keine fremde Datenbank löschen
- keine produktive Datenbank verändern
- Migration nicht ungefragt automatisch auf irgendeine reale DB anwenden
- keine `EnsureDeleted()`
- keine destruktiven Automatismen

Migrationen kontrolliert erzeugen.

Falls Tests Datenbanken benötigen, ausschließlich sichere Testumgebungen verwenden.

---

# 11. Initiale Rollenbereitstellung

Die fünf Rollen müssen zuverlässig angelegt werden können.

Das darf idempotent erfolgen.

Bevorzugt über einen klaren Bootstrap-/Initialization-Service.

Beispielsweise:

```text
IdentityBootstrapper
```

Die Rollenanlage muss mehrfach sicher ausführbar sein.

Keine doppelten Rollen erzeugen.

---

# 12. Erster Owner – sichere Bootstrap-Strategie

Ein neuer Cleanifico-Tenant benötigt einen ersten Owner.

Dafür darf **kein fest codiertes Standardpasswort** wie `admin123` oder `Password123!` im Code oder Repository existieren.

Implementiere eine sichere Bootstrap-Strategie.

Bevorzugte Möglichkeiten:

- Environment Variables / User Secrets
- expliziter Bootstrap-Befehl
- oder eine andere kontrollierte, dokumentierte Provisionierungsstrategie

Mindestens benötigte Bootstrap-Daten:

```text
E-Mail
Vorname
Nachname
Initialpasswort
```

Secrets dürfen nicht committed werden.

## Verhalten

- Bootstrap nur ausführen, wenn explizit konfiguriert/angefordert
- keinen Owner stillschweigend mit Standarddaten erzeugen
- vorhandenen Owner nicht überschreiben
- Ergebnis sinnvoll loggen, aber niemals Passwort loggen
- Wiederholung muss sicher/idempotent sein

Dokumentiere für lokale Entwicklung im README, wie ein erster Benutzer sicher erzeugt wird.

---

# 13. Keine öffentliche Registrierung

Cleanifico ist B2B-Software.

Es soll aktuell **keine öffentliche Selbstregistrierung** geben.

Kein:

```text
Registrieren
Kostenloses Konto erstellen
Sign up
```

Benutzer werden später durch Owner, Administrator oder Tenant-Provisionierung angelegt.

---

# 14. Benutzerverwaltung – Scope dieses Prompts

Implementiere eine **kleine, aber echte Benutzerverwaltung im Office-Bereich**.

Route bevorzugt:

```text
/administration/benutzer
```

Diese Seite ist nur erreichbar für:

```text
Owner
Administrator
```

Mindestens möglich:

- Benutzer anzeigen
- Benutzer anlegen
- Benutzer bearbeiten
- aktivieren
- deaktivieren
- Rollen anzeigen
- Rollen zuweisen/entziehen
- kein physisches Löschen notwendig

Noch nicht nötig:

- Mitarbeiter-Entity verknüpfen
- Personalakte
- Arbeitsvertrag
- Arbeitszeiten
- Urlaubsverwaltung
- E-Mail-Einladung
- Passwort-Reset-Mail
- MFA

Identity-Benutzer und spätere Mitarbeiter-Domain-Entity bleiben zunächst getrennte Konzepte.

Dokumentiere das.

---

# 15. Benutzer anlegen

Mindestens benötigte Felder:

```text
Vorname
Nachname
E-Mail
Initialpasswort
Rollen
Aktiv
```

Regeln:

- E-Mail erforderlich
- gültiges E-Mail-Format
- E-Mail tenantlokal eindeutig
- Vorname/Nachname erforderlich
- mindestens eine Rolle
- Passwort muss Identity-Policy erfüllen
- Passwort niemals in Listen/Responses zurückgeben
- Passwort niemals loggen
- keine geheimen Felder in Reports aufnehmen

---

# 16. Benutzer bearbeiten

Bearbeitbar:

```text
Vorname
Nachname
E-Mail
Aktiv
Rollen
```

Passwortänderung nicht als normales Textfeld in die Bearbeitungsmaske mischen.

Wenn eine administrative Passwort-Neuvergabe bereits sauber umgesetzt werden kann, dann als getrennte Aktion.

Ansonsten als späteren TODO dokumentieren.

---

# 17. Schutz des letzten Owners

Implementiere eine zentrale fachliche Schutzregel:

> Ein Tenant darf nicht durch normale Benutzerverwaltung ohne aktiven Owner zurückbleiben.

Mindestens verhindern:

- letzten aktiven Owner deaktivieren
- letzten aktiven Owner löschen, falls Löschen existiert
- dem letzten aktiven Owner die Owner-Rolle entziehen

Diese Regel muss serverseitig gelten.

Nicht nur im UI verstecken.

---

# 18. Login

Erstelle eine professionelle deutsche Login-Seite für Cleanifico Office.

Bevorzugte Route:

```text
/login
```

Mindestens:

```text
E-Mail
Passwort
Angemeldet bleiben
Anmelden
```

Die Seite soll visuell zur Cleanifico-Web-App passen.

Keine öffentliche Registrierung verlinken.

Bei falschen Zugangsdaten allgemeine Meldung:

```text
E-Mail oder Passwort ist ungültig.
```

Nicht verraten, ob eine E-Mail existiert.

---

# 19. Logout

Implementiere einen sicheren Logout.

Bevorzugt als klarer Menüpunkt im Benutzerbereich der Web-App.

Nach Logout:

- Auth-Session beendet
- Rückleitung zu `/login`

---

# 20. Passwort- und Lockout-Policy

Konfiguriere ASP.NET Core Identity sinnvoll für B2B-Betrieb.

Mindestens prüfen:

- vernünftige Mindestlänge
- Lockout nach mehreren Fehlversuchen
- sichere Tokenprovider-Konfiguration
- eindeutige E-Mail

Dokumentiere die konkret gewählten Werte in `ARCHITECTURE.md` oder `DECISIONS.md`.

Keine Passwörter in Klartext persistieren.

---

# 21. Cookie-/Session-Sicherheit

Für die Office-Web-App eine sichere Authentifizierung konfigurieren.

Mindestens berücksichtigen:

```text
HttpOnly
Secure in produktiver Umgebung
SameSite
Sliding Expiration falls sinnvoll
LoginPath
AccessDeniedPath
```

Keine Session-Cookies unnötig lange gültig machen.

Development und Production sauber unterscheiden.

---

# 22. Access Denied

Erstelle eine verständliche Seite, beispielsweise:

```text
/zugriff-verweigert
```

Text sinngemäß:

```text
Sie besitzen nicht die erforderliche Berechtigung für diesen Bereich.
```

Keine internen Policy-Namen oder technischen Details anzeigen.

---

# 23. Web-App vollständig schützen

Die Office-Web-App darf nach Prompt 003 nicht mehr standardmäßig anonym nutzbar sein.

Mindestens:

- Dashboard geschützt
- Reinigungstypen geschützt
- Administration geschützt
- Navigation berücksichtigt Rechte
- Login bleibt anonym erreichbar
- Access-Denied-Seite bleibt erreichbar

`Employee` erhält keinen normalen Office-Zugriff.

---

# 24. Navigation nach Berechtigung

Die Sidebar soll Einträge nur anzeigen, wenn sie für den Benutzer sinnvoll sind.

Beispiel:

```text
Owner / Administrator
├── Dashboard
├── Reinigungstypen
└── Administration
    └── Benutzer

Dispatcher
├── Dashboard
└── Reinigungstypen (nur lesend)

ObjectManager
├── Dashboard
└── Reinigungstypen (nur lesend)

Employee
└── keine administrative Office-Navigation
```

Noch nicht existierende Module nicht künstlich anlegen.

Wichtig:

> UI-Sichtbarkeit ersetzt niemals serverseitige Autorisierung.

---

# 25. CleaningType-Webseite absichern

Bestehende Seite:

```text
/reinigungstypen
```

muss künftig mindestens `ViewCleaningTypes` verlangen.

Änderungsaktionen müssen zusätzlich `ManageCleaningTypes` verlangen.

Für Benutzer ohne Änderungsrecht:

- Liste sichtbar
- Suche/Filter sichtbar
- keine Buttons für Anlegen/Bearbeiten/Deaktivieren/Reaktivieren/Löschen

Serverseitig trotzdem absichern.

---

# 26. CleaningType-API absichern

Bestehende Endpunkte:

```http
GET    /api/cleaning-types
GET    /api/cleaning-types/{id}
POST   /api/cleaning-types
PUT    /api/cleaning-types/{id}
POST   /api/cleaning-types/{id}/activate
POST   /api/cleaning-types/{id}/deactivate
DELETE /api/cleaning-types/{id}
```

Mindestens:

## Lesen

```text
ViewCleaningTypes
```

für GET-Liste und GET by id.

## Schreiben

```text
ManageCleaningTypes
```

für POST, PUT, activate, deactivate und delete.

Nicht authentifiziert:

```text
401 Unauthorized
```

Authentifiziert aber unberechtigt:

```text
403 Forbidden
```

Keine Umleitung auf HTML-Login für echte API-Requests.

---

# 27. Authentifizierung zwischen Web und API

Analysiere zuerst die tatsächliche aktuelle Kopplung von `Cleanifico.Web` und `Cleanifico.Api`.

Treffe dann eine saubere, dokumentierte Entscheidung.

Wichtig:

- keine unsichere Fake-Authentifizierung
- keine selbstgebastelten JWTs
- keine Secrets im Browsercode
- keine Autorisierung nur im Web
- API muss Benutzeridentität selbst verifizieren können

Wenn Web und API getrennte Prozesse sind, implementiere ein offiziell unterstütztes ASP.NET-Core-Verfahren, das zur aktuellen Architektur passt.

Wenn ein vollständiges Bearer-Token-System für spätere MAUI-Anmeldung den Scope unnötig aufblasen würde, dann:

- sichere Office-Web/API-Kommunikation jetzt sauber
- MAUI-Token-Authentifizierung ausdrücklich für einen späteren Prompt dokumentieren

Die gewählte Lösung muss im Report nachvollziehbar erklärt werden.

---

# 28. Benutzer-API / Application Layer

Benutzerverwaltung darf nicht als riesiger Block Businesslogik direkt im Web implementiert werden.

Erzeuge geeignete Services/Abstraktionen für:

```text
GetUsers
GetUserById
CreateUser
UpdateUser
ActivateUser
DeactivateUser
GetRoles
UpdateUserRoles
```

ASP.NET Core Identity APIs dürfen im Infrastructure-/Security-Bereich sinnvoll gekapselt werden.

Passwort- und Rollensicherheitsregeln serverseitig durchsetzen.

Keine Identity-Entities direkt als API-Responses serialisieren.

---

# 29. Contracts für Benutzerverwaltung

Erstelle getrennte Contracts, z. B.:

```text
UserResponse
CreateUserRequest
UpdateUserRequest
UpdateUserRolesRequest
RoleResponse
```

Keine Felder wie:

```text
PasswordHash
SecurityStamp
ConcurrencyStamp
AuthenticatorKey
RecoveryCodes
```

dürfen nach außen gelangen.

---

# 30. Fehlerbehandlung

Mindestens sauber behandeln:

```text
Benutzer nicht gefunden
E-Mail bereits vergeben
Rolle unbekannt
letzter Owner darf nicht deaktiviert werden
letzte Owner-Rolle darf nicht entfernt werden
Passwort erfüllt Anforderungen nicht
Benutzer ist deaktiviert
keine Berechtigung
```

Sinnvolle HTTP-Codes:

```text
400 Bad Request
401 Unauthorized
403 Forbidden
404 Not Found
409 Conflict
```

Keine Identity-internen Stacktraces an Clients ausgeben.

---

# 31. Logging

Security-relevante Vorgänge dürfen sinnvoll protokolliert werden.

Beispiele:

```text
Login erfolgreich
Login fehlgeschlagen
Benutzer deaktiviert
Rollen geändert
Bootstrap Owner erstellt
```

Aber niemals loggen:

```text
Passwörter
Passwort-Hashes
SecurityStamp
Tokens
Cookies
Secrets
```

---

# 32. Tests – Identity

Erweitere Tests substantiell.

Mindestens:

- Benutzer kann mit gültigen Daten erstellt werden
- doppelte E-Mail wird verhindert
- deaktivierter Benutzer kann sich nicht erfolgreich anmelden
- Rollen werden idempotent angelegt
- Benutzer kann Rolle erhalten
- Benutzer kann Rolle verlieren
- letzter Owner kann nicht deaktiviert werden
- letzte Owner-Rolle kann nicht entfernt werden
- normale Benutzer dürfen Owner-Schutzregel nicht umgehen

---

# 33. Tests – Autorisierung

Mindestens echte HTTP-/Integrationstests für:

## Anonym

```text
GET /api/cleaning-types -> 401
POST /api/cleaning-types -> 401
```

## Owner

```text
GET -> erlaubt
POST -> erlaubt
PUT -> erlaubt
Deactivate -> erlaubt
Delete -> erlaubt
```

## Administrator

Gleichwertige CleaningType-Verwaltung erlaubt.

## Dispatcher

```text
GET -> erlaubt
POST -> 403
PUT -> 403
DELETE -> 403
```

## ObjectManager

```text
GET -> erlaubt
POST -> 403
```

## Employee

```text
CleaningType Office/API administrativ nicht erlaubt
```

---

# 34. Tests – Web

Teste soweit mit bestehender Testarchitektur sinnvoll:

- Login-Seite anonym erreichbar
- geschützte Office-Seite anonym nicht erreichbar
- autorisierter Benutzer darf geschützte Seite öffnen
- unberechtigter Benutzer erhält Access Denied bzw. korrektes Verhalten
- Benutzerverwaltung nur Owner/Admin

Keine fragilen Pixel-/CSS-Tests erzwingen.

---

# 35. Tests und reale Datenbank

Wie bisher:

- niemals produktive DB verwenden
- sichere Testkonfiguration
- keine normalen Development-Daten zerstören
- keine gefälschten Integrationstests

Wenn MySQL-Integration lokal nicht zuverlässig verfügbar ist, Grenzen sauber dokumentieren.

---

# 36. Bestehende Tests dürfen nicht regressieren

Die 41 bestehenden Tests aus Prompt 002 müssen weiterhin sinnvoll funktionieren.

Falls Autorisierung bestehende API-Tests beeinflusst:

- Tests korrekt authentifizieren
- Security nicht deaktivieren, nur damit Tests wieder grün werden

Keine Produktionssicherheit für Tests aufweichen.

---

# 37. Build und Verifikation

Nach Implementierung mindestens:

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

Zusätzlich relevante Security-Flows manuell oder automatisiert prüfen.

---

# 38. README aktualisieren

Dokumentiere mindestens:

- wie lokale Authentifizierung funktioniert
- wie Rollen aufgebaut sind
- wie ein erster Owner sicher gebootstrapped wird
- welche Secrets/Environment Variables benötigt werden
- wie Login lokal getestet wird
- dass öffentliche Registrierung nicht existiert
- dass FergensHub-Lizenzprüfung noch folgt

Keine echten Zugangsdaten in README schreiben.

---

# 39. PROJECT_MEMORY.md aktualisieren

Nur dauerhaft relevantes Wissen aufnehmen:

- Identity-Technologie
- `ApplicationUser`
- Rollen
- Policy-Namen
- Bootstrap-Mechanismus
- Auth-Verfahren Web/API
- relevante Security-Service-Namen
- Migration
- wichtige Routen

Keine Verlaufsbeschreibung.

---

# 40. ARCHITECTURE.md aktualisieren

Dokumentiere:

```text
Browser
   |
   v
Cleanifico.Web
   |
   | authentifizierter Benutzer
   v
Cleanifico.Api
   |
   v
Application
   |
   v
Identity/Persistence
   |
   v
Tenant MySQL DB
```

Zusätzlich:

- Identity-Persistenz
- Auth-Verfahren
- Cookie-/Session-Strategie
- Policy-System
- Benutzerverwaltung
- Rollenmodell
- Bootstrap
- noch fehlende MAUI-Authentifizierung
- noch fehlende FergensHub-Lizenzprüfung

---

# 41. DECISIONS.md aktualisieren

Mindestens neue Decision Records für:

- tenantlokale ASP.NET Core Identity
- keine öffentliche Registrierung
- Rollenmodell
- Policy-basierte Autorisierung
- letzter Owner geschützt
- Benutzerkonto und spätere Mitarbeiter-Entity sind getrennte Konzepte
- gewähltes Web/API-Authentifizierungsverfahren
- initialer Owner nur über sicheren expliziten Bootstrap
- MFA folgt später und wird nicht vorgetäuscht

---

# 42. TODO.md aktualisieren

Nach Prompt 003 sollen mindestens als spätere Punkte bestehen, sofern noch offen:

```text
FergensHub-Lizenzprüfung
Discovery-Anmeldung
MAUI Auth
MFA/TOTP
Passwort vergessen / Reset
Einladungsworkflow
Verknüpfung ApplicationUser <-> Employee
Security Hardening vor Produktion
```

Nur echte offene Punkte aufnehmen.

---

# 43. AGENTS.md aktualisieren

Nur falls durch Prompt 003 dauerhaft neue Regeln entstehen.

Sinnvolle Ergänzungen können sein:

- Security niemals für Tests deaktivieren
- keine Secrets committen
- API-Endpunkte standardmäßig schützen
- neue Office-Module immer einer Policy zuordnen
- Rollen-/Policy-Namen zentral definieren

`AGENTS.md` nicht zu einem riesigen Handbuch aufblasen.

---

# 44. Report für Prompt 003

Erstelle:

```text
Reports/YYYY-MM-DD_HH-mm_Prompt-003_Identity-Authorization.md
```

Mindestens:

```markdown
# Report – Prompt 003

## Auftrag

## Ausgangsstand

## Analyse

## Identity

## Rollen

## Policies

## Benutzerverwaltung

## Login / Logout

## API-Absicherung

## Web-Absicherung

## Datenbank / Migration

## Bootstrap

## Tests

## Build

## Sicherheitsentscheidungen

## Noch nicht umgesetzt

## Aktualisierte Wissensdateien

## Git-Status
```

Keine Passwörter oder Secrets in den Report schreiben.

---

# 45. Git

Der Stand von Prompt 002 sollte vom Benutzer als eigener Meilenstein committed worden sein.

In diesem Prompt:

- keine Git-History umschreiben
- keinen Force Push
- keinen Push
- keinen Branch ungefragt erstellen
- nicht automatisch committen

Am Ende:

```bash
git status
git diff --stat
```

prüfen.

Codex soll in seiner Abschlussantwort sagen, dass die Prompt-003-Änderungen noch committed werden müssen, sofern sie uncommitted sind.

---

# 46. Nicht Bestandteil von Prompt 003

Noch NICHT bauen:

- FergensHub-Lizenzprüfung
- Discovery API Integration
- Kunden
- Objekte
- Zeittypen
- Mitarbeiter-Domain-Modul
- Mitarbeiterverträge
- Objektverträge
- Arbeitszeiten
- Einsatzplanung
- Leistungsverzeichnisse
- Qualitätsmanagement
- Reklamationen
- Lager
- Schlüssel
- MAUI
- Offline-Sync
- Kundenportal
- Rechnungen
- öffentliche Registrierung
- MFA
- SSO

Keine Feature-Ausweitung.

---

# 47. Definition of Done

Prompt 003 ist erst abgeschlossen, wenn:

- ASP.NET Core Identity integriert ist
- `ApplicationUser` existiert
- Identity in tenantlokaler MySQL-Persistenz vorgesehen ist
- neue Migration existiert
- Rollen zuverlässig angelegt werden
- sicherer Owner-Bootstrap existiert
- kein Defaultpasswort committed ist
- öffentliche Registrierung nicht existiert
- Login funktioniert
- Logout funktioniert
- deaktivierte Benutzer keinen Zugriff erhalten
- Rollenmodell umgesetzt ist
- Policies zentral definiert sind
- Office-App geschützt ist
- CleaningType-Webseite geschützt ist
- CleaningType-Schreibaktionen rollenabhängig geschützt sind
- CleaningType-API 401/403 korrekt liefert
- Benutzerverwaltung für Owner/Admin existiert
- letzter Owner geschützt ist
- Tests substantiell erweitert wurden
- bestehende Tests nicht durch Abschalten von Security „gerettet“ wurden
- Restore erfolgreich ist
- Build 0 Fehler hat
- nach Möglichkeit Build 0 Warnungen hat
- alle Tests grün sind
- Dokumentation aktuell ist
- Prompt-003-Report existiert
- Git-Status geprüft wurde

---

# 48. Abschlussantwort von Codex

Antworte kompakt auf Deutsch mit:

1. Dauer
2. implementierte Identity-/Security-Funktionen
3. Rollen und Policies
4. Benutzerverwaltung
5. Login-/Logout-Status
6. Datenbank-/Migrationsstatus
7. abgesicherte API-/Web-Bereiche
8. Bootstrap-Strategie
9. Build-Ergebnis
10. Testergebnis
11. noch fehlende Security-Themen
12. Pfad zum Report
13. Git-Status
14. Empfehlung für Prompt 004

Keine großen Codeblöcke in die Abschlussantwort kopieren.

Details gehören in Code, `/docs` und `/Reports`.
