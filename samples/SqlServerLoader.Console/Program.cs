using System;
using System.Linq;
using System.Threading.Tasks;
using GeoDistrictDetector.Models;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== SQL Server District Loader Test ===\n");

        // Get connection string from command line or use default
        string connectionString = GetConnectionString(args);
        
        if (string.IsNullOrEmpty(connectionString))
        {
            Console.WriteLine("Usage: SqlServerLoader.Console.exe [connection_string] [table_name] [batch_size]");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  SqlServerLoader.Console.exe \"Server=localhost;Database=GeoData;Trusted_Connection=true;\"");
            Console.WriteLine("  SqlServerLoader.Console.exe \"Data Source=192.168.9.62;Initial Catalog=GpsData;User ID=appuser;Password=TY5s2x9f4iK1lpYV;Encrypt=True;Trust Server Certificate=True;\" Districts 2000");
            return;
        }

        string tableName = args.Length > 1 ? args[1] : "Districts";
        int batchSize = args.Length > 2 && int.TryParse(args[2], out int size) ? size : 1000;

        try
        {
            await TestAsyncLoading(connectionString, tableName, batchSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static string GetConnectionString(string[] args)
    {
        if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
        {
            return args[0];
        }

        throw new ArgumentException("Connection string is required. Please provide a valid SQL Server connection string as the first argument.");
    }

    private static async Task TestAsyncLoading(string connectionString, string tableName, int batchSize)
    {
        Console.WriteLine($"🚀 Testing ASYNC loading from SQL Server");
        Console.WriteLine($"Connection: {connectionString}");
        Console.WriteLine($"Table: {tableName}");
        Console.WriteLine($"Batch Size: {batchSize}");
        Console.WriteLine();

        var startTime = DateTime.Now;

        try
        {
            // Test async loading
            var districts = await DistrictLoader.LoadFromSqlServerAsync(connectionString, tableName, batchSize);
            
            var endTime = DateTime.Now;
            var duration = endTime - startTime;

            Console.WriteLine($"✅ Async loading completed successfully!");
            Console.WriteLine($"📊 Results:");
            Console.WriteLine($"   Total Districts: {districts.Count:N0}");
            Console.WriteLine($"   Loading Time: {duration.TotalSeconds:F2} seconds");
            Console.WriteLine($"   Records/Second: {districts.Count / duration.TotalSeconds:F0}");

            if (districts.Count > 0)
            {
                Console.WriteLine($"\n📋 Sample Data (first 3 records):");
                for (int i = 0; i < Math.Min(3, districts.Count); i++)
                {
                    var d = districts[i];
                    Console.WriteLine($"   {i + 1}. ID:{d.Id}, Name:{d.Name}, Level:{d.Deep}, ExtPath:{d.ExtPath}");
                }

                // Statistics by level
                var byLevel = districts.GroupBy(d => d.Deep).ToDictionary(g => g.Key, g => g.Count());
                Console.WriteLine($"\n📈 Distribution by Level:");
                foreach (var kvp in byLevel.OrderBy(x => x.Key))
                {
                    Console.WriteLine($"   {kvp.Key}: {kvp.Value:N0} districts");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Async loading failed: {ex.Message}");
            throw;
        }
    }
}
