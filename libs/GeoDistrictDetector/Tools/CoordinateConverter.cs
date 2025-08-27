using System;

namespace GeoDistrictDetector
{

    /// <summary>
    /// Coordinate system type enumeration
    /// </summary>
    public enum CoordinateSystem
    {
        /// <summary>
        /// WGS84 coordinate system (GPS original coordinates)
        /// </summary>
        WGS84,
        /// <summary>
        /// GCJ02 coordinate system (Mars coordinate system, used by Gaode Maps)
        /// </summary>
        GCJ02,
        /// <summary>
        /// BD09 coordinate system (Baidu coordinate system, used by Baidu Maps)
        /// </summary>
        BD09
    }

    /// <summary>
    /// Coordinate converter (low-level algorithm implementation)
    /// </summary>
    public static class CoordinateConverter
    {

        private const double A = 6378245.0; // Semi-major axis
        private const double EE = 0.00669342162296594323; // Eccentricity squared

        /// <summary>
        /// Convert WGS84 to GCJ02 (Mars coordinate system/Gaode coordinate system)
        /// </summary>
        /// <param name="lng">WGS84 longitude</param>
        /// <param name="lat">WGS84 latitude</param>
        /// <returns>GCJ02 coordinates</returns>
        private static (double lng, double lat) Wgs84ToGcj02(double lng, double lat)
        {
            var (dlng, dlat) = TransformDelta(lng - 105.0, lat - 35.0);
            var radlat = lat / 180.0 * Math.PI;
            var magic = Math.Sin(radlat);
            magic = 1 - EE * magic * magic;
            var sqrtmagic = Math.Sqrt(magic);
            dlng = (dlng * 180.0) / (A / sqrtmagic * Math.Cos(radlat) * Math.PI);
            dlat = (dlat * 180.0) / ((A * (1 - EE)) / (magic * sqrtmagic) * Math.PI);

            return (lng + dlng, lat + dlat);
        }

        /// <summary>
        /// Convert GCJ02 (Mars coordinate system/Gaode coordinate system) to WGS84
        /// </summary>
        /// <param name="lng">GCJ02 longitude</param>
        /// <param name="lat">GCJ02 latitude</param>
        /// <returns>WGS84 coordinates</returns>
        private static (double lng, double lat) Gcj02ToWgs84(double lng, double lat)
        {
            var (dlng, dlat) = TransformDelta(lng - 105.0, lat - 35.0);
            var radlat = lat / 180.0 * Math.PI;
            var magic = Math.Sin(radlat);
            magic = 1 - EE * magic * magic;
            var sqrtmagic = Math.Sqrt(magic);
            dlng = (dlng * 180.0) / (A / sqrtmagic * Math.Cos(radlat) * Math.PI);
            dlat = (dlat * 180.0) / ((A * (1 - EE)) / (magic * sqrtmagic) * Math.PI);

            return (lng - dlng, lat - dlat);
        }

        /// <summary>
        /// Convert GCJ02 (Mars coordinate system/Gaode coordinate system) to BD09 (Baidu coordinate system)
        /// </summary>
        /// <param name="lng">GCJ02 longitude</param>
        /// <param name="lat">GCJ02 latitude</param>
        /// <returns>BD09 coordinates</returns>
        private static (double lng, double lat) Gcj02ToBd09(double lng, double lat)
        {
            var z = Math.Sqrt(lng * lng + lat * lat) + 0.00002 * Math.Sin(lat * Math.PI * 3000.0 / 180.0);
            var theta = Math.Atan2(lat, lng) + 0.000003 * Math.Cos(lng * Math.PI * 3000.0 / 180.0);
            var bdLng = z * Math.Cos(theta) + 0.0065;
            var bdLat = z * Math.Sin(theta) + 0.006;
            return (bdLng, bdLat);
        }

