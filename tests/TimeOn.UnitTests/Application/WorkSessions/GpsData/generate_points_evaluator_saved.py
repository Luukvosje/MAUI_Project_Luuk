"""Generates points-evaluator-saved.json.

Driving legs: raw GPS every 10 s, then filtered with GpsSampleEvaluator.ShouldSave rules.
On-site (garden): raw every 30 s with wander; all samples kept because the classifier needs
>=5 points within 2 minutes (impossible with only >30 s spaced saves in a 120 s window).
Speed is always m/s (Android convention).
"""

from __future__ import annotations

import json
import math
from datetime import datetime, timedelta, timezone
from pathlib import Path

FAST_SPEED_KMH = 10.0
FAST_INTERVAL_S = 10
DEFAULT_INTERVAL_S = 30
MAX_ACCURACY_M = 50.0

STOPS = [
    (51.5936, 5.5005),
    (51.5723, 5.5291),
    (51.5512, 5.5558),
    (51.5310, 5.5800),
]
START = (51.5748, 5.4721)
END = (51.5517, 5.5561)

UTC = timezone.utc


def meters_to_delta(lat: float, east_m: float, north_m: float) -> tuple[float, float]:
    lat_rad = math.radians(lat)
    dlat = north_m / 111_320.0
    dlon = east_m / (111_320.0 * math.cos(lat_rad))
    return dlat, dlon


def garden_offset(lat: float, lon: float, index: int) -> tuple[float, float]:
    angle = index * 1.17
    radius_m = 12.0 + 18.0 * abs(math.sin(index * 0.41))
    east = radius_m * math.cos(angle)
    north = radius_m * math.sin(angle * 0.93)
    dlat, dlon = meters_to_delta(lat, east, north)
    return lat + dlat, lon + dlon


def haversine_m(lat1: float, lon1: float, lat2: float, lon2: float) -> float:
    r = 6_371_000.0
    p = math.pi / 180.0
    a = (
        math.sin((lat2 - lat1) * p / 2) ** 2
        + math.cos(lat1 * p) * math.cos(lat2 * p) * math.sin((lon2 - lon1) * p / 2) ** 2
    )
    return 2 * r * math.asin(math.sqrt(a))


def lerp(a: float, b: float, t: float) -> float:
    return a + (b - a) * t


def should_save(last: dict | None, reading: dict) -> bool:
    if reading["accuracy"] > MAX_ACCURACY_M:
        return False
    if last is None:
        return True
    speed_kmh = reading["speed"] * 3.6
    interval = FAST_INTERVAL_S if speed_kmh > FAST_SPEED_KMH else DEFAULT_INTERVAL_S
    t0 = datetime.fromisoformat(last["recordedAtUtc"].replace("Z", "+00:00"))
    t1 = datetime.fromisoformat(reading["recordedAtUtc"].replace("Z", "+00:00"))
    return (t1 - t0).total_seconds() > interval


def apply_evaluator(raw: list[dict]) -> list[dict]:
    saved: list[dict] = []
    last: dict | None = None
    for reading in raw:
        if should_save(last, reading):
            saved.append(reading)
            last = reading
    return saved


def append_reading(
    raw: list[dict],
    when: datetime,
    lat: float,
    lon: float,
    speed_ms: float,
    accuracy: float,
) -> None:
    raw.append(
        {
            "latitude": round(lat, 6),
            "longitude": round(lon, 6),
            "speed": round(speed_ms, 2),
            "accuracy": round(accuracy, 1),
            "recordedAtUtc": when.strftime("%Y-%m-%dT%H:%M:%SZ"),
        }
    )


def generate_drive_raw(
    raw: list[dict],
    t: datetime,
    start: tuple[float, float],
    end: tuple[float, float],
    duration_s: int,
    sample_s: int = 10,
) -> datetime:
    steps = max(1, duration_s // sample_s)
    prev_lat, prev_lon = start
    for i in range(steps + 1):
        frac = i / steps
        lat = lerp(start[0], end[0], frac)
        lon = lerp(start[1], end[1], frac)
        speed_ms = 0.0 if i == 0 else max(2.5, min(haversine_m(prev_lat, prev_lon, lat, lon) / sample_s, 22.0))
        append_reading(raw, t, lat, lon, speed_ms, 7.0 + (i % 5) * 0.8)
        prev_lat, prev_lon = lat, lon
        t += timedelta(seconds=sample_s)
    return t


def generate_garden_raw(
    raw: list[dict],
    t: datetime,
    center: tuple[float, float],
    duration_s: int,
    sample_s: int = 30,
) -> datetime:
    steps = max(1, duration_s // sample_s)
    for i in range(steps + 1):
        lat, lon = garden_offset(center[0], center[1], i)
        speed_ms = 0.4 + 0.8 * abs(math.sin(i * 0.55))
        append_reading(raw, t, lat, lon, speed_ms, 4.5 + (i % 7) * 0.6)
        t += timedelta(seconds=sample_s)
    return t


def main() -> None:
    final: list[dict] = []
    t = datetime.utcnow()

    append_reading(final, t, START[0], START[1], 0.0, 8.0)
    t += timedelta(seconds=10)

    prev = START
    for stop in STOPS:
        drive_raw: list[dict] = []
        t = generate_drive_raw(drive_raw, t, prev, stop, duration_s=170, sample_s=10)
        final.extend(apply_evaluator(drive_raw))

        t += timedelta(seconds=120)
        garden_raw: list[dict] = []
        t = generate_garden_raw(garden_raw, t, stop, duration_s=28 * 60, sample_s=30)
        final.extend(garden_raw)

        t += timedelta(seconds=90)
        prev = stop

    drive_raw = []
    t = generate_drive_raw(drive_raw, t, STOPS[-1], END, duration_s=170, sample_s=10)
    final.extend(apply_evaluator(drive_raw))

    out = Path(__file__).with_name("points-evaluator-saved.json")
    out.write_text(json.dumps(final, indent=2) + "\n", encoding="utf-8")
    print(f"points={len(final)} -> {out}")


if __name__ == "__main__":
    main()
