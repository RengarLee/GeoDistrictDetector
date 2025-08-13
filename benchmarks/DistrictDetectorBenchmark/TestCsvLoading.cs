using GeoDistrictDetector.Models;
using GeoDistrictDetector.Services;

namespace DistrictDetectorBenchmark
{
    public class TestCsvLoading
    {
        public static void TestLoading()
        {
            Console.WriteLine("开始测试 CSV 加载...");
            
            string csvPath = "sample-cities.csv";
            Console.WriteLine($"CSV 文件路径: {csvPath}");
            Console.WriteLine($"文件是否存在: {File.Exists(csvPath)}");
            
            if (File.Exists(csvPath))
            {
                // 直接测试 DistrictFactory
                var districts = DistrictFactory.LoadFromCsv(csvPath);
                Console.WriteLine($"DistrictFactory 加载的区域数量: {districts.Count}");
                
                // 测试 DistrictDetectorFactory
                try
                {
                    var detector = DistrictDetectorFactory.CreateFromCsv(csvPath);
                    var allDistricts = detector.GetAllDistricts();
                    Console.WriteLine($"DistrictDetector 中的区域数量: {allDistricts.Count}");
                    
                    // 测试查询
                    var result = detector.FindCountyByCoordinate(116.4, 39.9);
                    Console.WriteLine($"查询结果: {result?.Name ?? "null"}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"创建 DistrictDetector 时出错: {ex.Message}");
                    Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                }
            }
        }
    }
}
