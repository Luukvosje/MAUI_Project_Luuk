using System;
using System.Collections.Generic;
using System.Text;
using TimeOn.Domain.Constants;
using TimeOn.Domain.Entities;
using TimeOn.Domain.ValueObjects;

namespace TimeOn.Domain.Services
{
    public class GpsClassifier
    {
        private readonly TimeSpan _minStationaryWindow;
        private readonly double _maxStationaryRadiusMeters;

        public GpsClassifier()
        {
            _minStationaryWindow = TrackingConstants.MinimumStopDurationMinutes;
            _maxStationaryRadiusMeters = TrackingConstants.MaxStationaryDistanceMeters;
        }

        public IReadOnlyList<GpsSegment> Classify(IReadOnlyList<GpsPoint> points)
        {
            if (points == null || points.Count == 0)
                return Array.Empty<GpsSegment>();

            bool[] isStationary = ComputeStationaryFlags(points);
            return BuildSegments(points, isStationary);
        }

        private bool[] ComputeStationaryFlags(IReadOnlyList<GpsPoint> points)
        {
            bool[] flags = new bool[points.Count];

            for (int i = 0; i < points.Count; i++)
            {
                var origin = points[i].Location;
                var windowEnd = points[i].RecordedAtUtc + _minStationaryWindow;

                // Collect points within the time window
                int j = i + 1;
                double maxDist = 0;

                while (j < points.Count && points[j].RecordedAtUtc <= windowEnd)
                {
                    var coord = Coordinate.Create(points[j].Location.Latitude, points[j].Location.Longitude);
                    double dist = origin.DistanceTo(coord);
                    if (dist > maxDist) maxDist = dist;
                    j++;
                }

                // Need at least a few points in the window
                bool hasEnoughPoints = (j - i) >= 5;
                flags[i] = hasEnoughPoints && maxDist <= _maxStationaryRadiusMeters;
            }

            return flags;
        }

        private static IReadOnlyList<GpsSegment> BuildSegments(
            IReadOnlyList<GpsPoint> points,
            bool[] isStationary)
        {
            var segments = new List<GpsSegment>();
            int i = 0;

            while (i < points.Count)
            {
                bool currentState = isStationary[i];
                int start = i;

                while (i < points.Count && isStationary[i] == currentState)
                    i++;

                var segmentPoints = points.Skip(start).Take(i - start).ToList();
                var type = currentState ? SegmentType.Stationary : SegmentType.Driving;

                segments.Add(GpsSegment.Create(type, segmentPoints));
            }

            return segments;
        }
    }
}