        /// <summary>
        /// Convert BD09 (Baidu coordinate system) to GCJ02 (Mars coordinate system/Gaode coordinate system)
        /// </summary>
        /// <param name="lng">BD09 longitude</param>
        /// <param name="lat">BD09 latitude</param>
        /// <returns>GCJ02 coordinates</returns>
        private static (double lng, double lat) Bd09ToGcj02(double lng, double lat)
        {
            var x = lng - 0.0065;
            var y = lat - 0.006;
            var z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * Math.PI * 3000.0 / 180.0);
            var theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * Math.PI * 3000.0 / 180.0);
            var gcjLng = z * Math.Cos(theta);
            var gcjLat = z * Math.Sin(theta);
            return (gcjLng, gcjLat);
        }

        /// <summary>
        /// Convert WGS84 to BD09 (Baidu coordinate system)
        /// </summary>
        /// <param name="lng">WGS84 longitude</param>
        /// <param name="lat">WGS84 latitude</param>
        /// <returns>BD09 coordinates</returns>
        private static (double lng, double lat) Wgs84ToBd09(double lng, double lat)
        {
            var (gcjLng, gcjLat) = Wgs84ToGcj02(lng, lat);
            return Gcj02ToBd09(gcjLng, gcjLat);
        }

        /// <summary>
        /// Convert BD09 (Baidu coordinate system) to WGS84
        /// </summary>
        /// <param name="lng">BD09 longitude</param>
        /// <param name="lat">BD09 latitude</param>
        /// <returns>WGS84 coordinates</returns>
        private static (double lng, double lat) Bd09ToWgs84(double lng, double lat)
        {
            var (gcjLng, gcjLat) = Bd09ToGcj02(lng, lat);
            return Gcj02ToWgs84(gcjLng, gcjLat);
        }

        /// <summary>
        /// Core algorithm for coordinate transformation
        /// </summary>
        private static (double lng, double lat) TransformDelta(double lng, double lat)
        {
            var dlat = -100.0 + 2.0 * lng + 3.0 * lat + 0.2 * lat * lat + 0.1 * lng * lat + 0.2 * Math.Sqrt(Math.Abs(lng));
            dlat += (20.0 * Math.Sin(6.0 * lng * Math.PI) + 20.0 * Math.Sin(2.0 * lng * Math.PI)) * 2.0 / 3.0;
            dlat += (20.0 * Math.Sin(lat * Math.PI) + 40.0 * Math.Sin(lat / 3.0 * Math.PI)) * 2.0 / 3.0;
            dlat += (160.0 * Math.Sin(lat / 12.0 * Math.PI) + 320 * Math.Sin(lat * Math.PI / 30.0)) * 2.0 / 3.0;

            var dlng = 300.0 + lng + 2.0 * lat + 0.1 * lng * lng + 0.1 * lng * lat + 0.1 * Math.Sqrt(Math.Abs(lng));
            dlng += (20.0 * Math.Sin(6.0 * lng * Math.PI) + 20.0 * Math.Sin(2.0 * lng * Math.PI)) * 2.0 / 3.0;
            dlng += (20.0 * Math.Sin(lng * Math.PI) + 40.0 * Math.Sin(lng / 3.0 * Math.PI)) * 2.0 / 3.0;
            dlng += (150.0 * Math.Sin(lng / 12.0 * Math.PI) + 300.0 * Math.Sin(lng / 30.0 * Math.PI)) * 2.0 / 3.0;

            return (dlng, dlat);
        }

        /// <summary>
        /// Unified coordinate system conversion method: convert coordinates from sourceSystem to targetSystem.
        /// All high-level conversion logic should be centralized in this method.
        /// </summary>
        public static (double lng, double lat) Convert(double lng, double lat, CoordinateSystem sourceSystem, CoordinateSystem targetSystem)
        {
            if (sourceSystem == targetSystem)
                return (lng, lat);

            return (sourceSystem, targetSystem) switch
            {
                (CoordinateSystem.WGS84, CoordinateSystem.GCJ02) => Wgs84ToGcj02(lng, lat),
                (CoordinateSystem.WGS84, CoordinateSystem.BD09) => Wgs84ToBd09(lng, lat),
                (CoordinateSystem.WGS84, CoordinateSystem.WGS84) => (lng, lat),
                (CoordinateSystem.GCJ02, CoordinateSystem.WGS84) => Gcj02ToWgs84(lng, lat),
                (CoordinateSystem.GCJ02, CoordinateSystem.BD09) => Gcj02ToBd09(lng, lat),
                (CoordinateSystem.GCJ02, CoordinateSystem.GCJ02) => (lng, lat),
                (CoordinateSystem.BD09, CoordinateSystem.WGS84) => Bd09ToWgs84(lng, lat),
                (CoordinateSystem.BD09, CoordinateSystem.GCJ02) => Bd09ToGcj02(lng, lat),
                (CoordinateSystem.BD09, CoordinateSystem.BD09) => (lng, lat),
                _ => throw new ArgumentException($"Conversion from {sourceSystem} to {targetSystem} is not supported")
            };
        }

    }
}
