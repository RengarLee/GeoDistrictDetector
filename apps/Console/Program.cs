using GeoDistrictDetector;
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        // 示例：判断点是否在多边形内
        var polygon = new List<(double lat, double lng)>
        {
            (39.9100, 116.4000),
            (39.9100, 116.4100),
            (39.9000, 116.4100),
            (39.9000, 116.4000)
        };
        var point = (39.9042, 116.4074); // 北京市东城区
        bool inside = GeoHelper.IsPointInPolygon(point, polygon);
        Console.WriteLine($"点({point.Item1}, {point.Item2}) 是否在多边形内: {inside}");

        // 初始化 District 列表（不读取内容，仅传递路径）
        string csvPath = @"../../libs/GeoDistrictDetector/sample-cities.csv";
        var districts = GeoDistrictDetector.Models.DistrictFactory.LoadFromCsv(csvPath);
        Console.WriteLine($"已初始化 {districts.Count} 个 District 实例。");
    }
}
