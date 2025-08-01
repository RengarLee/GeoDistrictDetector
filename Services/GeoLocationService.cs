using GeoLocationCityDetector.Models;
using System.Text.Json;

namespace GeoLocationCityDetector.Services
{
    /// <summary>
    /// 地理位置服务实现
    /// </summary>
    public class GeoLocationService : IGeoLocationService
    {
        private List<CityInfo> _cities = new List<CityInfo>();
        private readonly double _earthRadius = 6371; // 地球半径（公里）

        public async Task<CityInfo?> FindCityByPointAsync(GeoPoint point)
        {
            if (_cities.Count == 0)
            {
                throw new InvalidOperationException("城市数据未加载，请先调用 LoadCityDataAsync 方法");
            }

            // 方法1：使用最近距离算法（简单实现）
            CityInfo? nearestCity = null;
            double minDistance = double.MaxValue;

            foreach (var city in _cities)
            {
                double distance = CalculateDistance(point, city.CenterPoint);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestCity = city;
                }
            }

            // 方法2：如果有边界数据，可以使用点在多边形内判断算法
            var cityWithBoundary = _cities.FirstOrDefault(c => 
                c.Boundary.Count > 0 && IsPointInPolygon(point, c.Boundary));
            
            return cityWithBoundary ?? nearestCity;
        }

        public async Task LoadCityDataAsync(string dataFilePath)
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

                Console.WriteLine($"成功加载 {_cities.Count} 个城市数据");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载城市数据失败: {ex.Message}");
                throw;
            }
        }

        public List<CityInfo> GetAllCities()
        {
            return _cities.ToList();
        }

        private async Task LoadFromJsonAsync(string filePath)
        {
            string jsonContent = await File.ReadAllTextAsync(filePath);
            var cities = JsonSerializer.Deserialize<List<CityInfo>>(jsonContent);
            if (cities != null)
            {
                _cities = cities;
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
                    // 假设CSV格式为: 省份,城市,区县,经度,纬度
                    string[] parts = line.Split(',');
                    if (parts.Length >= 5)
                    {
                        string province = parts[0].Trim();
                        string city = parts[1].Trim();
                        string district = parts[2].Trim();
                        
                        if (double.TryParse(parts[3].Trim(), out double longitude) &&
                            double.TryParse(parts[4].Trim(), out double latitude))
                        {
                            var centerPoint = new GeoPoint(latitude, longitude);
                            var cityInfo = new CityInfo(province, city, district, centerPoint);
                            _cities.Add(cityInfo);
                        }
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

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }
    }
}
