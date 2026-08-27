# Prompt 010 – Mitarbeiterverträge

## Ergebnis

Das EmployeeContract-Modul ist als vollständiger vertikaler Schnitt umgesetzt: Domain, Application, Contracts, MySQL-Persistenz, REST API, zentrale Autorisierung, Office-Webseite, Mitarbeiterdetail-Erweiterung und Tests.

## Historienmodell und Employee-Refaktorierung

- `Employee` enthält nur persönliche Stamm-, Adress- und Kontaktdaten; die fünf bisherigen Beschäftigungsfelder wurden aus Domain, Contracts, API, Web und Schema entfernt.
- `EmployeeContract` ist die einzige Quelle für Zeitraum, Beschäftigungsart, Wochen-/Monatssollstunden und Urlaubsanspruch.
- Employee zu EmployeeContract ist 1:n mit verpflichtendem Restrict-FK. Mitarbeiter mit Vertrag sind im Application Service und in MySQL gegen physisches Löschen geschützt.
- Vertragsnummern sind tenantlokal case-insensitive eindeutig und änderbar. Aktive Vertragszeiträume eines Mitarbeiters dürfen sich nicht überschneiden; historische Folgeverträge bleiben eigenständige Datensätze.
- Verträge sind aktuell ohne spätere historische Referenzen physisch löschbar; solche Referenzen müssen künftig Restrict verwenden.

## Migration

- `20260826194117_AddEmployeeContracts` erstellt Tabelle, Indizes und FK.
- Vor dem Entfernen der alten Employee-Spalten werden vorhandene Beschäftigungswerte in deterministische `MIG-<EmployeeId>`-Verträge übernommen.
- Fehlt ein früherer Beschäftigungsbeginn, verwendet die Migration vorhandenes Ende oder Employee-Erstellungsdatum und kennzeichnet diese Ableitung in den Vertragsnotizen.
- Bestehende Migrationen wurden nicht verändert.

## API, Security und Office

- CRUD/Lifecycle und Filter `search`, `isActive`, `employeeId` unter `/api/employee-contracts`.
- `ViewEmployeeContracts`: Owner, Administrator, Dispatcher, ObjectManager; `ManageEmployeeContracts`: Owner, Administrator. Active-User- und Lizenzanforderung bleiben aktiv; anonym `401`, ohne Rolle oder Lizenz `403`.
- `/mitarbeitervertraege` bietet Suche, Status-/Mitarbeiterfilter, Liste, Details, Anlegen, Bearbeiten, Deaktivieren, Reaktivieren und bedingtes Löschen.
- Mitarbeiterdetails zeigen aktuellen Vertrag, Vertragsanzahl, Historie und Direktlinks.

## Verifikation

- `dotnet build`: erfolgreich, 0 Fehler, 0 Warnungen.
- `dotnet test`: 262/262 Tests erfolgreich (Domain 57, Application 37, Infrastructure 29, Web 40, API 99).
- Kein Commit und kein Push.

## Offen

- Echte MySQL-Integrationstests bleiben bis zu einer isolierten Testdatenbank offen.
- Spätere Arbeitszeiten, Abrechnungen oder andere historische Vorgänge müssen EmployeeContract per Restrict-FK referenzieren.
