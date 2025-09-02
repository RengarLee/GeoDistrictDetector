using GeoDistrictDetector.Models;
using GeoDistrictDetector.Services;

namespace DistrictDetectorBenchmark
{
    public class TestCsvLoading
    {
        public static void TestLoading()
        {
            Console.WriteLine("Starting CSV loading test...");
            
            string csvPath = "sample-cities.csv";
            Console.WriteLine($"CSV file path: {csvPath}");
            Console.WriteLine($"File exists: {File.Exists(csvPath)}");
            
            if (File.Exists(csvPath))
            {
                // Test DistrictLoader 直接加载
                var districts = DistrictLoader.LoadFromCsv(csvPath);
                Console.WriteLine($"Number of districts loaded by DistrictLoader: {districts.Count}");
                
                // Test DistrictDetectorFactory
                try
                {
                    var detector = DistrictDetectorFactory.CreateFromCsv(csvPath);
                    var allDistricts = detector.GetAllDistricts();
                    Console.WriteLine($"Number of districts in DistrictDetector: {allDistricts.Count}");
                    
                    // Test query
                    var result = detector.FindCountyByCoordinate(116.4, 39.9);
                    Console.WriteLine($"Query result: {result?.Name ?? "null"}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creating DistrictDetector: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }
    }
}
