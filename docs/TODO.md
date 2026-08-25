# Cleanifico – Offene Aufgaben

## Nächster sinnvoller Schritt

- [ ] Ein erstes schmales End-to-End-Modul fachlich festlegen; empfohlen sind die überschaubaren Reinigungstypen. Dabei Domain-Modell, Use Cases, API-Verträge, Persistenz und Tests gemeinsam aufbauen.

## Offen

- [ ] Mit dem ersten Persistenzmodul EF Core, Pomelo, MySQL-Konfiguration, `DbContext` und Migrationsstrategie versionskonsistent einrichten.
- [ ] Vor der Lizenz-/Discovery-Implementierung die vorhandenen FergensHub-, Assetfico- und Discovery-Verträge beziehungsweise Referenzimplementierungen bereitstellen und prüfen.
- [ ] Tenantlokale ASP.NET-Core-Identity-Strategie anhand konkreter Rollen- und Anmeldeanforderungen definieren.
- [ ] Automatisierten CI-Lauf für Restore, Build und Tests einrichten.

## Später

- [ ] Docker- und Deployment-Strategie pro Tenant aus vorhandenen FergensHub-/Assetfico-Patterns ableiten.
- [ ] `Cleanifico.Mobile` mit .NET MAUI ergänzen.
- [ ] Offline-Synchronisierung und SQLite-Cache für Mobile konzipieren.

## Erledigt

- [x] Initiale `.slnx`, Schichten, Hosts, Testprojekte und Wissensbasis erstellt.
