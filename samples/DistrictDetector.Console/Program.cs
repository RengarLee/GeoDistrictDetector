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
            Console.WriteLine("=== DistrictDetector.Console: Complete Address Lookup Test ===");
            var detector = CreateDetector();
            TestCompleteAddressLookup(detector);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Startup failed: {ex.Message}");
            Environment.Exit(1);
        }
    }

    static DistrictDetector CreateDetector()
    {
        string csvPath = "../../libs/GeoDistrictDetector/sample-cities.csv";
        Console.WriteLine($"Attempting to load data from CSV: {csvPath}");

        var detector = DistrictDetectorFactory.CreateFromCsv(csvPath);
        Console.WriteLine("✓ Successfully loaded data from CSV file");
        
        var districts = detector.GetAllDistricts();
        Console.WriteLine($"  Total loaded {districts.Count} administrative districts");
        var provinces = districts.Where(d => d.Deep == DistrictLevel.Province).Count();
        var cities = districts.Where(d => d.Deep == DistrictLevel.City).Count();
        var counties = districts.Where(d => d.Deep == DistrictLevel.County).Count();
        Console.WriteLine($"  Provinces: {provinces}, Cities: {cities}, Counties: {counties}");
        
        return detector;
    }

    static void TestCompleteAddressLookup(DistrictDetector detector)
    {
        Console.WriteLine("\n=== Complete Address Lookup Function Test ===");

        var testPoints = new[]
        {
            new { Name = "Shenzhen City Center", Lng = 114.0579, Lat = 22.5431 },
            new { Name = "Beijing City Center", Lng = 116.4074, Lat = 39.9042 },
            new { Name = "Shanghai City Center", Lng = 121.4737, Lat = 31.2304 },
            new { Name = "Invalid Coordinates", Lng = 0.0, Lat = 0.0 }
        };

        foreach (var point in testPoints)
        {
            Console.WriteLine($"\nTest coordinates: {point.Name} ({point.Lng}, {point.Lat})");
            var (province, city, district) = detector.FindCompleteAddressByCoordinate(point.Lng, point.Lat);
            if (province != null || city != null || district != null)
            {
                Console.WriteLine($"  Province: {province?.Name ?? "Not found"}");
                Console.WriteLine($"  City: {city?.Name ?? "Not found"}");
                Console.WriteLine($"  District: {district?.Name ?? "Not found"}");
            }
            else
            {
                Console.WriteLine("  No matching administrative district found");
            }
        }
    }
}
