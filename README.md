# Cleanifico

Cleanifico ist eine mandantenfähige Betriebssoftware für Gebäudereinigungsunternehmen. Dieses Repository enthält Cleanifico Office, die ASP.NET-Core-API und das technische Fundament der Fachmodule.

## Aktueller Stand

- .NET 10 (`net10.0`), SDK-Familie über `global.json` festgelegt
- Clean-Architecture-orientierte Schichten ohne zyklische Abhängigkeiten
- EF Core 9.0.19 mit stabilem Pomelo-MySQL-Provider 9.0.0 für MySQL 8.4
- `CleanificoDbContext`, tenantlokales ASP.NET Core Identity und Migrationen `InitialCleanificoPersistence` sowie `AddTenantIdentity`
- erster vertikaler Fachschnitt `CleaningType` mit Domain-Regeln, Application Service, REST API und Blazor-Verwaltung
- frei konfigurierbare `TimeType`-Stammdaten mit einmalig initialisierten, vollständig änderbaren Standardwerten
- Kundenverwaltung für Auftraggeber mit Ansprechpartner, Verwaltungsadresse, Detailansicht und Lifecycle
- Objektverwaltung mit verpflichtendem Kundenbezug, eigener Objektadresse, direktem Kontakt und Lifecycle
- Mitarbeiterverwaltung mit Personalstammdaten, Beschäftigungsumfang und bewusst getrennter Identity
- separate ASP.NET Core API mit geschützten Stammdaten- und Benutzer-Endpunkten; nur `GET /health` und Login sind anonym erreichbar
- AssetFico-kompatible, installationsgebundene und signierte Offline-Lease mit zentraler fail-closed Lizenzgrenze für alle fachlichen API- und Office-Bereiche
- separate Blazor-Web-App für Cleanifico Office mit Login, `/kunden`, `/objekte`, `/reinigungstypen`, `/zeittypen` und `/administration/benutzer`
- Mitarbeiterverwaltung in Cleanifico Office unter `/mitarbeiter`
- fünf xUnit-Testprojekte mit Domain-, Application-, Identity-, EF-, Architektur-, API- und Web-Integrationstests
- noch keine Discovery-, Mobile- oder weiteren Businessmodule

## Projektstruktur

| Pfad | Verantwortung |
| --- | --- |
| `src/Cleanifico.Domain` | Fachmodell und Domain-Regeln |
| `src/Cleanifico.Application` | Use Cases, Ports und fachliche Orchestrierung |
| `src/Cleanifico.Contracts` | Öffentliche API-Verträge und DTOs |
| `src/Cleanifico.Infrastructure` | EF-Core-/MySQL-Persistenz und technische Adapter |
| `src/Cleanifico.Api` | ASP.NET Core API und Composition Root |
| `src/Cleanifico.Web` | Blazor-basierte Office-Webanwendung |
| `tests` | Architektur-, Unit- und Integrationstests |
| `docs` | Aktuelle Wissens- und Architekturbasis |
| `Reports` | Historische Arbeitsnachweise pro Prompt |

## Lokale Entwicklung

Voraussetzungen sind ein passendes .NET-10-SDK und MySQL 8.4. Für die API wird der Connection String `ConnectionStrings:Cleanifico` benötigt. Lokale Zugangsdaten werden über User Secrets gesetzt und nicht committed:

```bash
dotnet user-secrets set --project src/Cleanifico.Api "ConnectionStrings:Cleanifico" "Server=localhost;Port=3306;Database=cleanifico_dev;User=<user>;Password=<password>"
```

Anschließend im Repository-Root ausführen:

```bash
dotnet restore Cleanifico.slnx
dotnet build Cleanifico.slnx --no-restore
dotnet test Cleanifico.slnx --no-build
```

API oder Web-App können separat gestartet werden:

```bash
dotnet run --project src/Cleanifico.Api
dotnet run --project src/Cleanifico.Web
```

Die Development-Konfiguration der Web-App erwartet die API unter `https://localhost:7182`; die Web-App selbst startet standardmäßig unter `https://localhost:7282`.

## Lizenzkonfiguration

Die API verwendet das AssetFico-Muster aus lokaler signierter Lease, persistenter Installation-ID und periodischem FergensHub-Refresh. Lokal liegt der State standardmäßig unter `src/Cleanifico.Api/config/license-state.json`, produktiv unter `/app/config/license-state.json`; er enthält ein geheimes Refresh-Credential und muss persistent, nur für den Dienstbenutzer lesbar und im Backup enthalten sein. Eine Freischaltung über Konfigurationswerte existiert nicht.

Die FergensHub-Basis-URL wird ausschließlich extern konfiguriert:

```bash
dotnet user-secrets set --project src/Cleanifico.Api "Licensing:BaseUrl" "https://<fergenshub-host>/"
```

Owner und Administrator aktivieren beziehungsweise erneuern die Installation unter `/lizenz`. Der analysierte FergensHub-Stand stellt die bereits von AssetFico definierten Aktivierungs-/Refresh-Routen derzeit noch nicht serverseitig bereit; bis zu dieser externen Ergänzung bleibt eine neue Cleanifico-Installation `NotActivated`.

## Tenantlokale Anmeldung

Identity liegt in derselben tenantlokalen Datenbank wie die Fachdaten. Die Rollen sind `Owner`, `Administrator`, `Dispatcher`, `ObjectManager` und `Employee`. Cleanifico Office bietet keine öffentliche Registrierung; Benutzer werden unter `/administration/benutzer` durch Owner oder Administrator verwaltet.

Beim Start werden die Rollen idempotent angelegt. Der erste Owner wird nur nach expliziter Aktivierung und ohne Standardpasswort erzeugt. Lokale Werte gehören in User Secrets:

