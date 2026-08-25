# Prompt 003 – Identity und Autorisierung

## Auftrag und Ausgangsstand

Der abgebrochene Prompt 003 wurde auf Basis der bereits uncommitted begonnenen Security-Arbeiten fortgesetzt. Funktionierende Implementierungen wurden beibehalten; ergänzt wurden nur fehlende Absicherung, Tests, Migration und Dokumentation.

## Umgesetzt

- Tenantlokales ASP.NET Core Identity im bestehenden `CleanificoDbContext` mit `ApplicationUser`, eindeutiger E-Mail als Benutzername, Aktivstatus und UTC-Auditzeiten.
- Rollen `Owner`, `Administrator`, `Dispatcher`, `ObjectManager`, `Employee` sowie zentrale Policies für Office, Reinigungstypen, Benutzer und Rollen.
- Sichere Passwortregeln, Lockout nach fünf Fehlversuchen, Security-Stamp-Prüfung und Ausschluss deaktivierter Benutzer.
- Gemeinsamer sicherer Identity-Cookie für Web und API über einen persistenten Data-Protection-Keyring; Login, Logout, Sessionprüfung und `/zugriff-verweigert` ohne öffentliche Registrierung.
- Idempotente Rollenerzeugung und expliziter Bootstrap des ersten Owners über Konfiguration/Secrets ohne fest codiertes Passwort.
- Benutzerverwaltung unter `/administration/benutzer` für Owner/Administrator: anlegen, Profil ändern, aktivieren/deaktivieren und Rollen pflegen; kein physisches Löschen. Der letzte aktive Owner ist gegen Deaktivierung und Rollenentzug geschützt.
- Reinigungstypen: Lesen für Owner, Administrator, Dispatcher und ObjectManager; Schreiben für Owner und Administrator. API liefert anonym `401`, bei fehlender Rolle `403`.
- Migration `20260825183330_AddTenantIdentity` mit Identity-Tabellen und aktualisiertem Model Snapshot; keine automatische Migration beim Start.
- Dokumentation in `AGENTS.md`, `README.md`, `docs/PROJECT_MEMORY.md`, `docs/ARCHITECTURE.md`, `docs/DECISIONS.md` und `docs/TODO.md` aktualisiert.

## Sicherheitsentscheidungen

- Keine Secrets in Code, Logs oder Contracts; Owner-Bootstrap ist ausdrücklich zu aktivieren.
- API-Autorisierung nutzt zusätzlich eine Active-User-Fallback-Policy.
- Web-Sitzungen werden gegen die API geprüft und bei API-Ausfall fail-closed behandelt; lokales Logout bleibt auch dann möglich.
- Benutzerkonto und späterer fachlicher Mitarbeiterdatensatz bleiben getrennt.

## Tests und Build

- `dotnet build`: erfolgreich, 0 Warnungen, 0 Fehler.
- `dotnet test`: 75/75 bestanden, 0 fehlgeschlagen, 0 übersprungen.
- Abgedeckt sind Identity, Passwort-Hashing, Lockout, Deaktivierung, Rollen, Owner-Schutz, Bootstrap, EF-Mapping, API-Rollenmatrix mit `401`/`403` und Web-Routenschutz.

## Noch offen

Nicht Teil dieses Prompts und weiterhin offen: FergensHub-Lizenzierung, Discovery, MAUI/Mobile-Tokenfluss, MFA, Passwort-Reset/Einladungsfluss, fachliche Benutzer-Mitarbeiter-Zuordnung sowie produktive Keyring- und Tenant-Migrations-Betriebsprozesse.

## Git

Die Prompt-003-Änderungen bleiben uncommitted; es wurde weder ein Branch erstellt noch committed oder gepusht.
