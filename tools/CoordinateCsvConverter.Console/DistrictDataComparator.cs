using GeoDistrictDetector;
using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace CoordinateCsvConverter
{
    /// <summary>
    /// DistrictData comparator - used to compare geographic data in two CSV files for discrepancies.
    /// Supports different coordinate systems and tolerance-based comparison.
    /// </summary>
    public class DistrictDataComparator
    {
        private readonly double _geoToleranceMeters;

        public DistrictDataComparator(double geoToleranceMeters = 5.0)
        {
            _geoToleranceMeters = geoToleranceMeters;
        }

        /// <summary>
        /// Comparison result information
        /// </summary>
        public class ComparisonResult
        {
            public int TotalCount { get; set; }
            public int PassedCount { get; set; }
            public int FailedCount { get; set; }
            public List<string> Failures { get; set; } = new List<string>();
            public double PassRate => TotalCount > 0 ? (double)PassedCount / TotalCount : 0.0;
        }

        /// <summary>
        /// Compare geographic data from two CSV files
        /// </summary>
        /// <param name="leftCsvPath">Path to the left CSV file</param>
        /// <param name="rightCsvPath">Path to the right CSV file</param>
        /// <param name="leftSystem">Coordinate system of left file</param>
        /// <param name="rightSystem">Coordinate system of right file</param>
        /// <returns>Comparison result</returns>
        public ComparisonResult Compare(string leftCsvPath, string rightCsvPath,
            CoordinateSystem leftSystem = CoordinateSystem.WGS84,
            CoordinateSystem rightSystem = CoordinateSystem.WGS84)
        {
            var result = new ComparisonResult();

            try
            {
                Console.WriteLine("Loading CSV files...");
                // Read both CSV files
                var leftData = DistrictData.LoadFromCsv(leftCsvPath).ToDictionary(d => d.Id);
                var rightData = DistrictData.LoadFromCsv(rightCsvPath).ToDictionary(d => d.Id);

                // Get all IDs
                var allIds = new HashSet<string>(leftData.Keys.Concat(rightData.Keys));
                result.TotalCount = allIds.Count;

                Console.WriteLine($"Comparing {result.TotalCount} records...");

                int processedCount = 0;
                int lastPercent = -1;

                foreach (var id in allIds.OrderBy(x => x))
                {
                    leftData.TryGetValue(id, out var leftItem);
                    rightData.TryGetValue(id, out var rightItem);

                    var comparisonErrors = CompareDistrictData(id, leftItem, rightItem, leftSystem, rightSystem);
                    if (comparisonErrors.Count == 0)
                    {
                        result.PassedCount++;
                    }
                    else
                    {
                        result.FailedCount++;
                        result.Failures.Add($"ID={id}: {string.Join("; ", comparisonErrors)}");
                    }

                    processedCount++;

                    // Update progress bar
                    int currentPercent = (int)((double)processedCount / result.TotalCount * 100);
                    if (currentPercent != lastPercent)
                    {
                        ConsoleProgressBar.Show(currentPercent, "Comparing");
                        lastPercent = currentPercent;
                    }
                }

                ConsoleProgressBar.Complete("Comparison");
            }
            catch (Exception ex)
            {
                result.Failures.Add($"Comparison error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Compare individual DistrictData objects
        /// </summary>
        /// <param name="id">Data ID</param>
        /// <param name="left">Left side data</param>
        /// <param name="right">Right side data</param>
        /// <param name="leftSystem">Left coordinate system</param>
        /// <param name="rightSystem">Right coordinate system</param>
        /// <returns>List of error messages</returns>
        private List<string> CompareDistrictData(string id, DistrictData? left, DistrictData? right,
            CoordinateSystem leftSystem, CoordinateSystem rightSystem)
        {
            var errors = new List<string>();

            // Check data existence
            if (left == null)
            {
                errors.Add("Left data missing");
                return errors;
            }
            if (right == null)
            {
                errors.Add("Right data missing");
                return errors;
            }

            // Compare geo coordinates
            var geoError = CompareGeoCoordinates(left.Geo, right.Geo, leftSystem, rightSystem);
            if (!string.IsNullOrEmpty(geoError))
            {
                errors.Add($"Geo coordinate error: {geoError}");
            }

            // Compare polygon coordinates
            var polygonError = ComparePolygonCoordinatesWithEmpty(left.Polygon, right.Polygon, leftSystem, rightSystem);
            if (!string.IsNullOrEmpty(polygonError))
            {
                errors.Add($"Polygon error: {polygonError}");
            }

            return errors;
        }

        /// <summary>
        /// Compare two geo coordinate points
        /// </summary>
        /// <param name="leftGeo">Left coordinate</param>
        /// <param name="rightGeo">Right coordinate</param>
        /// <param name="leftSystem">Left coordinate system</param>
        /// <param name="rightSystem">Right coordinate system</param>
        /// <returns>Error message, empty string if no error</returns>
        private string CompareGeoCoordinates(Coordinate? leftGeo, Coordinate? rightGeo, CoordinateSystem leftSystem, CoordinateSystem rightSystem)
        {
            if (leftGeo == null && rightGeo == null)
                return string.Empty;

            if (leftGeo == null)
                return "Left geo coordinate is EMPTY";

            if (rightGeo == null)
                return "Right geo coordinate is EMPTY";

            // Convert right coordinate to left coordinate system for comparison
            double rightLng = rightGeo.X, rightLat = rightGeo.Y;
            if (rightSystem != leftSystem)
            {
                var converted = CoordinateConverter.Convert(rightLng, rightLat, rightSystem, leftSystem);
                rightLng = converted.lng;
                rightLat = converted.lat;
            }

            // Calculate distance
            double distance = CalculateDistanceMeters(leftGeo.X, leftGeo.Y, rightLng, rightLat);

            if (double.IsNaN(distance))
                return "Failed to calculate coordinate distance";

            if (distance > _geoToleranceMeters)
                return $"Distance exceeds tolerance {distance:F3}m > {_geoToleranceMeters}m";

            return string.Empty;
        }

        /// <summary>
        /// Compare two polygon coordinate lists
        /// </summary>
        /// <param name="leftPolygon">Left polygon coordinates</param>
        /// <param name="rightPolygon">Right polygon coordinates</param>
        /// <param name="leftSystem">Left coordinate system</param>
        /// <param name="rightSystem">Right coordinate system</param>
        /// <returns>Error message, empty string if no error</returns>
        private string ComparePolygonCoordinates(List<Coordinate>? leftPolygon, List<Coordinate>? rightPolygon,
            CoordinateSystem leftSystem, CoordinateSystem rightSystem)
        {
            if ((leftPolygon == null || leftPolygon.Count == 0) &&
                (rightPolygon == null || rightPolygon.Count == 0))
                return string.Empty;

            if (leftPolygon == null || leftPolygon.Count == 0)
                return "Left polygon is null or empty";

            if (rightPolygon == null || rightPolygon.Count == 0)
                return "Right polygon is null or empty";

            // Check vertex count must be exactly the same
            if (leftPolygon.Count != rightPolygon.Count)
            {
                return $"Vertex count mismatch: {leftPolygon.Count} vs {rightPolygon.Count}";
            }

            // Convert right polygon to left coordinate system
            var convertedRightPolygon = ConvertPolygonCoordinates(rightPolygon, rightSystem, leftSystem);

            // Compare each vertex in order
            var maxDistance = ComparePolygonVertices(leftPolygon, convertedRightPolygon);

            if (double.IsNaN(maxDistance))
                return "Failed to calculate polygon vertex distance";

            if (maxDistance > _geoToleranceMeters)
                return $"Max vertex distance exceeds tolerance {maxDistance:F3}m > {_geoToleranceMeters}m";

            return string.Empty;
        }

        /// <summary>
        /// Convert polygon coordinate system
        /// </summary>
        /// <param name="polygon">Original polygon coordinates</param>
        /// <param name="fromSystem">Source coordinate system</param>
        /// <param name="toSystem">Target coordinate system</param>
        /// <returns>Converted coordinate list</returns>
        private List<Coordinate> ConvertPolygonCoordinates(List<Coordinate> polygon,
            CoordinateSystem fromSystem, CoordinateSystem toSystem)
        {
            if (fromSystem == toSystem || polygon == null)
                return polygon ?? new List<Coordinate>();

            var converted = new List<Coordinate>();
            foreach (var coord in polygon)
            {
                try
                {
                    var convertedCoord = CoordinateConverter.Convert(coord.X, coord.Y, fromSystem, toSystem);
                    converted.Add(new Coordinate(convertedCoord.lng, convertedCoord.lat));
                }
                catch
                {
                    // If conversion fails, keep original coordinate
                    converted.Add(new Coordinate(coord.X, coord.Y));
                }
            }
            return converted;
        }

        /// <summary>
        /// Compare polygon vertices and return maximum distance
        /// Compare each point in order (requires exact same number of vertices)
        /// </summary>
        /// <param name="leftPolygon">Left polygon</param>
        /// <param name="rightPolygon">Right polygon</param>
        /// <returns>Maximum distance between vertices (meters)</returns>
        private double ComparePolygonVertices(List<Coordinate> leftPolygon, List<Coordinate> rightPolygon)
        {
            if (leftPolygon == null || rightPolygon == null ||
                leftPolygon.Count == 0 || rightPolygon.Count == 0)
                return double.NaN;

            // At this point, both polygons have the same number of vertices
            double maxDistance = 0.0;

            // Compare each vertex in order
            for (int i = 0; i < leftPolygon.Count; i++)
            {
                var leftCoord = leftPolygon[i];
                var rightCoord = rightPolygon[i];

                double distance = CalculateDistanceMeters(leftCoord.X, leftCoord.Y, rightCoord.X, rightCoord.Y);
                if (!double.IsNaN(distance) && distance > maxDistance)
                {
                    maxDistance = distance;
                }
            }

            return maxDistance;
        }

        /// <summary>
        /// Calculate distance between two points using Haversine formula (meters)
        /// </summary>
        /// <param name="lng1">Point 1 longitude</param>
        /// <param name="lat1">Point 1 latitude</param>
        /// <param name="lng2">Point 2 longitude</param>
        /// <param name="lat2">Point 2 latitude</param>
        /// <returns>Distance in meters</returns>
        private double CalculateDistanceMeters(double lng1, double lat1, double lng2, double lat2)
        {
            try
            {
                const double R = 6371000; // Earth radius (meters)
                var φ1 = lat1 * Math.PI / 180.0;
                var φ2 = lat2 * Math.PI / 180.0;
                var Δφ = (lat2 - lat1) * Math.PI / 180.0;
                var Δλ = (lng2 - lng1) * Math.PI / 180.0;

                var a = Math.Sin(Δφ / 2) * Math.Sin(Δφ / 2) +
                        Math.Cos(φ1) * Math.Cos(φ2) *
                        Math.Sin(Δλ / 2) * Math.Sin(Δλ / 2);
                var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

                return R * c;
            }
            catch
            {
                return double.NaN;
            }
        }

        /// <summary>
        /// Print comparison report
        /// </summary>
        /// <param name="result">Comparison result</param>
        /// <param name="leftPath">Left file path</param>
        /// <param name="rightPath">Right file path</param>
        public void PrintComparisonReport(ComparisonResult result, string leftPath, string rightPath)
        {
            Console.WriteLine("=== DistrictData Comparison Report ===");
            Console.WriteLine($"Left file: {leftPath}");
            Console.WriteLine($"Right file: {rightPath}");
            Console.WriteLine($"Geo tolerance: {_geoToleranceMeters} meters");
            Console.WriteLine();
            Console.WriteLine($"Total compared: {result.TotalCount}");
            Console.WriteLine($"Passed: {result.PassedCount}");
            Console.WriteLine($"Failed: {result.FailedCount}");
            Console.WriteLine($"Pass rate: {result.PassRate:P2}");
            Console.WriteLine();

            if (result.Failures.Count > 0)
            {
                Console.WriteLine("Failure details:");
                foreach (var failure in result.Failures)
                {
                    Console.WriteLine($"  {failure}");
                }
            }
            else
            {
                Console.WriteLine("All data comparison passed!");
            }
        }

        /// <summary>
        /// Compare polygon coordinates (improved version with better EMPTY handling)
        /// </summary>
        /// <param name="leftPolygon">Left polygon coordinates</param>
        /// <param name="rightPolygon">Right polygon coordinates</param>
        /// <param name="leftSystem">Left coordinate system</param>
        /// <param name="rightSystem">Right coordinate system</param>
        /// <returns>Error message, empty string if no error</returns>
        private string ComparePolygonCoordinatesWithEmpty(List<Coordinate>? leftPolygon, List<Coordinate>? rightPolygon,
            CoordinateSystem leftSystem, CoordinateSystem rightSystem)
        {
            // Improved EMPTY handling: both null/empty polygons are considered equal
            bool leftIsEmpty = leftPolygon == null || leftPolygon.Count == 0;
            bool rightIsEmpty = rightPolygon == null || rightPolygon.Count == 0;

            if (leftIsEmpty && rightIsEmpty)
                return string.Empty; // Both empty, considered equal

            if (leftIsEmpty)
                return "Left polygon is empty but right polygon has data";

            if (rightIsEmpty)
                return "Right polygon has data but left polygon is empty";

            // Both have data, proceed with normal comparison
            return ComparePolygonCoordinates(leftPolygon, rightPolygon, leftSystem, rightSystem);
        }

    }
}