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

        public IReadOnlyList<GpsSegment> Classify(
            IReadOnlyList<GpsPoint> points,
            IReadOnlyList<Customer>? customers = null)
        {
            if (points == null || points.Count == 0)
                return Array.Empty<GpsSegment>();

            bool[] isStationary = ComputeStationaryFlags(points);
            return BuildSegments(points, isStationary, customers);
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
            bool[] isStationary,
            IReadOnlyList<Customer>? customers)
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

                Guid? customerId = null;
                double? distanceFromCustomerMeters = null;

                if (type == SegmentType.Stationary && customers is { Count: > 0 })
                {
                    var centerLatitude = segmentPoints.Average(point => point.Location.Latitude);
                    var centerLongitude = segmentPoints.Average(point => point.Location.Longitude);
                    var center = Coordinate.Create(centerLatitude, centerLongitude);
                    var match = CustomerProximityMatcher.FindNearest(center, customers);

                    if (match is not null)
                    {
                        customerId = match.CustomerId;
                        distanceFromCustomerMeters = match.DistanceMeters;
                    }
                }

                segments.Add(GpsSegment.Create(type, segmentPoints, customerId, distanceFromCustomerMeters));
            }

            return segments;
        }
    }
}
