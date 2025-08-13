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
        // 初始化 District 列表（不读取内容，仅传递路径）
        string csvPath = @"../../libs/GeoDistrictDetector/sample-cities.csv";
        var districts = GeoDistrictDetector.Models.DistrictFactory.LoadFromCsv(csvPath);
        Console.WriteLine($"从CSV加载了 {districts.Count} 个 District 实例。");

        // 如果CSV文件为空，创建一些测试数据
        if (districts.Count == 0)
        {
            districts = CreateTestDistricts();
            Console.WriteLine($"创建了 {districts.Count} 个测试 District 实例。");
        }

        // 创建地理位置服务并加载数据
        var geoService = new GeoDistrictService();
        geoService.LoadDistrictData(districts);
        Console.WriteLine("空间索引构建完成！");

        // 测试城市查找功能
        TestCityLookup(geoService);
    }

    static List<District> CreateTestDistricts()
    {
        var factory = GeometryFactory.Default;
        var districts = new List<District>();

        // 创建深圳市的测试数据 (简化的矩形区域)
        var shenzhenCoords = new[]
        {
            new Coordinate(113.8, 22.4),  // 左下
            new Coordinate(114.6, 22.4),  // 右下
            new Coordinate(114.6, 22.8),  // 右上
            new Coordinate(113.8, 22.8),  // 左上
            new Coordinate(113.8, 22.4)   // 闭合
        };
        var shenzhenRing = factory.CreateLinearRing(shenzhenCoords);
        var shenzhenPolygon = factory.CreatePolygon(shenzhenRing);
        
        districts.Add(new District(
            id: 1, 
            pid: 0, 
            deep: DistrictLevel.City, 
            name: "深圳市", 
            extPath: "广东省 深圳市", 
            geo: new Coordinate(114.0579, 22.5431), 
            polygon: shenzhenPolygon
        ));

        // 创建北京市的测试数据
        var beijingCoords = new[]
        {
            new Coordinate(116.0, 39.4),
            new Coordinate(117.0, 39.4),
            new Coordinate(117.0, 40.4),
            new Coordinate(116.0, 40.4),
            new Coordinate(116.0, 39.4)
        };
        var beijingRing = factory.CreateLinearRing(beijingCoords);
        var beijingPolygon = factory.CreatePolygon(beijingRing);
        
        districts.Add(new District(
            id: 2, 
            pid: 0, 
            deep: DistrictLevel.City, 
            name: "北京市", 
            extPath: "北京市", 
            geo: new Coordinate(116.4074, 39.9042), 
            polygon: beijingPolygon
        ));

        return districts;
    }

    static void TestCityLookup(IGeoDistrictService geoService)
    {
        Console.WriteLine("\n=== 城市查找功能测试 ===");
        
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
                var city = geoService.FindCityByCoordinate(point.Lng, point.Lat);
                if (city != null)
                {
                    Console.WriteLine($"  ✓ 找到城市: {city.Name}");
                    Console.WriteLine($"  完整路径: {city.ExtPath}");
                    Console.WriteLine($"  区域ID: {city.Id}");
                    Console.WriteLine($"  层级: {city.Deep}");
                }
                else
                {
                    Console.WriteLine($"  ✗ 未找到匹配的城市");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  ✗ 查询出错: {ex.Message}");
            }
        }
    }
}
