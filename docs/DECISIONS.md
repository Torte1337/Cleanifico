# Cleanifico – Entscheidungen

## DEC-001 – Eigene API-/Instanz pro Tenant

Status: Accepted

Entscheidung: Jeder Cleanifico-Tenant erhält eine eigene API-/Anwendungsinstanz.

Grund: Starke Isolation, tenantbezogene Konfiguration und kontrollierbare Deployments.

Datum: 2026-08-25

## DEC-002 – Eigene MySQL-Datenbank pro Tenant

Status: Accepted

Entscheidung: Jeder Tenant erhält eine eigene MySQL-Datenbank; es gibt keine zentrale Cleanifico-Fachdatenbank für alle Kunden.

Grund: Datenisolation, vereinfachte Backups und Migrationen sowie geringeres Risiko tenantübergreifender Zugriffe.

Datum: 2026-08-25

## DEC-003 – Lizenzierung über FergensHub

Status: Accepted

Entscheidung: FergensHub ist die zentrale Quelle für Produktlizenz, Tarif, Features, Limits und Laufzeit.

Grund: Vermeidung einer zweiten unabhängigen Lizenzarchitektur und zentrale Produktverwaltung.

Datum: 2026-08-25

## DEC-004 – Tenant-Auflösung über Discovery

Status: Accepted

Entscheidung: Firmencode und Produkt werden später über die bestehende Discovery API zum Tenant-Endpunkt aufgelöst.

Grund: Clients müssen keine tenantbezogenen Endpunkte fest einbauen.

Datum: 2026-08-25

## DEC-005 – Blazor-Web-App für das Büro

Status: Accepted

Entscheidung: Cleanifico Office wird als eigenständige Blazor-Web-App umgesetzt und kommuniziert über öffentliche Contracts mit der API.

Grund: Klare Trennung von UI und Serverimplementierung sowie Eignung für die Büro-Zielgruppen.

Datum: 2026-08-25

## DEC-006 – Spätere .NET-MAUI-App für den Außendienst

Status: Accepted

Entscheidung: Die Außendienst-App wird später als `Cleanifico.Mobile` mit .NET MAUI umgesetzt; sie ist noch nicht Teil der initialen Solution.

Grund: Mobile- und Offline-Anforderungen sollen separat und erst mit konkreten Workflows eingeführt werden.

Datum: 2026-08-25

## DEC-007 – Klare Schichtentrennung

Status: Accepted

Entscheidung: Domain, Application, Contracts, Infrastructure, API und Web besitzen eindeutige Verantwortlichkeiten und einen zyklusfreien Referenzgraphen.

Grund: Fachlogik bleibt unabhängig von UI, Persistenz und externen Diensten und kann gezielt getestet werden.

Datum: 2026-08-25

## DEC-008 – `.slnx` als Solution-Format

Status: Accepted

Entscheidung: Die Solution wird ausschließlich als `Cleanifico.slnx` geführt.

Grund: Modernes, kompaktes und gut diffbares Solution-Format des verwendeten SDKs.

Datum: 2026-08-25

## DEC-009 – .NET 10 als initiale Plattform

Status: Accepted

Entscheidung: Alle initialen Projekte zielen auf `net10.0`; die SDK-Familie wird über `global.json` festgelegt.

Grund: Installierte, aktuelle und langfristig geeignete Plattform für ein neu gestartetes Produkt.

Datum: 2026-08-25

## DEC-010 – Persistenzpakete erst mit einem echten Modul

Status: Superseded by DEC-011

Entscheidung: EF Core, Pomelo und Identity-Persistenz werden in Prompt 001 noch nicht als Pakete eingebunden.

Grund: Ohne Fachmodell, `DbContext` und Konfiguration wären die Pakete ungenutzt; die passende Version und Einrichtung wird mit dem ersten Persistenzmodul festgelegt.

Datum: 2026-08-25

## DEC-011 – EF Core und Pomelo/MySQL als Persistenzbasis

