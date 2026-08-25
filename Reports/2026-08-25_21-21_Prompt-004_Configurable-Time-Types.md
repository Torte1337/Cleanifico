# Prompt 004 – Konfigurierbare Zeittypen

## Auftrag

Ein vollständiges, tenantlokales TimeType-Modul wurde nach dem bestehenden CleaningType-Muster umgesetzt. Zeitbuchungen und angrenzende Fachmodule blieben ausdrücklich außerhalb des Scopes.

## Umgesetzt

- `TimeType` mit Name, Code, Beschreibung, Arbeitszeit-/Bezahlungs-/Objekt-/Abwesenheitsmerkmalen, optionaler Hex-Farbe, frei wählbarer Sortierung, Aktivstatus und UTC-Auditzeiten.
- Normalisierung und Validierung in der Domain; Name und Code sind erforderlich, Code wird großgeschrieben, Name und Code sind tenantlokal eindeutig.
- Application Service und Repository-Port für Liste, Einzelabruf, Anlegen, Bearbeiten, Aktivieren, Deaktivieren und Löschen.
- EF-/MySQL-Mapping mit Defaults, Indizes und eindeutigen case-insensitive Name-/Code-Indizes.
- Migration `20260825191646_AddConfigurableTimeTypes` mit `TimeTypes` und technischem Initialisierungsmarker; bestehende Migrationen blieben unverändert.
- Einmalige, idempotente Startdaten `ARB`, `PAU`, `FAH`, `URL`, `KRK`, `SCH`, `BES`. Die Datensätze sind vollständig änder- und löschbar; ein atomar gesetzter Marker verhindert jedes spätere Reseeding oder Zurücksetzen.
- REST API unter `/api/time-types` mit Suche und Statusfilter sowie rollenbasierter Autorisierung: Owner/Administrator lesen und verwalten, Dispatcher/ObjectManager lesen, Employee kein Zugriff; anonym `401`, ohne Recht `403`.
- Office-Seite `/zeittypen` mit deutscher Suche, Statusfilter, Eigenschaftstabelle, Anlegen/Bearbeiten, Aktivieren/Deaktivieren und endgültigem Löschen.
- Dokumentation der historischen Snapshot-Pflicht für spätere `TimeEntry`-Datensätze und der künftigen Deaktivierung verwendeter Zeittypen.

## Tests

Ergänzt wurden Domain-, Application-, Persistenz-, API-, Autorisierungs- und Webtests für Pflichtwerte, Normalisierung, Eindeutigkeit, vollständige Änderbarkeit, Lifecycle, Standarddaten, Idempotenz, Nicht-Zurücksetzen und Rollenmatrix.

- `dotnet build`: erfolgreich, 0 Warnungen, 0 Fehler.
- `dotnet test`: 110/110 bestanden, 0 fehlgeschlagen, 0 übersprungen.
- EF-Tools-Hinweis: lokal 9.0.17 gegenüber Runtime 9.0.19; Migration wurde erfolgreich erzeugt.

## Entscheidungen und offen

Standard-Zeittypen sind normale Kundendaten ohne System-/Sperrkennzeichen. Historische Zeitbuchungen müssen später mindestens ID, Name und die vier fachlichen Bool-Eigenschaften als Snapshot speichern. Nicht umgesetzt wurden `TimeEntry`, Mitarbeiter, Kunden, Objekte, Planung, Mobile, FergensHub, Discovery, MFA, Abwesenheitsworkflows und Lohnabrechnung.

## Git

Alle Prompt-004-Änderungen bleiben uncommitted. Es wurde weder ein Branch erstellt noch committed oder gepusht; `git diff --check` war ohne Befund.
