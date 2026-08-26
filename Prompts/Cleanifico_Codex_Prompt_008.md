# Cleanifico – Codex Prompt 008
## Bestehende AssetFico-/Fergenshub-Lizenzierung analysieren und auf Cleanifico übertragen

Arbeite primär im bestehenden Cleanifico-Repository:

`~/Documents/Projekte/FergenixLabs/Cleanifico`

Zusätzlich darfst du folgende Schwesterprojekte **nur lesend analysieren**:

`~/Documents/Projekte/FergenixLabs/AssetFico`
`~/Documents/Projekte/FergenixLabs/Fergenshub`

Wichtig:

> AssetFico und Fergenshub dürfen in diesem Prompt nicht verändert werden.

Alle Implementierungsänderungen dieses Prompts erfolgen ausschließlich in:

`~/Documents/Projekte/FergenixLabs/Cleanifico`

Nutze `AGENTS.md` und nur die relevanten Dateien unter `docs/`.

# Ziel

Cleanifico soll **genau nach dem bereits vorhandenen Lizenzierungsprinzip von AssetFico/Fergenshub** lizenziert werden.

Keine neue Lizenzarchitektur erfinden.
Keine zentrale Runtime-API in Fergenshub neu bauen.
Keine andere Tenant-Strategie einführen.

Weiterhin gilt:
- jeder Cleanifico-Kunde besitzt eine eigene Cleanifico-API
- jeder Cleanifico-Kunde besitzt eine eigene MySQL-Datenbank
- Fergenshub ist die interne Verwaltungs-/Lizenzplattform
- Cleanifico soll das vorhandene AssetFico-Lizenzmuster übernehmen

# Schritt 1 – gezielte Analyse

Analysiere in `AssetFico` und `Fergenshub` ausschließlich die für Lizenzierung relevanten Bereiche.

Finde konkret heraus:
- wie AssetFico seinen Tenant/Kunden identifiziert
- wie eine AssetFico-Lizenz technisch repräsentiert wird
- wie Fergenshub diese Lizenz verwaltet
- wie Aktiv/Inaktiv bzw. Gültigkeit bestimmt wird
- wie Produktzuweisung funktioniert
- wie Features/Limits behandelt werden
- ob Lizenzdaten lokal gespeichert, signiert, übertragen oder anderweitig geprüft werden
- welche Services, Contracts, DTOs oder Konfigurationswerte beteiligt sind
- wie sich AssetFico bei ungültiger Lizenz verhält
- welche Teile für Cleanifico wiederverwendbar sind

Keine vollständige Analyse der Schwesterprojekte durchführen.

# Schritt 2 – aktuellen Cleanifico-Stand prüfen

Cleanifico besitzt bereits aus Prompt 007:
- `ILicenseService`
- Lizenz-Policies
- `/api/license/status`
- `/lizenz`
- fail-closed Verhalten
- Absicherung der bestehenden Businessmodule

Prüfe, welche Teile davon zum echten AssetFico-/Fergenshub-Muster passen.

Bestehende funktionierende Cleanifico-Strukturen behalten, wenn sie kompatibel sind.

# Schritt 3 – echte Integration

Übertrage das bestehende AssetFico-Lizenzprinzip auf Cleanifico.

Dabei:
- vorhandene Patterns und Contracts bevorzugen
- keine parallele zweite Lizenzlogik erzeugen
- `ILicenseService` sinnvoll an die reale Implementierung anbinden oder anpassen
- Konfiguration sauber über Settings / Environment Variables / Secrets
- keine Secrets committen
- keine fest codierten Tenant-/Lizenzdaten
- vorhandene Identity- und Rollen-Policies unverändert als zusätzliche Sicherheitsebene behalten

Wenn das AssetFico-/Fergenshub-Modell eine Provisionierung oder lokale Lizenzinformation vorsieht, übernimm dieses Prinzip für Cleanifico.

Wenn dafür zwingend eine Änderung in Fergenshub erforderlich wäre:

> Fergenshub in diesem Prompt NICHT verändern.

Stattdessen:
- Cleanifico soweit möglich vorbereiten
- exakt dokumentieren, welche konkrete Änderung in Fergenshub noch benötigt wird
- keinen Fake-Contract oder erfundenen Endpunkt bauen

# Discovery

Discovery ist **nicht** Bestandteil dieses Prompts, außer AssetFico verwendet sie zwingend direkt für die Lizenzierung.

Keine MAUI-Discovery bauen.
Keine Firmencode-Anmeldung bauen.

# Verhalten

Nach Möglichkeit soll Cleanifico dieselben Lizenzzustände und dasselbe Verhalten wie AssetFico verwenden.

Businessfunktionen bleiben nur bei gültiger Lizenz nutzbar.

Mindestens weiterhin schützen:
- CleaningTypes
- TimeTypes
- Customers
- CleaningObjects
- zugehörige Office-Seiten

`/health` bleibt erreichbar.

Login/Logout bleiben funktionsfähig, damit eine Lizenzfehlermeldung angezeigt werden kann.

# Tests

Erweitere bzw. korrigiere Tests entsprechend der echten Lizenzimplementierung.

Mindestens:
- gültige Lizenz -> Businesszugriff möglich
- ungültige Lizenz -> Businesszugriff blockiert
- vorhandene Identity-/Rollenprüfung bleibt wirksam
- `/health` bleibt erreichbar
- `/api/license/status` liefert sinnvollen Status
- bestehende Business-Tests bleiben grün

Keine echten produktiven Fergenshub-/AssetFico-Systeme in Tests verändern.

# Dokumentation

Aktualisiere nur dauerhaft relevantes Wissen:
- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- ggf. `AGENTS.md`
- ggf. `README.md`

Dokumentiere insbesondere:
- wie die AssetFico-Lizenzierung tatsächlich funktioniert
- welche Teile Cleanifico übernommen hat
- welche Teile bewusst nicht übernommen wurden
- ob noch eine konkrete Fergenshub-Anpassung nötig ist
- dass jede Firma weiterhin eine eigene Cleanifico-API und MySQL-Datenbank besitzt

# Nicht Bestandteil

Nicht implementieren:
- neue zentrale Fergenshub-Lizenz-API
- Änderungen in AssetFico
- Änderungen in Fergenshub
- MAUI
- Discovery, sofern nicht zwingend Teil des bestehenden Lizenzpatterns
- Mitarbeiter
- Verträge
- Arbeitszeiten
- Dienstplanung
- MFA
- neue Businessmodule

Keine Feature-Ausweitung.

# Abschluss

Am Ende in Cleanifico ausführen:

```bash
dotnet build
dotnet test
```

Ziel:
- 0 Fehler
- 0 Warnungen
- alle Tests grün

Erstelle:

`Reports/YYYY-MM-DD_HH-mm_Prompt-008_AssetFico-Licensing-Integration.md`

Prüfe:

```bash
git status
git diff --stat
git diff --check
```

Stelle sicher, dass Git-Änderungen ausschließlich Cleanifico betreffen.

Nicht automatisch committen oder pushen.

Antworte kompakt mit:
- wie AssetFico/Fergenshub die Lizenzierung tatsächlich lösen
- was davon in Cleanifico übernommen wurde
- ob Fergenshub noch angepasst werden muss
- Lizenzverhalten in Cleanifico
- Build
- Tests
- Report
- Git-Status
- Empfehlung für Prompt 009