Status: Accepted

Entscheidung: Cleanifico verwendet EF Core 9.0.19 mit dem stabilen Pomelo-Provider 9.0.0 und einem expliziten MySQL-8.4-Serverprofil. Auf EF Core/Pomelo 10 wird erst gemeinsam gewechselt, wenn eine stabile kompatible Providerfreigabe verfügbar und geprüft ist.

Grund: Pomelo 9 ist der stabile MySQL-Providerzweig und an EF Core 9 gebunden. Die gemeinsame 9.0-Patchlinie vermeidet eine nicht unterstützte Mischung oder Preview-Abhängigkeiten und ist von `net10.0`-Hosts nutzbar.

Datum: 2026-08-25

## DEC-012 – Keine TenantId auf jeder Business-Entity

Status: Accepted

Entscheidung: Solange jeder Tenant eine eigene Instanz und MySQL-Datenbank besitzt, erhalten Business-Entities wie `CleaningType` keine zusätzliche `TenantId`.

Grund: Die Datenbank bildet bereits die technische Isolationsgrenze. Ein redundantes Tenantfeld würde Abfragen und Indizes verkomplizieren, ohne die Isolation innerhalb dieses Deploymentmodells zu erhöhen.

Datum: 2026-08-25

## DEC-013 – Deaktivierung und bedingtes physisches Löschen von Reinigungstypen

Status: Accepted

Entscheidung: Reinigungstypen können deaktiviert und reaktiviert werden. Physisches Löschen ist nur zulässig, solange keine fachlichen oder historischen Referenzen existieren; sobald solche Referenzen eingeführt werden, ist Deaktivierung der normale Weg und ein Fremdschlüsselkonflikt verhindert das Löschen.

Grund: Aktuell existieren keine referenzierenden Module. Das erlaubt eine einfache Bereinigung, ohne die spätere Nachvollziehbarkeit historischer Daten zu gefährden.

Datum: 2026-08-25

## DEC-014 – API-Contracts bleiben von Domain-Entities getrennt

Status: Accepted

Entscheidung: HTTP-Eingaben und -Ausgaben verwenden Typen aus `Cleanifico.Contracts`; Domain-/EF-Entities werden nicht direkt serialisiert.

Grund: Öffentliche Verträge bleiben stabil und enthalten nur erlaubte Felder. Persistenzdetails und private Setter der Domain gelangen nicht über die API-Grenze.

Datum: 2026-08-25

## DEC-015 – Migrationen werden kontrolliert ausgeführt

Status: Accepted

Entscheidung: Die API führt beim normalen Start weder `EnsureCreated` noch automatische EF-Migrationen aus. Schemaänderungen werden als Migrationen versioniert und pro Ziel-Datenbank explizit ausgerollt.

Grund: Jede Tenant-Datenbank muss gezielt, beobachtbar und ohne ungefragte destruktive Startaktion aktualisiert werden können.

Datum: 2026-08-25

## DEC-016 – Tenantlokales ASP.NET Core Identity

Status: Accepted

Entscheidung: `ApplicationUser`, Rollen und Identity-Tabellen liegen im bestehenden tenantlokalen `CleanificoDbContext`; es gibt keine zusätzliche `TenantId`.

Grund: Die eigene Instanz und Datenbank bilden bereits die Tenantgrenze und erlauben die Standardmechanismen von ASP.NET Core Identity.

Datum: 2026-08-25

## DEC-017 – Zentrale Rollen und Policies

Status: Accepted

Entscheidung: Zugriff wird über die festen Rollen `Owner`, `Administrator`, `Dispatcher`, `ObjectManager`, `Employee` und zentrale Authorization Policies gesteuert. Eine Active-User-Anforderung gilt als API-Fallback.

Grund: Einheitliche Policies vermeiden verstreute Rollenlogik und verhindern den Zugriff deaktivierter Konten.

Datum: 2026-08-25

## DEC-018 – Gemeinsamer Identity-Cookie für Office und API

