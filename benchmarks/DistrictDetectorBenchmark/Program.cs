using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GeoDistrictDetector.Models;
using GeoDistrictDetector.Services;
using NetTopologySuite.Geometries;

namespace DistrictDetectorBenchmark
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class FindCountyBenchmark
    {
        private DistrictDetector _detector = null!;
        private List<District> _testDistricts = null!;
        private List<(double Lng, double Lat)> _testCoordinates = null!;

        [GlobalSetup]
        public void Setup()
        {
            // 使用复制到输出目录的 CSV 文件
            string csvPath = "sample-cities.csv";
            _detector = DistrictDetectorFactory.CreateFromCsv(csvPath);
            _testDistricts = _detector.GetAllDistricts();
            
            // 生成测试坐标点（覆盖有数据和无数据的区域）
            _testCoordinates = GenerateTestCoordinates();
        }

        [Benchmark]
        public District? FindCountyByCoordinate_SinglePoint()
        {
            // 测试单个坐标查询性能
            return _detector.FindCountyByCoordinate(114.0579, 22.5431);
        }

        [Benchmark]
        public List<District?> FindCountyByCoordinate_MultiplePoints()
        {
            // 测试多个坐标查询性能
            var results = new List<District?>();
            foreach (var (lng, lat) in _testCoordinates)
            {
                results.Add(_detector.FindCountyByCoordinate(lng, lat));
            }
            return results;
        }

        [Benchmark]
        public List<District?> FindCountyByCoordinate_RandomPoints()
        {
            // 测试随机坐标查询性能
            var results = new List<District?>();
            var random = new Random(42); // 固定种子确保可重现
            
            for (int i = 0; i < 100; i++)
            {
                var lng = random.NextDouble() * 360 - 180; // -180 到 180
                var lat = random.NextDouble() * 180 - 90;  // -90 到 90
                results.Add(_detector.FindCountyByCoordinate(lng, lat));
            }
            return results;
        }

        [Benchmark]
        public List<District?> FindCountyByCoordinate_HotSpotPoints()
        {
            // 测试热点区域（数据密集区域）查询性能
            var results = new List<District?>();
            var hotSpots = new[]
            {
                (114.0, 22.5), (114.1, 22.5), (114.2, 22.5),
                (116.4, 39.9), (116.5, 39.9), (116.6, 39.9),
                (121.4, 31.2), (121.5, 31.2), (121.6, 31.2)
            };

            foreach (var (lng, lat) in hotSpots)
            {
                for (int i = 0; i < 10; i++) // 每个热点查询10次
                {
                    results.Add(_detector.FindCountyByCoordinate(lng + i * 0.01, lat + i * 0.01));
                }
            }
            return results;
        }

        private List<(double Lng, double Lat)> GenerateTestCoordinates()
        {
            var coordinates = new List<(double, double)>();
            var random = new Random(42);

            // 添加一些中国主要城市坐标
            coordinates.AddRange(new[]
            {
                (114.0579, 22.5431), // 深圳
                (116.4074, 39.9042), // 北京
                (121.4737, 31.2304), // 上海
                (113.2644, 23.1291), // 广州
                (104.0665, 30.5723), // 成都
                (108.9402, 34.3416), // 西安
                (120.1614, 30.2936), // 杭州
                (118.7674, 32.0415), // 南京
                (114.2999, 30.5844), // 武汉
                (117.2272, 31.8206)  // 合肥
            });

            // 添加一些随机坐标
            for (int i = 0; i < 20; i++)
            {
                var lng = 73 + random.NextDouble() * (135 - 73);
                var lat = 18 + random.NextDouble() * (53 - 18);
                coordinates.Add((lng, lat));
            }

            return coordinates;
        }
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("=== DistrictDetector FindCountyByCoordinate 基准测试 ===");
            
            // 首先测试 CSV 加载
            TestCsvLoading.TestLoading();
            
            Console.WriteLine("正在启动 BenchmarkDotNet...");
            BenchmarkRunner.Run<FindCountyBenchmark>();
            
            Console.WriteLine("基准测试完成！");
            Console.WriteLine("详细报告已保存到 BenchmarkDotNet.Artifacts 文件夹");
        }
    }
}
