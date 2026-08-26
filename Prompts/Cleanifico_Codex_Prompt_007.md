# Cleanifico – Codex Prompt 007
## FergensHub-Lizenzierung integrieren

Arbeite im bestehenden Repository:

`~/Documents/Projekte/FergenixLabs/Cleanifico`

Nutze `AGENTS.md` und nur die relevanten Dateien unter `docs/`.

Analysiere zusätzlich gezielt vorhandene Referenzimplementierungen unter:

`~/Documents/Projekte/FergenixLabs/`

insbesondere **FergensHub**, **Assetfico** und eine vorhandene **Discovery API**, sofern dort vorhanden.

Wichtig:

> Keine neue Lizenzarchitektur erfinden, wenn FergensHub/Assetfico bereits funktionierende Contracts, Services oder Patterns besitzen.

# Ziel

Cleanifico soll wie Assetfico zentral über **FergensHub** lizenziert werden.

Nach Prompt 007 soll Cleanifico eine echte, dokumentierte Lizenzgrenze besitzen.

Dabei gilt weiterhin:

- jeder Cleanifico-Kunde besitzt eigene API/Instanz
- jeder Kunde besitzt eigene MySQL-Datenbank
- FergensHub ist die zentrale Quelle für Produkt-/Lizenzstatus
- Cleanifico entscheidet nicht unabhängig mit einer konkurrierenden lokalen Lizenzlogik

# Zuerst prüfen

Prüfe gezielt:

- wie Assetfico eine Lizenz identifiziert
- wie FergensHub Produkte/Tenants/Lizenzen modelliert
- welche Contracts/DTOs existieren
- wie Lizenzstatus und Laufzeit geprüft werden
- ob Feature Flags / Limits vorhanden sind
- wie Tenant-Endpoints verwaltet werden
- welche Discovery-Verträge bereits existieren
- ob bestehende Shared Libraries oder HTTP-Clients wiederverwendbar sind

Dokumentiere nur relevante Erkenntnisse.

Keine vollständige Neu-Analyse aller Schwesterprojekte.

# Integration

Wenn eine klare vorhandene Referenzimplementierung existiert:

- übernehme deren Konzept für Cleanifico
- verwende bestehende Contracts/Patterns soweit technisch sinnvoll
- implementiere einen klaren Cleanifico-Lizenzservice bzw. die bereits vorhandene entsprechende Abstraktion
- registriere benötigte Services sauber per DI
- Konfiguration ausschließlich über Settings/Environment Variables/User Secrets
- keine Secrets committen

Wenn kein belastbarer realer Contract vorhanden ist:

> Keine Fake-FergensHub-Endpunkte erfinden.

Dann nur eine saubere interne Abstraktionsgrenze schaffen, den fehlenden externen Contract dokumentieren und keine Scheinintegration vortäuschen.

# Lizenzstatus

Mindestens sinnvoll unterscheiden, sofern das bestehende FergensHub-Modell dies unterstützt:

- aktiv
- abgelaufen
- deaktiviert/gesperrt
- nicht gefunden
- FergensHub vorübergehend nicht erreichbar

Die tatsächlichen Statusnamen sollen sich an FergensHub orientieren.

# Verhalten in Cleanifico

Geschäftliche Cleanifico-Funktionen dürfen nur bei gültiger Lizenz nutzbar sein.

Schütze mindestens die vorhandenen Businessbereiche:

- `/api/cleaning-types`
- `/api/time-types`
- `/api/customers`
- `/api/objects`
- zugehörige Office-Webseiten

`/health` muss weiterhin für technische Überwachung erreichbar bleiben.

Login/Logout sollen nicht unnötig kaputt gemacht werden, wenn dadurch keine sinnvolle Fehleranzeige mehr möglich wäre.

Für ungültige Lizenz eine verständliche zentrale Office-Seite bzw. Meldung bereitstellen, z. B.:

`/lizenz`

mit Informationen wie:

- Lizenz nicht aktiv
- Lizenz abgelaufen
- Lizenzprüfung derzeit nicht möglich

