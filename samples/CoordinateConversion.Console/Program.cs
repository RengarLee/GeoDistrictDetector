using GeoDistrictDetector;
using GeoDistrictDetector.Services;
using GeoDistrictDetector.Models;
using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== CoordinateConversion.Console: 坐标转换测试 ===");
            var detector = CreateDetector();
            TestCoordinateConversion(detector);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"启动失败: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static DistrictDetector CreateDetector()
    {
        string csvPath = "../../libs/GeoDistrictDetector/sample-cities.csv";
        Console.WriteLine($"尝试从CSV加载数据: {csvPath}");

        var detector = DistrictDetectorFactory.CreateFromCsv(csvPath);
        Console.WriteLine("✓ 从CSV文件成功加载数据");
        
        var districts = detector.GetAllDistricts();
        Console.WriteLine($"  共加载了 {districts.Count} 个行政区");
        var provinces = districts.Where(d => d.Deep == DistrictLevel.Province).Count();
        var cities = districts.Where(d => d.Deep == DistrictLevel.City).Count();
        var counties = districts.Where(d => d.Deep == DistrictLevel.County).Count();
        Console.WriteLine($"  省份: {provinces}, 城市: {cities}, 县区: {counties}");
        
        return detector;
    }

    static void TestCoordinateConversion(DistrictDetector detector)
    {
        Console.WriteLine("\n=== 坐标系转换功能测试 ===");

        var shenzhenWgs84 = new { Lng = 114.0579, Lat = 22.5431, System = "WGS84" };
        var shenzhenGcj02 = new { Lng = 114.064887, Lat = 22.548721, System = "GCJ02" };
        var shenzhenBd09 = new { Lng = 114.071166, Lat = 22.554745, System = "BD09" };

        Console.WriteLine("\n1. 测试WGS84坐标:");
        TestCoordinateSystemDetection(detector, shenzhenWgs84.Lng, shenzhenWgs84.Lat, CoordinateSystem.WGS84, shenzhenWgs84.System);

        Console.WriteLine("\n2. 测试GCJ02坐标:");
        TestCoordinateSystemDetection(detector, shenzhenGcj02.Lng, shenzhenGcj02.Lat, CoordinateSystem.GCJ02, shenzhenGcj02.System);

        Console.WriteLine("\n3. 测试BD09坐标:");
        TestCoordinateSystemDetection(detector, shenzhenBd09.Lng, shenzhenBd09.Lat, CoordinateSystem.BD09, shenzhenBd09.System);

        Console.WriteLine("\n=== 坐标转换演示 ===");
        TestCoordinateConversions();
    }

    static void TestCoordinateSystemDetection(DistrictDetector detector, double lng, double lat, CoordinateSystem system, string systemName)
    {
        Console.WriteLine($"原始{systemName}坐标: ({lng}, {lat})");
        var (province, city, district) = detector.FindCompleteAddressByCoordinate(lng, lat, system);
        if (province != null || city != null || district != null)
        {
            Console.WriteLine($"  检测结果 - 省份: {province?.Name ?? "未找到"}, 城市: {city?.Name ?? "未找到"}, 区县: {district?.Name ?? "未找到"}");
            var (convertedLng, convertedLat) = CoordinateConverter.Convert(lng, lat, system, CoordinateSystem.GCJ02);
            Console.WriteLine($"  转换为GCJ02坐标: ({convertedLng:F6}, {convertedLat:F6})");
        }
        else
        {
            Console.WriteLine("  未找到匹配的行政区");
        }
    }

    static void TestCoordinateConversions()
    {
        double wgs84Lng = 116.3974, wgs84Lat = 39.9093;
        Console.WriteLine($"原始WGS84坐标: ({wgs84Lng}, {wgs84Lat})");
        var (gcj02Lng, gcj02Lat) = CoordinateConverter.Wgs84ToGcj02(wgs84Lng, wgs84Lat);
        Console.WriteLine($"转换为GCJ02: ({gcj02Lng:F6}, {gcj02Lat:F6})");
        var (bd09Lng, bd09Lat) = CoordinateConverter.Wgs84ToBd09(wgs84Lng, wgs84Lat);
        Console.WriteLine($"转换为BD09: ({bd09Lng:F6}, {bd09Lat:F6})");
        var (backWgs84Lng, backWgs84Lat) = CoordinateConverter.Gcj02ToWgs84(gcj02Lng, gcj02Lat);
        Console.WriteLine($"GCJ02反转为WGS84: ({backWgs84Lng:F6}, {backWgs84Lat:F6})");
        var lngError = Math.Abs(wgs84Lng - backWgs84Lng);
        var latError = Math.Abs(wgs84Lat - backWgs84Lat);
        Console.WriteLine($"转换误差: 经度 {lngError:E6}, 纬度 {latError:E6}");
    }
}
