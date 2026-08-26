# Prompt 008 – AssetFico-Lizenzierung für Cleanifico

## Ergebnis

AssetFico und FergensHub wurden ausschließlich lizenzierungsbezogen und read-only analysiert. Cleanifico verwendet nun AssetFicos installationsgebundenes Offline-Lease-Prinzip anstelle des Prompt-007-Platzhalters.

## Tatsächliches Referenzmuster

- Eine Kundeninstallation besitzt eine persistente Installation-ID; Businessdaten bleiben je Kunde in eigener API und MySQL-Datenbank.
- Aktivierung und Refresh verwenden `POST api/licensing/v1/activate` beziehungsweise `POST api/licensing/v1/refresh`, `flk1_`-/`flr1_`-Credentials und einen stabilen Produktcode.
- Der lokale JSON-State enthält Installation-ID, geheimes Refresh-Credential, signierte Lease und letzten erfolgreichen Refresh; AssetFico speichert atomar, symlinkgeschützt und unter Unix mit Modus `0600`.
- Die installations- und produktgebundene Lease wird kanonisch mit `ECDSA-P256-SHA256` verifiziert, gilt 30 Tage und anschließend 14 Tage in Grace. Features sind Stringcodes; numerische Limits enthält der Vertrag nicht.
- Nur `Valid`/`Grace` erlaubt den Betrieb. Temporäre Nichterreichbarkeit lässt eine vorhandene Lease weiterwirken; ungültige Signatur, falsche Installation/falsches Produkt, Ablauf oder fachliche Sperre blockieren fail-closed.
- Discovery ist nicht an der AssetFico-Lizenzprüfung beteiligt.

## Cleanifico-Umsetzung

- `ILicenseService` auf `NotActivated`, `Valid`, `Grace`, `Expired`, `Invalid` und Lease-Metadaten angepasst.
- Sicherer lokaler State, ECDSA-P-256-Verifikation, Produktcode `CLEANIFICO`, Pflichtfeature `base`, Trust Anchor, Aktivierung, Refresh und periodische Erneuerung umgesetzt.
- Bestehende zentrale Lizenz-, Identity- und Rollen-Policies beibehalten; Lizenzverwaltung ist Owner/Administrator vorbehalten und selbst nicht lizenzpflichtig.
- `/api/license/status`, `/api/license/activate`, `/api/license/refresh` sowie `/lizenz` auf den realen Zustand umgestellt. Credentials werden weder ausgegeben noch protokolliert.
- `/health`, Login/Logout und Benutzerverwaltung bleiben lizenzunabhängig. CleaningTypes, TimeTypes, Customers, CleaningObjects und ihre Office-Seiten bleiben geschützt.
- State-Pfad, FergensHub-Basis-URL, Timeout und Refreshintervall sind konfigurierbar; der lokale geheime State ist in `.gitignore` ausgeschlossen.

## Noch erforderliche FergensHub-Anpassung

Das vorhandene FergensHub-Repository implementiert die von AssetFico bereits konsumierten Runtime-Routen derzeit nicht und besitzt dort keine Lizenzschlüssel-, Installations-, Refresh-Credential- oder Lease-Issuing-Persistenz. FergensHub muss die bestehenden AssetFico-Verträge serverseitig bereitstellen, `CLEANIFICO` mit Feature `base` verwalten und Leases mit dem bestehenden Signing-Key ausstellen. Es wurde kein abweichender oder erfundener Contract gebaut; AssetFico und FergensHub wurden nicht verändert. Im FergensHub-Worktree vorhandene Änderungen an `global.json` und `FergensHUB.sln` bestanden außerhalb dieses Prompts und wurden nicht berührt.

## Verifikation

- `dotnet build`: erfolgreich, 0 Fehler, 0 Warnungen.
- `dotnet test`: erfolgreich, 203/203 Tests.
- Ergänzt: Signatur-/Installationsbindung, Valid/Grace/Expired, Businesszugriff, Rollenfortbestand, Lizenzverwaltung, Status, Health und Office-Darstellung.
- `git diff --check`: ohne Befund.
- Keine Migration erforderlich; der License State ist bewusst dateibasiert und kein Fachdatenbankschema.
