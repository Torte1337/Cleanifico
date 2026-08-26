# Cleanifico – Offene Aufgaben

## Nächster sinnvoller Schritt

- [ ] In FergensHub einen authentifizierten externen Effective-License-Query-Contract definieren und veröffentlichen; danach den fail-closed Cleanifico-Adapter durch den realen HTTP-Adapter ersetzen.

## Offen

- [ ] FergensHub-Contract für Tenant-/Produktidentifikation, Authentifizierung, Status-/Feature-DTOs, Fehlersemantik sowie Cache-/Grace-Period festlegen; Laufzeit, Tarife und Limits nur ergänzen, wenn das zentrale Modell sie tatsächlich führt.
- [ ] Assetfico- und Discovery-Referenzimplementierungen bereitstellen, sobald sie verfügbar sind; Discovery bleibt bis dahin unimplementiert.
- [ ] Produktiven Data-Protection-Keyring für API und Web persistent, zugriffsgeschützt und at rest verschlüsselt betreiben.
- [ ] Passwort-Reset, Einladung/Initialpasswort-Wechsel und MFA mit konkreten Produktanforderungen ergänzen; keine öffentliche Registrierung einführen.
- [ ] Eine optionale fachliche Verknüpfung von Benutzerkonto und späterem Mitarbeiterdatensatz erst mit dem Personalmodul festlegen.
- [ ] Beim späteren `TimeEntry`-Modul die beschlossenen Zeittyp-Snapshots speichern und verwendete Zeittypen gegen physisches Löschen absichern.
- [ ] Einen kontrollierten Rollout-Prozess für Migrationen über alle tenantlokalen Datenbanken definieren.
- [ ] Echte MySQL-Integrationstests mit eindeutig isolierter, kurzlebiger Testdatenbank ergänzen; bis dahin bleiben EF-Metadaten- und HTTP-Tests bewusst datenbankfrei.
- [ ] EF-Core-/Pomelo-10-Kompatibilität erneut prüfen, sobald ein stabiler Pomelo-10-Provider veröffentlicht ist.
- [ ] Automatisierten CI-Lauf für Restore, Build und Tests einrichten.

## Später

- [ ] Docker- und Deployment-Strategie pro Tenant aus vorhandenen FergensHub-/Assetfico-Patterns ableiten.
- [ ] `Cleanifico.Mobile` mit .NET MAUI ergänzen.
- [ ] Für Mobile einen eigenen Bearer-/Token-Anmeldefluss entwerfen.
- [ ] Offline-Synchronisierung und SQLite-Cache für Mobile konzipieren.

## Erledigt

- [x] Initiale `.slnx`, Schichten, Hosts, Testprojekte und Wissensbasis erstellt.
- [x] Repositoryweite Arbeitsregeln in `AGENTS.md` festgelegt.
- [x] EF Core, Pomelo, `CleanificoDbContext`, MySQL-Konfiguration und initiale Migration eingerichtet.
- [x] Reinigungstypen als ersten End-to-End-Schnitt mit Domain, Application, Contracts, API, Web und Tests umgesetzt.
- [x] Tenantlokales ASP.NET Core Identity, Rollen, zentrale Policies, Login/Logout und sicheren Owner-Bootstrap umgesetzt.
- [x] Cleaning-Type-Webseite und -API rollenbasiert abgesichert; Benutzerverwaltung mit Schutz des letzten aktiven Owners ergänzt.
- [x] Frei konfigurierbare Zeittypen mit einmaligen Standarddaten, Persistenz, API, rollenbasierter Office-Seite, Migration und Tests umgesetzt.
- [x] Kundenverwaltung für Auftraggeber mit eindeutiger Kundennummer, Kontakt-/Adressdaten, Detailansicht, API, Autorisierung, Migration und Tests umgesetzt.
- [x] Objektverwaltung mit verpflichtendem Customer-Bezug, eigener Objektadresse, CRUD/Lifecycle, API, Autorisierung, Migration, Kundendetail-Verknüpfung und Kunden-Löschschutz umgesetzt.
- [x] FergensHub-Referenz gezielt analysiert und eine zusätzliche fail-closed Lizenzgrenze mit zentralen API-/Office-Policies, kontrollierter Statusanzeige und Tests umgesetzt; keine externe Scheinintegration erfunden.