```bash
dotnet user-secrets set --project src/Cleanifico.Api "SecurityBootstrap:Owner:Enabled" "true"
dotnet user-secrets set --project src/Cleanifico.Api "SecurityBootstrap:Owner:Email" "owner@example.test"
dotnet user-secrets set --project src/Cleanifico.Api "SecurityBootstrap:Owner:FirstName" "Erika"
dotnet user-secrets set --project src/Cleanifico.Api "SecurityBootstrap:Owner:LastName" "Muster"
dotnet user-secrets set --project src/Cleanifico.Api "SecurityBootstrap:Owner:InitialPassword" "<sicheres-einmaliges-passwort>"
```

Nach erfolgreichem Bootstrap ist die Owner-Option wieder zu deaktivieren und das Initialpasswort aus dem Secret Store zu entfernen. API und Web teilen den geschützten Identity-Cookie-Schlüsselbund. `Authentication:DataProtectionKeysPath` muss bei getrennten Hosts auf denselben persistenten Pfad zeigen; produktiv sind Zugriffsschutz und Verschlüsselung des Schlüsselbunds durch die Deploymentumgebung sicherzustellen.

## Datenbankmigrationen

Migrationen werden kontrolliert ausgeführt. Die Anwendung migriert beim Start keine Datenbank automatisch. Eine lokale Datenbank kann mit einem expliziten, nicht eingecheckten Connection String aktualisiert werden:

```bash
dotnet ef database update \
  --project src/Cleanifico.Infrastructure \
  --startup-project src/Cleanifico.Infrastructure \
  --context CleanificoDbContext \
  --connection "Server=localhost;Port=3306;Database=cleanifico_dev;User=<user>;Password=<password>"
```

Neue Migrationen werden aus dem Repository-Root erzeugt:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/Cleanifico.Infrastructure \
  --startup-project src/Cleanifico.Infrastructure \
  --context CleanificoDbContext \
  --output-dir Persistence/Migrations
```

Der Design-Time-Context verwendet ohne explizite Zielverbindung nur eine nicht funktionsfähige lokale Platzhalterverbindung zur Modellerzeugung. Produktive Tenant-Migrationen benötigen später einen kontrollierten Rollout-Prozess.

## API für Reinigungstypen

| Methode | Route | Zweck |
| --- | --- | --- |
| `GET` | `/api/cleaning-types?search=&isActive=` | Suchen, filtern und sortiert auflisten |
| `GET` | `/api/cleaning-types/{id}` | Einzelnen Reinigungstyp laden |
| `POST` | `/api/cleaning-types` | Reinigungstyp anlegen |
| `PUT` | `/api/cleaning-types/{id}` | Reinigungstyp bearbeiten |
| `POST` | `/api/cleaning-types/{id}/activate` | Reaktivieren |
| `POST` | `/api/cleaning-types/{id}/deactivate` | Deaktivieren |
| `DELETE` | `/api/cleaning-types/{id}` | Unreferenzierten Datensatz endgültig löschen |

Lesen erfordert `Owner`, `Administrator`, `Dispatcher` oder `ObjectManager`; Schreiben ist auf `Owner` und `Administrator` beschränkt. Anonyme Zugriffe erhalten `401`, angemeldete Benutzer ohne Berechtigung `403`.

## Zeittypen

Zeittypen werden unter `/api/time-types` und in Cleanifico Office unter `/zeittypen` verwaltet. Sie sind normale tenantlokale Datensätze und keine fest codierten Enums. Owner und Administrator dürfen lesen und verwalten; Dispatcher und ObjectManager nur lesen; Employee erhält keinen administrativen Zugriff.

Beim ersten Start nach der Migration `AddConfigurableTimeTypes` werden `ARB`, `PAU`, `FAH`, `URL`, `KRK`, `SCH` und `BES` einmalig angelegt. Ein technischer Initialisierungsmarker verhindert jede spätere Neueinspielung: Umbenennen, Codeänderungen, Eigenschaften, Deaktivierung und auch Löschungen werden nie durch Startlogik zurückgesetzt. Sobald spätere Zeitbuchungen einen Zeittyp verwenden, ist Deaktivierung statt physischem Löschen vorgesehen.

## Kunden

Kunden sind die Auftraggeber des tenantlokalen Reinigungsunternehmens. Sie werden unter `/api/customers` und in Cleanifico Office unter `/kunden` verwaltet. Die Kundennummer ist innerhalb der Tenant-Datenbank eindeutig und änderbar. Ansprechpartner und Verwaltungsadresse liegen direkt am Kunden.

Owner und Administrator dürfen Kunden lesen und verwalten, Dispatcher und ObjectManager nur lesen. Employee besitzt keinen administrativen Zugriff. Ein Kunde mit mindestens einem Objekt kann nicht physisch gelöscht werden; der reguläre Lifecycle ist dann die Deaktivierung.

## Objekte

Reinigungsobjekte werden unter `/api/objects` und in Cleanifico Office unter `/objekte` verwaltet. Jedes Objekt gehört verpflichtend zu einem realen Kunden, besitzt eine eigene Einsatzadresse und kann einen direkten Ansprechpartner sowie Zugangs- und Reinigungshinweise führen. Die Objektnummer ist tenantlokal eindeutig und änderbar. Das Kundendetail zeigt die zugeordneten Objekte mit Direktlinks.

## Dokumentation

- [Projektwissen](docs/PROJECT_MEMORY.md)
- [Architektur](docs/ARCHITECTURE.md)
- [Entscheidungen](docs/DECISIONS.md)
- [Offene Aufgaben](docs/TODO.md)
