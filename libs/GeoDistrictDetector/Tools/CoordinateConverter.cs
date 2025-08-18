using System;

namespace GeoDistrictDetector
{

    /// <summary>
    /// 坐标系类型枚举
    /// </summary>
    public enum CoordinateSystem
    {
        /// <summary>
        /// WGS84坐标系（GPS原始坐标）
        /// </summary>
        WGS84,
        /// <summary>
        /// GCJ02坐标系（火星坐标系，高德地图使用）
        /// </summary>
        GCJ02,
        /// <summary>
        /// BD09坐标系（百度坐标系，百度地图使用）
        /// </summary>
        BD09
    }

    /// <summary>
    /// 坐标转换器（低级算法实现）
    /// </summary>
    public static class CoordinateConverter
    {

        private const double A = 6378245.0; // 长半轴
        private const double EE = 0.00669342162296594323; // 扁心率平方

        /// <summary>
        /// 判断坐标是否在中国大陆范围内
        /// </summary>
        private static bool IsInChina(double lng, double lat)
        {
            return lng > 73.66 && lng < 135.05 && lat > 3.86 && lat < 53.55;
        }

        /// <summary>
        /// WGS84转GCJ02(火星坐标系/高德坐标系)
        /// </summary>
        /// <param name="lng">WGS84经度</param>
        /// <param name="lat">WGS84纬度</param>
        /// <returns>GCJ02坐标</returns>
        public static (double lng, double lat) Wgs84ToGcj02(double lng, double lat)
        {
            if (!IsInChina(lng, lat))
            {
                return (lng, lat);
            }

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
        /// GCJ02(火星坐标系/高德坐标系)转WGS84
        /// </summary>
        /// <param name="lng">GCJ02经度</param>
        /// <param name="lat">GCJ02纬度</param>
        /// <returns>WGS84坐标</returns>
        public static (double lng, double lat) Gcj02ToWgs84(double lng, double lat)
        {
            if (!IsInChina(lng, lat))
            {
                return (lng, lat);
            }

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
        /// GCJ02(火星坐标系/高德坐标系)转BD09(百度坐标系)
        /// </summary>
        /// <param name="lng">GCJ02经度</param>
        /// <param name="lat">GCJ02纬度</param>
        /// <returns>BD09坐标</returns>
        public static (double lng, double lat) Gcj02ToBd09(double lng, double lat)
        {
            var z = Math.Sqrt(lng * lng + lat * lat) + 0.00002 * Math.Sin(lat * Math.PI * 3000.0 / 180.0);
            var theta = Math.Atan2(lat, lng) + 0.000003 * Math.Cos(lng * Math.PI * 3000.0 / 180.0);
            var bdLng = z * Math.Cos(theta) + 0.0065;
            var bdLat = z * Math.Sin(theta) + 0.006;
            return (bdLng, bdLat);
        }

        /// <summary>
        /// BD09(百度坐标系)转GCJ02(火星坐标系/高德坐标系)
        /// </summary>
        /// <param name="lng">BD09经度</param>
        /// <param name="lat">BD09纬度</param>
        /// <returns>GCJ02坐标</returns>
        public static (double lng, double lat) Bd09ToGcj02(double lng, double lat)
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
        /// WGS84转BD09(百度坐标系)
        /// </summary>
        /// <param name="lng">WGS84经度</param>
        /// <param name="lat">WGS84纬度</param>
        /// <returns>BD09坐标</returns>
        public static (double lng, double lat) Wgs84ToBd09(double lng, double lat)
        {
            var (gcjLng, gcjLat) = Wgs84ToGcj02(lng, lat);
            return Gcj02ToBd09(gcjLng, gcjLat);
        }

        /// <summary>
        /// BD09(百度坐标系)转WGS84
        /// </summary>
        /// <param name="lng">BD09经度</param>
        /// <param name="lat">BD09纬度</param>
        /// <returns>WGS84坐标</returns>
        public static (double lng, double lat) Bd09ToWgs84(double lng, double lat)
        {
            var (gcjLng, gcjLat) = Bd09ToGcj02(lng, lat);
            return Gcj02ToWgs84(gcjLng, gcjLat);
        }

        /// <summary>
        /// 坐标转换的核心算法
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
        /// 统一的坐标系转换方法：将坐标从 sourceSystem 转换到 targetSystem。
        /// 所有高层次转换逻辑应集中在此方法中。
        /// </summary>
        public static (double lng, double lat) Convert(double lng, double lat, CoordinateSystem sourceSystem, CoordinateSystem targetSystem)
        {
            if (sourceSystem == targetSystem)
                return (lng, lat);

            return (sourceSystem, targetSystem) switch
            {
                (CoordinateSystem.WGS84, CoordinateSystem.GCJ02) => Wgs84ToGcj02(lng, lat),
                (CoordinateSystem.WGS84, CoordinateSystem.BD09) => Wgs84ToBd09(lng, lat),
                (CoordinateSystem.GCJ02, CoordinateSystem.WGS84) => Gcj02ToWgs84(lng, lat),
                (CoordinateSystem.GCJ02, CoordinateSystem.BD09) => Gcj02ToBd09(lng, lat),
                (CoordinateSystem.BD09, CoordinateSystem.WGS84) => Bd09ToWgs84(lng, lat),
                (CoordinateSystem.BD09, CoordinateSystem.GCJ02) => Bd09ToGcj02(lng, lat),
                _ => throw new ArgumentException($"不支持从 {sourceSystem} 转换到 {targetSystem}")
            };
        }

    }
}
