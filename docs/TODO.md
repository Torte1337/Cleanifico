# Cleanifico – Offene Aufgaben

## Nächster sinnvoller Schritt

- [ ] In FergensHub die bereits von AssetFico definierten Routen `POST api/licensing/v1/activate` und `POST api/licensing/v1/refresh` serverseitig implementieren, `CLEANIFICO` mit Feature `base` anlegen und installationsgebundene signierte Leases ausstellen.

## Offen

- [ ] Produktive Lizenzkonfiguration (`Licensing__BaseUrl`, persistentes `Licensing__StatePath`) und gesichertes Backup des lokalen License State pro Tenant-Instanz in das Deployment aufnehmen.
- [ ] In FergensHub Lizenzschlüssel, Installationslimit/-widerruf, Refresh-Credentials, 30-Tage-Leases, 14-Tage-Grace und ECDSA-Signierung entsprechend dem AssetFico-Vertrag persistieren und verwalten; numerische Limits nur nach Erweiterung des gemeinsamen Lease-Vertrags ergänzen.
- [ ] Produktiven Data-Protection-Keyring für API und Web persistent, zugriffsgeschützt und at rest verschlüsselt betreiben.
- [ ] Passwort-Reset, Einladung/Initialpasswort-Wechsel und MFA mit konkreten Produktanforderungen ergänzen; keine öffentliche Registrierung einführen.
- [ ] Eine optionale Verknüpfung von `Employee` und `ApplicationUser` erst mit konkreten App-Zugangsanforderungen festlegen; keine automatische Zuordnung ableiten.
- [ ] Bei Arbeitszeiten, Objektzuweisungen, Einsätzen, Schlüsseln oder weiteren historischen Personaldaten zusätzliche Restrict-Fremdschlüssel auf Employee ergänzen; EmployeeContract schützt Mitarbeiter bereits gegen physisches Löschen.
- [ ] Sobald Arbeitszeiten, Abrechnungen oder andere historische Vorgänge EmployeeContract referenzieren, Restrict-Fremdschlüssel ergänzen und physisches Vertragslöschen verhindern.
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
- [x] AssetFico-Lizenzierung gezielt analysiert und das installationsgebundene signierte Lease-Muster einschließlich lokalem State, Aktivierung/Refresh, Grace, Hintergrund-Erneuerung und zentraler Policy-Grenze auf Cleanifico übertragen.
- [x] Fachliche Mitarbeiterverwaltung mit Personalstammdaten, frei pflegbarer Beschäftigungsart, Persistenz, API, Policies, Office-Seite und Trennung von Identity umgesetzt.
- [x] Historienfähige Mitarbeiterverträge als 1:n-Modul mit Datenübernahme aus Employee, Zeitraumkonfliktschutz, Restrict-FK, API, Policies, Office-Seite und Mitarbeiterdetail-Historie umgesetzt.
