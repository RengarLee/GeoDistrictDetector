using GeoDistrictDetector;
using System;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        // 初始化 District 列表（不读取内容，仅传递路径）
        string csvPath = @"../../libs/GeoDistrictDetector/sample-cities.csv";
        var districts = GeoDistrictDetector.Models.DistrictFactory.LoadFromCsv(csvPath);
        Console.WriteLine($"已初始化 {districts.Count} 个 District 实例。");
    }
}
