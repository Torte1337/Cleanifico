# Cleanifico – Offene Aufgaben

## Nächster sinnvoller Schritt

- [ ] Tenantlokale Authentifizierung und Autorisierung als eigenen vertikalen Sicherheitsschnitt konzipieren und umsetzen; Rollen- und Anmeldeanforderungen vorher konkretisieren und anschließend die Cleaning-Type-Endpunkte absichern.

## Offen

- [ ] Vor Produktiveinsatz Identity, Autorisierung und FergensHub-Lizenzprüfung vollständig integrieren; die Cleaning-Type-Endpunkte sind bis dahin nicht produktionsbereit.
- [ ] Vor der Lizenz-/Discovery-Implementierung die vorhandenen FergensHub-, Assetfico- und Discovery-Verträge beziehungsweise Referenzimplementierungen bereitstellen und prüfen; keine parallele lokale Lizenzlogik ergänzen.
- [ ] Einen kontrollierten Rollout-Prozess für Migrationen über alle tenantlokalen Datenbanken definieren.
- [ ] Echte MySQL-Integrationstests mit eindeutig isolierter, kurzlebiger Testdatenbank ergänzen; bis dahin bleiben EF-Metadaten- und HTTP-Tests bewusst datenbankfrei.
- [ ] EF-Core-/Pomelo-10-Kompatibilität erneut prüfen, sobald ein stabiler Pomelo-10-Provider veröffentlicht ist.
- [ ] Automatisierten CI-Lauf für Restore, Build und Tests einrichten.

## Später

- [ ] Docker- und Deployment-Strategie pro Tenant aus vorhandenen FergensHub-/Assetfico-Patterns ableiten.
- [ ] `Cleanifico.Mobile` mit .NET MAUI ergänzen.
- [ ] Offline-Synchronisierung und SQLite-Cache für Mobile konzipieren.

## Erledigt

- [x] Initiale `.slnx`, Schichten, Hosts, Testprojekte und Wissensbasis erstellt.
- [x] Repositoryweite Arbeitsregeln in `AGENTS.md` festgelegt.
- [x] EF Core, Pomelo, `CleanificoDbContext`, MySQL-Konfiguration und initiale Migration eingerichtet.
- [x] Reinigungstypen als ersten End-to-End-Schnitt mit Domain, Application, Contracts, API, Web und Tests umgesetzt.
