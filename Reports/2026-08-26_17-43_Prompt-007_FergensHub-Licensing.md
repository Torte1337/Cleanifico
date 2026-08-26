# Prompt 007 – FergensHub-Lizenzgrenze

## Analysierte Referenzimplementierung

- Geprüft wurde gezielt `FergensHub/FergensHUB`: `Tenant`, `Product`, `TenantProduct`, `ProductFeature`, `TenantProductFeature`, effektive Aktivitätsregel, Application-Contracts und Architekturberichte.
- FergensHub führt aktuell interne Produkt-/Featurezuordnungen. Effektiv aktiv bedeutet Tenant, Product und TenantProduct aktiv; Features verlangen zusätzlich aktives ProductFeature und aktivierte TenantProductFeature.
- Der vorhandene API-Host besitzt keinen externen Lizenzabfrage-Controller/-Contract. Laufzeit/Ablauf, Tarife, Limits, Client-Authentifizierung, Fehlersemantik und Cache-/Grace-Period sind nicht modelliert. Assetfico und Discovery waren im bereitgestellten Projektstamm nicht vorhanden.

## Umgesetzte Lizenzintegration

- `ILicenseService` als Application-Port mit `Active`, `Inactive`, `NotFound` und `Unavailable`; kein erfundener `Expired`-Status ohne zentrales Laufzeitmodell.
- Fail-closed Infrastructure-Adapter liefert bis zu einem realen FergensHub-Query-Contract kontrolliert `Unavailable`. Keine lokale Lizenzdatenbank, Konfigurationsfreischaltung, Fake-URL oder Secrets.
- Business-Policies für CleaningTypes, TimeTypes, Customers und CleaningObjects verlangen zusätzlich `LicensedProductRequirement`. Identity, Active-User- und Rollenprüfungen bleiben erhalten.
- Ungültige/nicht prüfbare Lizenz liefert kontrolliertes `403 ProblemDetails`; anonym bleibt `401`, fehlende Rolle bei gültiger Lizenz `403`.
- `/api/license/status`, Office-Seite `/lizenz`, zentraler Redirect und einminütiger Office-Circuit-Cache ergänzt. `/health`, Login/Logout, Sessionprüfung und Benutzeradministration bleiben lizenzunabhängig.

## Konfiguration und Discovery

Aktuell ist bewusst keine FergensHub-Endpoint-/Credential-Konfiguration definiert: Identifikation, Authentifizierung und DTOs fehlen im Quellsystem. Sobald FergensHub den externen Contract veröffentlicht, werden Tenant-/Produktidentifikation, Endpoint und Credentials ausschließlich über Settings, Environment Variables oder User Secrets an einen echten Infrastructure-Adapter übergeben.

Discovery bleibt unimplementiert, weil kein vorhandener Contract oder Referenzdienst gefunden wurde. MAUI-/Firmencode-Flows wurden nicht vorgezogen.

## Tests und Build

- `dotnet build`: erfolgreich, 0 Warnungen, 0 Fehler.
- `dotnet test`: 198/198 bestanden, 0 fehlgeschlagen, 0 übersprungen.
- Getestet sind gültige/inaktive/fehlende/nicht prüfbare Lizenz, alle vier Business-APIs, `401`-/`403`-Kombination, kontrollierte Statusantwort, Health-Ausnahme, fail-closed Adapter und Office-Anzeige/Routenschutz.

## Dokumentation und Git

`AGENTS.md`, `README.md`, `docs/PROJECT_MEMORY.md`, `docs/ARCHITECTURE.md`, `docs/DECISIONS.md` und `docs/TODO.md` wurden dauerhaft aktualisiert. Alle Änderungen bleiben uncommitted; kein Branch, Commit oder Push.
