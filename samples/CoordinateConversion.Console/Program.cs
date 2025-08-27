using GeoDistrictDetector;
using System;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== CoordinateConversion.Console: Coordinate Conversion Test ===");
            TestCoordinateConversions();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Startup failed: {ex.Message}");
            Environment.Exit(1);
        }
    }

    // ...existing code...

    static void TestCoordinateConversions()
    {
        double wgs84Lng = 116.3974, wgs84Lat = 39.9093;
        Console.WriteLine($"Original WGS84 coordinates: ({wgs84Lng}, {wgs84Lat})");
        (double gcj02Lng, double gcj02Lat) = CoordinateConverter.Convert(wgs84Lng, wgs84Lat, CoordinateSystem.WGS84, CoordinateSystem.GCJ02);
        Console.WriteLine($"Converted to GCJ02: ({gcj02Lng:F6}, {gcj02Lat:F6})");
        (double bd09Lng, double bd09Lat) = CoordinateConverter.Convert(wgs84Lng, wgs84Lat, CoordinateSystem.WGS84, CoordinateSystem.BD09);
        Console.WriteLine($"Converted to BD09: ({bd09Lng:F6}, {bd09Lat:F6})");
        (double backWgs84Lng, double backWgs84Lat) = CoordinateConverter.Convert(gcj02Lng, gcj02Lat, CoordinateSystem.GCJ02, CoordinateSystem.WGS84);
        Console.WriteLine($"GCJ02 converted back to WGS84: ({backWgs84Lng:F6}, {backWgs84Lat:F6})");
        var lngError = Math.Abs(wgs84Lng - backWgs84Lng);
        var latError = Math.Abs(wgs84Lat - backWgs84Lat);
        Console.WriteLine($"Conversion error: Longitude {lngError:E6}, Latitude {latError:E6}");
    }
}