Status: Accepted

Entscheidung: Die getrennten Office- und API-Hosts verwenden einen gemeinsamen sicheren Identity-Cookie und Data-Protection-Keyring. Für Mobile wird später ein eigener Bearer-Flow entworfen.

Grund: Office erhält eine serverseitige, widerrufbare Sitzung ohne eine vorgezogene JWT- oder Mobile-Architektur.

Datum: 2026-08-25

## DEC-019 – Expliziter Bootstrap ohne Standardpasswort

Status: Accepted

Entscheidung: Rollen werden idempotent angelegt; der erste Owner wird nur bei explizit aktivierter Konfiguration mit extern bereitgestelltem Initialpasswort erzeugt. Es gibt keine öffentliche Registrierung.

Grund: Ein fest codiertes oder automatisch bekanntes Administrationspasswort wäre ein vermeidbares Übernahmerisiko.

Datum: 2026-08-25

## DEC-020 – Schutz des letzten aktiven Owners

Status: Accepted

Entscheidung: Der letzte aktive Owner kann weder deaktiviert werden noch seine Owner-Rolle verlieren. Benutzerkonten werden deaktiviert statt physisch gelöscht.

Grund: Ein Tenant darf sich nicht selbst aus der Administration aussperren; die Kontohistorie bleibt erhalten.

Datum: 2026-08-25

## DEC-021 – Zeittypen sind frei konfigurierbare Kundendaten

Status: Accepted

Entscheidung: `TimeType` ist ein normaler tenantlokaler Datensatz und kein Enum. Die Standard-Zeittypen werden genau einmal idempotent angelegt und besitzen keine System-, Built-in- oder Sperrkennzeichen. Ein technischer Initialisierungsmarker verhindert jedes spätere Reseeding oder Zurücksetzen.

Grund: Jeder Tenant muss Namen, Codes, Eigenschaften, Status und Bestand vollständig an die eigene Arbeitsweise anpassen können. Startlogik darf produktive Kundendaten niemals überschreiben.

Datum: 2026-08-25

## DEC-022 – Historische Zeitbuchungen speichern Zeittyp-Snapshots

Status: Accepted

Entscheidung: Spätere `TimeEntry`-Datensätze speichern neben `TimeTypeId` mindestens Name, Arbeitszeit-, Bezahlt-, Objektpflicht- und Abwesenheitsmerkmal des Zeittyps als Snapshot. Ein verwendeter Zeittyp wird regulär deaktiviert statt gelöscht.

Grund: Änderungen an frei konfigurierbaren Zeittypen dürfen historische Arbeitszeitbuchungen nicht rückwirkend verändern oder fachlich umdeuten.

Datum: 2026-08-25

## DEC-023 – Customer ist Auftraggeber mit eigener Verwaltungsadresse

Status: Accepted

Entscheidung: `Customer` bildet den tenantlokalen Auftraggeber ab. Seine änderbare `CustomerNumber` ist tenantlokal eindeutig. Ein einzelner Ansprechpartner und die Verwaltungsadresse liegen zunächst direkt am Customer; spätere Objects besitzen eigene Einsatzadressen.

Grund: Das aktuelle Kundenmodul bleibt ohne vorgezogene Kontakt- oder Objektarchitektur nutzbar und trennt dennoch Kundenverwaltung und spätere Reinigungsorte fachlich sauber.

Datum: 2026-08-26

## DEC-024 – Referenzierte Kunden werden deaktiviert

Status: Accepted

Entscheidung: Customer darf nur solange physisch gelöscht werden, wie keine fachlichen Referenzen bestehen. Mit späteren Objekten, Verträgen, Rechnungen oder historischen Daten verhindern Fremdschlüssel das Löschen; dann ist Deaktivierung der reguläre Lifecycle.

Grund: Historische und abrechnungsrelevante Daten müssen ihren Auftraggeber dauerhaft nachvollziehbar behalten.

Datum: 2026-08-26

