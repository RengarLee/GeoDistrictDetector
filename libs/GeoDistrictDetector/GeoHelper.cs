using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Linq;

namespace GeoDistrictDetector
{
    public static class GeoHelper
    {
        /// <summary>
        /// 判断一个点是否在多边形内（使用 NetTopologySuite 实现）
        /// </summary>
        /// <param name="point">点</param>
        /// <param name="polygonCoords">多边形坐标（经纬度对）</param>
        /// <returns></returns>
        public static bool IsPointInPolygon((double lat, double lng) point, List<(double lat, double lng)> polygonCoords)
        {
            var coordinates = polygonCoords.Select(c => new Coordinate(c.lng, c.lat)).ToList();
            // 闭合多边形
            if (!coordinates.First().Equals2D(coordinates.Last()))
                coordinates.Add(coordinates.First());
            var polygon = new Polygon(new LinearRing(coordinates.ToArray()));
            var pt = new Point(point.lng, point.lat);
            return polygon.Contains(pt);
        }
    }
}