Keine internen URLs, Secrets oder Stacktraces anzeigen.

# Autorisierung vs. Lizenzierung

Bestehende Identity-/Policy-Autorisierung bleibt bestehen.

Lizenzprüfung ist eine zusätzliche Ebene:

`gültige Lizenz + authentifiziert + passende Policy`

Nicht bestehende Rollen/Policies durch Lizenzlogik ersetzen.

# Ausfallsicherheit

Orientiere dich am bestehenden Assetfico/FergensHub-Verhalten.

Falls dort Caching/Grace Period vorhanden ist, übernehme das Pattern.

Falls nicht:

- keine komplizierte eigene Offline-Lizenzlogik erfinden
- Fehlerzustände sauber behandeln
- externe Fehler nicht als 500-Stacktrace durchreichen

Keine aggressive FergensHub-Abfrage bei jedem einzelnen UI-Render, wenn ein vorhandenes besseres Pattern existiert.

# Discovery

Discovery nur soweit integrieren oder vorbereiten, wie es für die vorhandene Cleanifico-Tenant-/Lizenzarchitektur tatsächlich nötig ist.

Noch NICHT bauen:

- MAUI Login
- Firmencode-Eingabe in MAUI
- mobile Tenant-Auswahl
- Offline-Discovery

Falls Discovery für Prompt 007 nicht erforderlich ist, nur bestehende Verträge dokumentieren und für später belassen.

# Feature Flags / Limits

Wenn FergensHub bereits echte Feature-/Limit-Mechanismen besitzt, Cleanifico dafür technisch vorbereiten bzw. bestehende Mechanismen anbinden.

Noch keine künstlichen Tarife oder Limits erfinden.

Keine Features sperren, die FergensHub aktuell nicht tatsächlich modelliert.

# Tests

Erweitere die Tests sinnvoll.

Mindestens, soweit mit der echten Architektur möglich:

- gültige Lizenz -> Businesszugriff möglich
- ungültige/abgelaufene Lizenz -> Businesszugriff blockiert
- anonymer Zugriff bleibt 401, wo Auth erforderlich ist
- fehlende Rolle bleibt 403, auch bei gültiger Lizenz
- Healthcheck bleibt erreichbar
- externe Lizenzfehler werden kontrolliert behandelt
- vorhandene CleaningType-, TimeType-, Customer- und Object-Tests bleiben grün

Externe FergensHub-Aufrufe in Tests sauber mocken/faken über die vorhandene Abstraktion, nicht über echte Produktionssysteme.

# Dokumentation

Aktualisiere nur dauerhaft relevantes Wissen:

- `docs/PROJECT_MEMORY.md`
- `docs/ARCHITECTURE.md`
- `docs/DECISIONS.md`
- `docs/TODO.md`
- ggf. `AGENTS.md`
- ggf. `README.md`

Dokumentiere insbesondere:

- welche FergensHub-/Assetfico-Patterns übernommen wurden
- wie Cleanifico seine Lizenz prüft
- welche Konfiguration benötigt wird
- Verhalten bei ungültiger Lizenz
- Verhalten bei Nichterreichbarkeit
- welche Discovery-Themen bewusst später folgen

# Nicht Bestandteil

Nicht implementieren:

- MAUI
- MFA
- neue Businessmodule
- Mitarbeiter
- Verträge
- Arbeitszeiten
- Dienstplanung
- Kundenportal
- erfundene FergensHub-APIs
- eigene parallele Lizenzdatenbank in Cleanifico

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

`Reports/YYYY-MM-DD_HH-mm_Prompt-007_FergensHub-Licensing.md`

Prüfe:

```bash
git status
git diff --stat
git diff --check
```

Nicht automatisch committen oder pushen.

Antworte kompakt mit:

- analysierte Referenzimplementierung
- umgesetzte Lizenzintegration
- Verhalten bei gültiger/ungültiger Lizenz
- Discovery-Status
- Build
- Tests
- Report
- Git-Status
- Empfehlung für Prompt 008