## DEC-025 – CleaningObject ist verpflichtend einem Customer zugeordnet

Status: Accepted

Entscheidung: `CleaningObject` bildet den tenantlokalen Reinigungsort mit eigener Adresse und eigener änderbarer Objektnummer ab. Jedes Objekt gehört genau einem Customer; ein Customer besitzt null bis viele Objekte. Der Fremdschlüssel verwendet `Restrict`, und der Application Service verhindert das physische Löschen eines Kunden mit Objekten durch einen verständlichen Konflikt.

Grund: Auftraggeber und Einsatzort bleiben fachlich getrennt, während Referenzintegrität und spätere historische Nachvollziehbarkeit ohne Cascade Delete geschützt sind.

Datum: 2026-08-26

## DEC-026 – Lizenzgrenze ist zentral, zusätzlich und fail-closed

Status: Superseded by DEC-027

Entscheidung: Cleanifico kapselt die zentrale FergensHub-Prüfung hinter `ILicenseService`. Die Business-Policies verlangen zusätzlich zu aktivem Benutzer und Rolle eine gültige Lizenz. Solange FergensHub keinen belastbaren externen Query-Contract besitzt, liefert der Infrastructure-Adapter kontrolliert `Unavailable` und sperrt Businessfunktionen; es gibt keine lokale Lizenzdatenbank, Konfigurationsfreischaltung oder erfundene HTTP-API. Health, Login/Logout, Sessionprüfung und Lizenzstatus bleiben erreichbar.

Grund: Die Sicherheitsgrenze ist jetzt eindeutig und testbar, ohne eine konkurrierende Lizenzquelle oder eine Scheinintegration zu schaffen. Der spätere echte FergensHub-Adapter kann den Port ersetzen, sobald Identifikation, Authentifizierung, DTOs und Fehlersemantik veröffentlicht sind.

Datum: 2026-08-26

## DEC-027 – Installationsgebundene signierte Leases nach AssetFico-Muster

Status: Accepted

Entscheidung: Cleanifico übernimmt AssetFicos lokalen License State und die signierte Lease als einzige produktive Lizenzquelle. Nur eine zum Produkt `CLEANIFICO`, zur persistenten Installation-ID und zum Feature `base` passende ECDSA-P-256-Lease erlaubt in `Valid` oder `Grace` den Businesszugriff. Aktivierung und Refresh verwenden ausschließlich die bestehenden AssetFico-Routen und Credentialformate. Identity und Rollen bleiben eine zusätzliche, unabhängige Sicherheitsgrenze.

Grund: Das Offline-Lease-Verfahren ist ein bereits implementiertes Fergenix-Muster, erlaubt kontrollierten Betrieb bei temporärer Nichterreichbarkeit und verhindert lokale Konfigurationsfreischaltungen. Das aktuelle FergensHub-Repository muss die bereits definierten Runtime-Verträge noch serverseitig implementieren; Cleanifico erfindet dafür keinen zweiten Contract.

Datum: 2026-08-26

## DEC-028 – Fachliche Mitarbeiter bleiben von Identity getrennt

Status: Accepted

Entscheidung: `Employee` ist eine eigenständige tenantlokale Personal-Entity und besitzt zunächst keine verpflichtende Beziehung zu `ApplicationUser`. Beschäftigungsarten werden als frei pflegbarer Text statt als Enum gespeichert. Personalnummern sind tenantlokal eindeutig und änderbar. Physisches Löschen ist nur ohne fachliche Referenzen zulässig; spätere referenzierende Module müssen Löschen per Fremdschlüssel verhindern.

Grund: Personalstammdaten müssen auch für Beschäftigte ohne Login existieren, während technische Benutzerkonten nicht automatisch Personal darstellen. Frei pflegbare Beschäftigungsarten vermeiden eine unbegründete globale HR-Taxonomie, und der Lifecycle bleibt für spätere historische Daten erweiterbar.

Datum: 2026-08-26
