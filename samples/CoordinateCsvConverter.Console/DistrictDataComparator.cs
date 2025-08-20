using GeoDistrictDetector;
using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace CoordinateCsvConverter
{
    //            return string.Empty;

    /// <summary>
    /// 比较多边形坐标
    /// </summary>
    /// <param name="leftPolygon">左侧多边形坐标</param>
    /// <param name="rightPolygon">右侧多边形坐标</param>
    /// <param name="leftSystem">左侧坐标系</param>
    /// <param name="rightSystem">右侧坐标系</param>
    /// <returns>错误信息，无误差返回空字符串</returns>
    /// DistrictData comparator - used to compare geographic data in two CSV files for discrepancies.
    /// </summary>
    public class DistrictDataComparator
    {
        private readonly double _geoToleranceMeters;

        public DistrictDataComparator(double geoToleranceMeters = 5.0)
        {
            _geoToleranceMeters = geoToleranceMeters;
        }

        /// <summary>
        /// 对比结果信息
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
        /// 对比两个CSV文件中的DistrictData
        /// <summary>
        /// Comparison result information
        /// </summary>
        /// <param name="leftSystem">左侧坐标系</param>
        /// <param name="rightSystem">右侧坐标系</param>
        /// <returns>对比结果</returns>
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
                        ShowProgressBar(currentPercent, "Comparing");
                        lastPercent = currentPercent;
                    }
                }

                Console.WriteLine(); // New line after progress bar
            }
            catch (Exception ex)
            {
                result.Failures.Add($"Comparison error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// 对比单个DistrictData对象
        /// </summary>
        /// <param name="id">数据ID</param>
        /// <param name="left">左侧数据</param>
        /// <param name="right">右侧数据</param>
        /// <param name="leftSystem">左侧坐标系</param>
        /// <param name="rightSystem">右侧坐标系</param>
        /// <returns>错误信息列表</returns>
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
        /// 对比两个geo坐标点
        /// </summary>
        /// <param name="leftGeo">左侧坐标</param>
        /// <param name="rightGeo">右侧坐标</param>
        /// <param name="leftSystem">左侧坐标系</param>
        /// <param name="rightSystem">右侧坐标系</param>
        /// <returns>错误信息，无误差返回空字符串</returns>
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
        /// 对比两个polygon坐标列表
        /// </summary>
        /// <param name="leftPolygon">左侧多边形坐标</param>
        /// <param name="rightPolygon">右侧多边形坐标</param>
        /// <param name="leftSystem">左侧坐标系</param>
        /// <param name="rightSystem">右侧坐标系</param>
        /// <returns>错误信息，无误差返回空字符串</returns>
        private string ComparePolygonCoordinates(List<Coordinate> leftPolygon, List<Coordinate> rightPolygon,
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
        /// 转换多边形坐标系
        /// </summary>
        /// <param name="polygon">原始多边形坐标</param>
        /// <param name="fromSystem">源坐标系</param>
        /// <param name="toSystem">目标坐标系</param>
        /// <returns>转换后的坐标列表</returns>
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
        /// 对比多边形顶点，返回最大距离
        /// 按顺序逐一对比每个点位（要求顶点数量完全一致）
        /// </summary>
        /// <param name="leftPolygon">左侧多边形</param>
        /// <param name="rightPolygon">右侧多边形</param>
        /// <returns>最大顶点间距离（米）</returns>
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
        /// 使用Haversine公式计算两点间距离（米）
        /// </summary>
        /// <param name="lng1">点1经度</param>
        /// <param name="lat1">点1纬度</param>
        /// <param name="lng2">点2经度</param>
        /// <param name="lat2">点2纬度</param>
        /// <returns>距离（米）</returns>
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
        /// 打印对比报告
        /// </summary>
        /// <param name="result">对比结果</param>
        /// <param name="leftPath">左侧文件路径</param>
        /// <param name="rightPath">右侧文件路径</param>
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

        private static void ShowProgressBar(int percent, string operation)
        {
            const int barWidth = 40;
            int filledWidth = (int)((double)percent / 100 * barWidth);

            Console.Write($"\r{operation}: [");
            Console.Write(new string('█', filledWidth));
            Console.Write(new string('░', barWidth - filledWidth));
            Console.Write($"] {percent}%");
        }

        /// <summary>
        /// 比较多边形坐标（改进版，更好地处理EMPTY情况）
        /// </summary>
        /// <param name="leftPolygon">左侧多边形坐标</param>
        /// <param name="rightPolygon">右侧多边形坐标</param>
        /// <param name="leftSystem">左侧坐标系</param>
        /// <param name="rightSystem">右侧坐标系</param>
        /// <returns>错误信息，无误差返回空字符串</returns>
        private string ComparePolygonCoordinatesWithEmpty(List<Coordinate> leftPolygon, List<Coordinate> rightPolygon,
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