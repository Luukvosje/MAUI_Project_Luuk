# GPS-testdata

| Bestand | Doel |
|---------|------|
| `points.json` | Oudere synthetische trace (veel samples, vaste coördinaten bij stops). |
| `points-evaluator-saved.json` | Realistischere trace: rijden gefilterd zoals `GpsSampleEvaluator`, stops met wander van een landmeter in de tuin. |

Opnieuw genereren van `points-evaluator-saved.json`:

```bash
python tests/TimeOn.UnitTests/Application/WorkSessions/GpsData/generate_points_evaluator_saved.py
```

**Rijden:** ruwe platformsamples elke 10 s, daarna alleen punten waar `elapsed > interval` (10 s bij snel / 30 s bij langzaam) en `accuracy <= 50 m`.

**Op locatie (tuin):** samples elke 30 s met ~12–30 m wander rond de klant. Alle tuin-samples staan in dit bestand zodat `SegmentClassifier` nog minstens 5 punten binnen 2 minuten ziet (strikte `> 30 s`-filtering kan geen vijf saves binnen een venster van 120 s opleveren).

**Snelheid:** meter per seconde (zelfde als Android `Location.Speed`).
