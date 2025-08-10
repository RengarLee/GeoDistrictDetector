using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using GeoDistrictDetector.Models;
using System.Text.Json;

namespace GeoDistrictDetector.Services
{
    /// <summary>
    /// 地理位置服务实现
    /// </summary>
    public class GeoDistrictService : IGeoDistrictService
    {
        private List<District> _districts = new List<District>();
        private readonly double _earthRadius = 6371; // 地球半径（公里）

public async Task<District?> FindDistrictByPointAsync(GeoPoint point)
        {
            if (_districts.Count == 0)
            {
                throw new InvalidOperationException("区域数据未加载，请先调用 LoadDistrictDataAsync 方法");
            }

            // 方法1：使用最近距离算法（简单实现）
            District? nearestDistrict = null;
            double minDistance = double.MaxValue;

            foreach (var district in _districts)
            {
                if (string.IsNullOrWhiteSpace(district.Geo) || district.Geo == "EMPTY") continue;
                var geoParts = district.Geo.Split(' ');
                if (geoParts.Length != 2) continue;
                if (!double.TryParse(geoParts[1], out double lat)) continue;
                if (!double.TryParse(geoParts[0], out double lng)) continue;
                var centerPoint = new GeoPoint(lat, lng);
                double distance = CalculateDistance(point, centerPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestDistrict = district;
                }
            }

            // 方法2：如果有边界数据，可以使用点在多边形内判断算法
            var districtWithBoundary = _districts.FirstOrDefault(d =>
                d.Polygon != null && !d.Polygon.IsEmpty &&
                d.Polygon.Contains(new NetTopologySuite.Geometries.Point(point.Longitude, point.Latitude)));

            return districtWithBoundary != null ? districtWithBoundary : nearestDistrict;
        }

        public async Task LoadDistrictDataAsync(string dataFilePath)
        {
            try
            {
                if (!File.Exists(dataFilePath))
                {
                    throw new FileNotFoundException($"数据文件不存在: {dataFilePath}");
                }

                string fileExtension = Path.GetExtension(dataFilePath).ToLower();
                
                switch (fileExtension)
                {
                    case ".json":
                        await LoadFromJsonAsync(dataFilePath);
                        break;
                    case ".csv":
                        await LoadFromCsvAsync(dataFilePath);
                        break;
                    default:
                        throw new NotSupportedException($"不支持的文件格式: {fileExtension}");
                }

                Console.WriteLine($"成功加载 {_districts.Count} 个区域数据");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载区域数据失败: {ex.Message}");
                throw;
            }
        }

        public List<District> GetAllDistricts()
        {
            return _districts.ToList();
        }

        private async Task LoadFromJsonAsync(string filePath)
        {
            string jsonContent = await File.ReadAllTextAsync(filePath);
            var districts = JsonSerializer.Deserialize<List<District>>(jsonContent);
            if (districts != null)
            {
                _districts = districts;
            }
        }

        private async Task LoadFromCsvAsync(string filePath)
        {
            string[] lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length == 0)
                return;

            // 跳过标题行
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    // 假设CSV格式为: Id,Pid,Deep,Name,ExtPath,Geo,Polygon
                    string[] parts = line.Split(',');
                    if (parts.Length >= 7)
                    {
                        int id = int.TryParse(parts[0].Trim(), out int tid) ? tid : 0;
                        int pid = int.TryParse(parts[1].Trim(), out int tpid) ? tpid : 0;
                        DistrictLevel deep = int.TryParse(parts[2].Trim(), out int tdeep) ? (DistrictLevel)tdeep : DistrictLevel.Province;
                        string name = parts[3].Trim();
                        string extPath = parts[4].Trim();
                        string geo = parts[5].Trim();
                        string polygonStr = parts[6].Trim();
                        var polygon = GeoDistrictDetector.Models.DistrictFactory.LoadFromCsv("dummy").Count == 0 ? null : GeoDistrictDetector.Models.DistrictFactory.LoadFromCsv("dummy")[0].Polygon; // 占位，实际应调用解析方法
                        // 实际应调用 DistrictFactory 的私有解析方法，这里直接用 NetTopologySuite 解析
                        polygon = ParsePolygonGeometry(polygonStr);
                        var district = new District(id, pid, deep, name, extPath, geo, polygon);
                        _districts.Add(district);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"解析第 {i + 1} 行数据失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 计算两点间的距离（使用Haversine公式）
        /// </summary>
        private double CalculateDistance(GeoPoint point1, GeoPoint point2)
        {
            double ToRadians(double degrees) => degrees * Math.PI / 180.0;
            double lat1Rad = ToRadians(point1.Latitude);
            double lat2Rad = ToRadians(point2.Latitude);
            double deltaLatRad = ToRadians(point2.Latitude - point1.Latitude);
            double deltaLonRad = ToRadians(point2.Longitude - point1.Longitude);

            double a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return _earthRadius * c;
        }

        /// <summary>
        /// 判断点是否在多边形内（射线法）
        /// </summary>
        private bool IsPointInPolygon(GeoPoint point, List<GeoPoint> polygon)
        {
            if (polygon.Count < 3)
                return false;

            bool inside = false;
            int j = polygon.Count - 1;

            for (int i = 0; i < polygon.Count; i++)
            {
                if (((polygon[i].Latitude > point.Latitude) != (polygon[j].Latitude > point.Latitude)) &&
                    (point.Longitude < (polygon[j].Longitude - polygon[i].Longitude) *
                     (point.Latitude - polygon[i].Latitude) / (polygon[j].Latitude - polygon[i].Latitude) + polygon[i].Longitude))
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }

        // 解析 polygon 字符串为 Geometry 对象
        private NetTopologySuite.Geometries.Geometry ParsePolygonGeometry(string polygonStr)
        {
            if (string.IsNullOrEmpty(polygonStr) || polygonStr == "EMPTY")
                return NetTopologySuite.Geometries.GeometryFactory.Default.CreateGeometryCollection(null);
            try
            {
                var factory = NetTopologySuite.Geometries.GeometryFactory.Default;
                var polygons = new List<NetTopologySuite.Geometries.Polygon>();
                var blocks = polygonStr.Split(';');
                foreach (var block in blocks)
                {
                    var coords = block.Split(',');
                    var points = new List<NetTopologySuite.Geometries.Coordinate>();
                    foreach (var coord in coords)
                    {
                        var lngLat = coord.Trim().Split(' ');
                        if (lngLat.Length == 2 &&
                            double.TryParse(lngLat[0], out double lng) &&
                            double.TryParse(lngLat[1], out double lat))
                        {
                            points.Add(new NetTopologySuite.Geometries.Coordinate(lng, lat));
                        }
                    }
                    if (points.Count > 2)
                    {
                        if (!points[0].Equals2D(points[points.Count - 1]))
                            points.Add(points[0]);
                        var linearRing = factory.CreateLinearRing(points.ToArray());
                        polygons.Add(factory.CreatePolygon(linearRing));
                    }
                }
                if (polygons.Count == 1)
                    return polygons[0];
                else if (polygons.Count > 1)
                    return factory.CreateMultiPolygon(polygons.ToArray());
            }
            catch { }
            return NetTopologySuite.Geometries.GeometryFactory.Default.CreateGeometryCollection(null);
        }

        // 移除未使用的 ToRadians
    }
}
