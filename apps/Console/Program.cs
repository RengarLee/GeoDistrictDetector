using GeoDistrictDetector;
using GeoDistrictDetector.Services;
using GeoDistrictDetector.Models;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== DistrictDetector Factory 测试 ===");
            var detector = CreateDetector();
            TestCompleteAddressLookup(detector);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"应用启动失败: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static DistrictDetector CreateDetector()
    {
        string csvPath = "../../libs/GeoDistrictDetector/sample-cities.csv";
        Console.WriteLine($"尝试从CSV加载数据: {csvPath}");

        var detector = DistrictDetectorFactory.CreateFromCsv(csvPath);
        Console.WriteLine("✓ 从CSV文件成功加载数据");
        return detector;
    }

    static void TestCompleteAddressLookup(DistrictDetector detector)
    {
        Console.WriteLine("\n=== 完整地址查找功能测试 ===");

        // 测试几个坐标点
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
            try
            {
                var (province, city, district) = detector.FindCompleteAddressByCoordinate(point.Lng, point.Lat);
                if (province != null || city != null || district != null)
                {
                    Console.WriteLine("  ✓ 找到地址信息:");
                    if (province != null) Console.WriteLine($"    省份: {province.Name} (ID: {province.Id})");
                    if (city != null) Console.WriteLine($"    城市: {city.Name} (ID: {city.Id})");
                    if (district != null) Console.WriteLine($"    县区: {district.Name} (ID: {district.Id})");
                    var fullPath = $"{province?.Name} {city?.Name} {district?.Name}".Trim();
                    Console.WriteLine($"    完整地址: {fullPath}");
                }
                else
                {
                    Console.WriteLine("  ✗ 未找到匹配的地址");
                }
                TestIndividualQueries(detector, point.Lng, point.Lat);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ 查询出错: {ex.Message}");
            }
        }
    }

    static void TestIndividualQueries(DistrictDetector detector, double lng, double lat)
    {
        var province = detector.FindProvinceByCoordinate(lng, lat);
        var city = detector.FindCityByCoordinate(lng, lat);
        var district = detector.FindDistrictByCoordinate(lng, lat);
        Console.WriteLine("    分别查询结果:");
        Console.WriteLine($"      省份查询: {province?.Name ?? "无"}");
        Console.WriteLine($"      城市查询: {city?.Name ?? "无"}");
        Console.WriteLine($"      县区查询: {district?.Name ?? "无"}");
    }
}
