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

        public List<District> GetAllDistricts()
        {
            return _districts.ToList();
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
