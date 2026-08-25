# Cleanifico – Projektwissen

## Zweck und Zielgruppen

Cleanifico wird eine kommerzielle Betriebssoftware für Gebäudereinigungsunternehmen. Cleanifico Office richtet sich an Geschäftsführung, Verwaltung, Disposition, Objektleitung und Personalverwaltung. Die spätere Mobile-App richtet sich an Reinigungskräfte, Vorarbeiter und Objektleiter im Außendienst.

## Wichtige Fachbegriffe

- **Tenant:** ein lizenzierter Gebäudereinigungsbetrieb; besitzt eine eigene Cleanifico-Instanz und MySQL-Datenbank.
- **Kunde/Auftraggeber:** Geschäftskunde eines Tenants; kann mehrere Objekte besitzen.
- **Objekt:** konkreter Einsatz- beziehungsweise Reinigungsort eines Auftraggebers.
- **Reinigungstyp / Zeittyp:** tenantseitig konfigurierbare Klassifikation von Leistungen beziehungsweise Zeiten.

## Aktueller technischer Stand

- Solution `Cleanifico.slnx` mit sechs Produkt- und vier Testprojekten auf `net10.0`.
- API stellt derzeit ausschließlich `GET /health` bereit.
- Web ist eine minimale Blazor-Web-App mit Server-Interaktivität; sie greift später per HTTP auf die API zu.
- Fachmodule, Datenbankzugriff, Authentifizierung, Lizenzprüfung und Discovery sind noch nicht implementiert.
- EF Core, Pomelo und Identity-Persistenz werden erst mit dem ersten echten Persistenzmodul eingebunden.

## Zentrale Dateien und Typen

- `src/*/AssemblyReference.cs`: stabile Assembly-Referenzen für Scans und Architekturtests.
- `src/Cleanifico.Api/ApiApplication.cs`: API-Aufbau, technische Middleware und Health-Endpunkt.
- `tests/Architecture/RepositoryStructure.cs`: gemeinsame Prüfung der Projekt- und Solution-Struktur.
- `docs/ARCHITECTURE.md`: verbindliche Beschreibung des aktuellen und geplanten Aufbaus.
- `docs/DECISIONS.md`: akzeptierte Architekturentscheidungen.

## Konventionen

- Abhängigkeiten zeigen nach innen; Domain und Contracts kennen keine Infrastruktur.
- Öffentliche API-Nachrichten gehören nach `Cleanifico.Contracts`.
- Keine Fachlogik in API, Web oder Infrastructure.
- Nullable Reference Types und implizite Usings bleiben aktiviert.
- Keine Template-Demos oder ungenutzten Pakete.
- Nach jedem abgeschlossenen Prompt: Wissensdateien prüfen und einen Report unter `Reports/` anlegen.
