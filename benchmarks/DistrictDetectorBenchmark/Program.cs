using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GeoDistrictDetector.Models;
using GeoDistrictDetector.Services;
using NetTopologySuite.Geometries;

namespace DistrictDetectorBenchmark
{
    [MemoryDiagnoser]
    [SimpleJob]
    public class FindBenchmark
    {
        private DistrictDetector _detector = null!;
        private List<District> _testDistricts = null!;
        private List<(double Lng, double Lat)> _testCoordinates = null!;

        [GlobalSetup]
        public void Setup()
        {
            // Use CSV file copied to output directory
            string csvPath = "sample-cities.csv";
            _detector = DistrictDetectorFactory.CreateFromCsv(csvPath);
            _testDistricts = _detector.GetAllDistricts();
            
            // Generate test coordinates (covering areas with and without data)
            _testCoordinates = GenerateTestCoordinates();
        }

        [Benchmark]
        public District? FindCountyByCoordinate_SinglePoint()
        {
            // Test single coordinate query performance - original version
            return _detector.FindCountyByCoordinate(114.0579, 22.5431);
        }

        [Benchmark]
        public List<District?> FindCountyByCoordinate_MultiplePoints()
        {
            // Test multiple coordinate query performance - original version
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
            // Test random coordinate query performance - original version
            var results = new List<District?>();
            var random = new Random(42); // Fixed seed for reproducible results
            
            for (int i = 0; i < 100; i++)
            {
                var lng = random.NextDouble() * 360 - 180; // -180 to 180
                var lat = random.NextDouble() * 180 - 90;  // -90 to 90
                results.Add(_detector.FindCountyByCoordinate(lng, lat));
            }
            return results;
        }

        [Benchmark]
        public List<District?> FindCountyByCoordinate_HotSpotPoints()
        {
            // Test hotspot area (data-dense regions) query performance - original version
            var results = new List<District?>();
            var hotSpots = new[]
            {
                (114.0, 22.5), (114.1, 22.5), (114.2, 22.5),
                (116.4, 39.9), (116.5, 39.9), (116.6, 39.9),
                (121.4, 31.2), (121.5, 31.2), (121.6, 31.2)
            };

            foreach (var (lng, lat) in hotSpots)
            {
                for (int i = 0; i < 10; i++) // Query each hotspot 10 times
                {
                    results.Add(_detector.FindCountyByCoordinate(lng + i * 0.01, lat + i * 0.01));
                }
            }
            return results;
        }
        // ====================== FindProvinceByCoordinate Benchmarks ======================

        [Benchmark]
        public List<District?> FindProvinceByCoordinate_SinglePoint()
        {
            // Test single point province query performance - original version
            var results = new List<District?>();
            for (int i = 0; i < 1000; i++)
            {
                results.Add(_detector.FindProvinceByCoordinate(116.4074, 39.9042)); // Beijing
            }
            return results;
        }

        [Benchmark]
        public List<District?> FindProvinceByCoordinate_MultiplePoints()
        {
            // Test multiple points province query performance - original version
            var results = new List<District?>();
            var coordinates = new (double lng, double lat)[]
            {
                (116.4074, 39.9042), // Beijing
                (121.4737, 31.2304), // Shanghai
                (113.2644, 23.1291), // Guangzhou
                (104.0665, 30.5723), // Chengdu
                (108.9480, 34.2588)  // Xi'an
            };

            for (int i = 0; i < 200; i++)
            {
                var (lng, lat) = coordinates[i % coordinates.Length];
                results.Add(_detector.FindProvinceByCoordinate(lng, lat));
            }
            return results;
        }

        [Benchmark]
        public List<District?> FindProvinceByCoordinate_RandomPoints()
        {
            // Test random coordinate province query performance - original version
            var results = new List<District?>();
            var random = new Random(42); // Fixed seed for reproducible results
            
            for (int i = 0; i < 100; i++)
            {
                var lng = random.NextDouble() * 360 - 180; // -180 to 180
                var lat = random.NextDouble() * 180 - 90;  // -90 to 90
                results.Add(_detector.FindProvinceByCoordinate(lng, lat));
            }
            return results;
        }

        // ====================== FindCityByCoordinate Benchmarks ======================

        [Benchmark]
        public List<District?> FindCityByCoordinate_SinglePoint()
        {
            // Test single point city query performance - original version
            var results = new List<District?>();
            for (int i = 0; i < 1000; i++)
            {
                results.Add(_detector.FindCityByCoordinate(116.4074, 39.9042)); // Beijing
            }
            return results;
        }

        [Benchmark]
        public List<District?> FindCityByCoordinate_MultiplePoints()
        {
            // Test multiple points city query performance - original version
            var results = new List<District?>();
            var coordinates = new (double lng, double lat)[]
            {
                (116.4074, 39.9042), // Beijing
                (121.4737, 31.2304), // Shanghai
                (113.2644, 23.1291), // Guangzhou
                (104.0665, 30.5723), // Chengdu
                (108.9480, 34.2588)  // Xi'an
            };

            for (int i = 0; i < 200; i++)
            {
                var (lng, lat) = coordinates[i % coordinates.Length];
                results.Add(_detector.FindCityByCoordinate(lng, lat));
            }
            return results;
        }

        [Benchmark]
        public List<District?> FindCityByCoordinate_RandomPoints()
        {
            // Test random coordinate city query performance - original version
            var results = new List<District?>();
            var random = new Random(42); // Fixed seed for reproducible results
            
            for (int i = 0; i < 100; i++)
            {
                var lng = random.NextDouble() * 360 - 180; // -180 to 180
                var lat = random.NextDouble() * 180 - 90;  // -90 to 90
                results.Add(_detector.FindCityByCoordinate(lng, lat));
            }
            return results;
        }


        private List<(double Lng, double Lat)> GenerateTestCoordinates()
        {
            var coordinates = new List<(double, double)>();
            var random = new Random(42);

            // Add coordinates for major Chinese cities
            coordinates.AddRange(new[]
            {
                (114.0579, 22.5431), // Shenzhen
                (116.4074, 39.9042), // Beijing
                (121.4737, 31.2304), // Shanghai
                (113.2644, 23.1291), // Guangzhou
                (104.0665, 30.5723), // Chengdu
                (108.9402, 34.3416), // Xi'an
                (120.1614, 30.2936), // Hangzhou
                (118.7674, 32.0415), // Nanjing
                (114.2999, 30.5844), // Wuhan
                (117.2272, 31.8206)  // Hefei
            });

            // Add some random coordinates
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
            Console.WriteLine("=== DistrictDetector Benchmark Tests ===");
            
            // Test CSV loading first
            TestCsvLoading.TestLoading();
            
            Console.WriteLine("Starting BenchmarkDotNet...");
            Console.WriteLine("Running performance tests for DistrictDetector");
            
            // Run benchmarks
            BenchmarkRunner.Run<FindBenchmark>();
            
            Console.WriteLine("Benchmark completed!");
            Console.WriteLine("Detailed report saved to BenchmarkDotNet.Artifacts folder");
            Console.WriteLine("Check performance metrics and memory usage");
        }
    }
}
