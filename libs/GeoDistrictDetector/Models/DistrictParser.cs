using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// Utility class for parsing District-related data fields.
    /// </summary>
    public static class DistrictParser
    {
        /// <summary>
        /// Parses a 'lng lat' string to a Coordinate.
        /// </summary>
        public static Coordinate ParseCoordinate(string geoStr)
        {
            var arr = geoStr.Split(' ');
            if (arr.Length == 2 &&
                double.TryParse(arr[0], out double lng) &&
                double.TryParse(arr[1], out double lat))
            {
                return new Coordinate(lng, lat);
            }
            return new Coordinate();
        }

        /// <summary>
        /// Parses a multi-segment coordinate string to List<List<Coordinate>>.
        /// </summary>
        public static List<List<Coordinate>> ParseCoordinates(string polygonStr)
        {
            var result = new List<List<Coordinate>>();
            if (string.IsNullOrEmpty(polygonStr) || polygonStr == "EMPTY")
                return result;
            var blocks = polygonStr.Split(';');
            foreach (var block in blocks)
            {
                var coords = block.Split(',');
                var points = new List<Coordinate>();
                foreach (var coord in coords)
                {
                    var lngLat = coord.Trim().Split(' ');
                    if (lngLat.Length == 2 &&
                        double.TryParse(lngLat[0], out double lng) &&
                        double.TryParse(lngLat[1], out double lat))
                    {
                        points.Add(new Coordinate(lng, lat));
                    }
                }
                if (points.Count > 0)
                    result.Add(points);
            }
            return result;
        }

        /// <summary>
        /// Parses multi-segment coordinates to a Geometry object.
        /// </summary>
        public static Geometry ParsePolygon(List<List<Coordinate>> multiCoords)
        {
            var factory = GeometryFactory.Default;
            var polygons = new List<Polygon>();
            foreach (var points in multiCoords)
            {
                if (points.Count > 2)
                {
                    // Close polygon if not closed
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
            return factory.CreateGeometryCollection(null);
        }
    }
}
