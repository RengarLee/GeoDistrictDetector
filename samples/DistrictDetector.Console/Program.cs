using GeoDistrictDetector;
using GeoDistrictDetector.Services;
using GeoDistrictDetector.Models;
using System;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== DistrictDetector.Console: 完整地址查找测试 ===");
            var detector = CreateDetector();
            TestCompleteAddressLookup(detector);
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

    static void TestCompleteAddressLookup(DistrictDetector detector)
    {
        Console.WriteLine("\n=== 完整地址查找功能测试 ===");

        var testPoints = new[]
        {
            new { Name = "深圳市区", Lng = 114.0579, Lat = 22.5431 },
            new { Name = "北京市区", Lng = 116.4074, Lat = 39.9042 },
            new { Name = "上海市区", Lng = 121.4737, Lat = 31.2304 },
            new { Name = "无效坐标", Lng = 0.0, Lat = 0.0 }
        };

        foreach (var point in testPoints)
        {
            Console.WriteLine($"\n测试坐标: {point.Name} ({point.Lng}, {point.Lat})");
            var (province, city, district) = detector.FindCompleteAddressByCoordinate(point.Lng, point.Lat);
            if (province != null || city != null || district != null)
            {
                Console.WriteLine($"  省份: {province?.Name ?? "未找到"}");
                Console.WriteLine($"  城市: {city?.Name ?? "未找到"}");
                Console.WriteLine($"  区县: {district?.Name ?? "未找到"}");
            }
            else
            {
                Console.WriteLine("  未找到匹配的行政区");
            }
        }
    }
}
