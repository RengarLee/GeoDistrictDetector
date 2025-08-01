using GeoLocationCityDetector.Models;
using GeoLocationCityDetector.Services;
using GeoLocationCityDetector.Utils;

namespace GeoLocationCityDetector
{
    class Program
    {
        private static IGeoLocationService _geoService = new GeoLocationService();

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== 地理位置城市检测器 ===");
            Console.WriteLine();

            try
            {
                // 处理命令行参数
                if (args.Length > 0)
                {
                    await ProcessCommandLineArgs(args);
                    return;
                }

                // 交互式模式
                await RunInteractiveMode();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序执行出错: {ex.Message}");
                Environment.Exit(1);
            }
        }

        private static async Task ProcessCommandLineArgs(string[] args)
        {
            if (args.Length < 3)
            {
                ShowUsage();
                return;
            }

            string dataFile = args[0];
            if (!double.TryParse(args[1], out double latitude) || 
                !double.TryParse(args[2], out double longitude))
            {
                Console.WriteLine("错误：无效的坐标格式");
                ShowUsage();
                return;
            }

            if (!InputValidator.IsValidCoordinate(latitude, longitude))
            {
                Console.WriteLine("错误：坐标超出有效范围");
                return;
            }

            await LoadDataAndQuery(dataFile, new GeoPoint(latitude, longitude));
        }

        private static async Task RunInteractiveMode()
        {
            string? dataFile = null;

            while (true)
            {
                Console.WriteLine("\n请选择操作：");
                Console.WriteLine("1. 加载城市数据文件");
                Console.WriteLine("2. 查询坐标所属城市");
                Console.WriteLine("3. 显示已加载的城市数量");
                Console.WriteLine("4. 退出");
                Console.Write("请输入选项 (1-4): ");

                string? choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        dataFile = await LoadDataFile();
                        break;
                    case "2":
                        if (string.IsNullOrEmpty(dataFile))
                        {
                            Console.WriteLine("请先加载城市数据文件");
                            break;
                        }
                        await QueryCoordinate();
                        break;
                    case "3":
                        ShowLoadedCitiesCount();
                        break;
                    case "4":
                        Console.WriteLine("感谢使用地理位置城市检测器！");
                        return;
                    default:
                        Console.WriteLine("无效选项，请重新输入");
                        break;
                }
            }
        }

        private static async Task<string?> LoadDataFile()
        {
            Console.Write("请输入城市数据文件路径 (支持CSV和JSON格式): ");
            string? filePath = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                Console.WriteLine("文件路径不能为空");
                return null;
            }

            try
            {
                await _geoService.LoadCityDataAsync(filePath);
                Console.WriteLine("城市数据加载成功！");
                return filePath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载失败: {ex.Message}");
                return null;
            }
        }

        private static async Task QueryCoordinate()
        {
            Console.Write("请输入坐标 (格式：纬度,经度，如：39.9042,116.4074): ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("输入不能为空");
                return;
            }

            if (!InputValidator.TryParseCoordinate(input, out GeoPoint? point) || point == null)
            {
                Console.WriteLine("无效的坐标格式，请使用：纬度,经度");
                return;
            }

            try
            {
                var city = await _geoService.FindCityByPointAsync(point);
                if (city != null)
                {
                    Console.WriteLine($"查询结果：");
                    Console.WriteLine($"  坐标: {point}");
                    Console.WriteLine($"  所属城市: {city}");
                    Console.WriteLine($"  城市中心点: {city.CenterPoint}");
                }
                else
                {
                    Console.WriteLine($"未找到坐标 {point} 对应的城市信息");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"查询失败: {ex.Message}");
            }
        }

        private static void ShowLoadedCitiesCount()
        {
            var cities = _geoService.GetAllCities();
            Console.WriteLine($"已加载 {cities.Count} 个城市数据");
            
            if (cities.Count > 0)
            {
                Console.WriteLine("示例城市：");
                for (int i = 0; i < Math.Min(5, cities.Count); i++)
                {
                    Console.WriteLine($"  {cities[i]}");
                }
                
                if (cities.Count > 5)
                {
                    Console.WriteLine($"  ... 还有 {cities.Count - 5} 个城市");
                }
            }
        }

        private static async Task LoadDataAndQuery(string dataFile, GeoPoint point)
        {
            Console.WriteLine($"加载数据文件: {dataFile}");
            await _geoService.LoadCityDataAsync(dataFile);

            Console.WriteLine($"查询坐标: {point}");
            var city = await _geoService.FindCityByPointAsync(point);
            
            if (city != null)
            {
                Console.WriteLine($"结果: {city}");
            }
            else
            {
                Console.WriteLine("未找到对应的城市信息");
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("用法:");
            Console.WriteLine("  GeoLocationCityDetector.exe <数据文件路径> <纬度> <经度>");
            Console.WriteLine();
            Console.WriteLine("示例:");
            Console.WriteLine("  GeoLocationCityDetector.exe cities.csv 39.9042 116.4074");
            Console.WriteLine();
            Console.WriteLine("或者直接运行程序进入交互式模式");
        }
    }
}
