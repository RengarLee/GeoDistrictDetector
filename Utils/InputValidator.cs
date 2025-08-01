using GeoLocationCityDetector.Models;

namespace GeoLocationCityDetector.Utils
{
    /// <summary>
    /// 输入验证工具类
    /// </summary>
    public static class InputValidator
    {
        /// <summary>
        /// 验证经纬度是否有效
        /// </summary>
        /// <param name="latitude">纬度</param>
        /// <param name="longitude">经度</param>
        /// <returns>是否有效</returns>
        public static bool IsValidCoordinate(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 && longitude >= -180 && longitude <= 180;
        }

        /// <summary>
        /// 尝试解析坐标字符串
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <param name="point">解析结果</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParseCoordinate(string input, out GeoPoint? point)
        {
            point = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            // 支持多种格式：
            // 1. "39.9042,116.4074"
            // 2. "39.9042, 116.4074"
            // 3. "39.9042 116.4074"
            // 4. "(39.9042, 116.4074)"

            input = input.Trim().Trim('(', ')');
            string[] parts = input.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                if (double.TryParse(parts[0].Trim(), out double lat) &&
                    double.TryParse(parts[1].Trim(), out double lon))
                {
                    if (IsValidCoordinate(lat, lon))
                    {
                        point = new GeoPoint(lat, lon);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
