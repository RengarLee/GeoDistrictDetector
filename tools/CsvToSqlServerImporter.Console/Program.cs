using CsvToSqlServerImporter.Console;

namespace CsvToSqlServerImporter.Console;

/// <summary>
/// Console application for importing CSV district data directly to SQL Server
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("=== CSV to SQL Server Importer ===");
        System.Console.WriteLine("Direct bulk import of district CSV data to SQL Server\n");

        try
        {
            var config = ParseArguments(args);
            
            System.Console.WriteLine($"📁 CSV File: {config.CsvPath}");
            System.Console.WriteLine($"🗄️  Table: {config.TableName}");
            System.Console.WriteLine($"📊 Batch Size: {config.BatchSize}");
            System.Console.WriteLine($"🔧 Create Table: {config.CreateTable}");
            System.Console.WriteLine($"🗑️  Clear Table: {config.ClearTable}");
            System.Console.WriteLine();

            var importer = new CsvToSqlServerImporter(
                config.ConnectionString, 
                config.TableName, 
                config.CreateTable, 
                config.ClearTable,
                config.BatchSize);

            // Test connection first
            if (!await importer.TestConnectionAsync())
            {
                return;
            }

            System.Console.WriteLine();

            // Get existing record count if table exists
            var existingCount = await importer.GetTableRecordCountAsync();
            
            System.Console.WriteLine();

            // Import data
            var startTime = DateTime.Now;
            int importedCount = await importer.ImportCsvToSqlServerAsync(config.CsvPath);
            var duration = DateTime.Now - startTime;

            System.Console.WriteLine($"\n🎉 Import Summary:");
            System.Console.WriteLine($"   📥 Records imported: {importedCount:N0}");
            System.Console.WriteLine($"   ⏱️  Duration: {duration.TotalSeconds:F1} seconds");
            System.Console.WriteLine($"   🚀 Speed: {importedCount / Math.Max(duration.TotalSeconds, 1):F0} records/second");
            
            if (existingCount >= 0)
            {
                var newCount = await importer.GetTableRecordCountAsync();
                System.Console.WriteLine($"   📊 Table records: {existingCount:N0} → {newCount:N0}");
            }
        }
        catch (Exception ex) when (ex is FileNotFoundException or ArgumentException)
        {
            System.Console.WriteLine($"❌ Error: {ex.Message}");
            ShowUsage();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"💥 Unexpected error: {ex.Message}");
            System.Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }

        System.Console.WriteLine("\nPress any key to exit...");
        System.Console.ReadKey();
    }

    static ImportConfig ParseArguments(string[] args)
    {
        if (args.Length < 2)
            throw new ArgumentException("CSV file path and connection string are required.");

        var config = new ImportConfig
        {
            CsvPath = args[0],
            ConnectionString = args[1],
            TableName = "Districts",
            CreateTable = true,
            ClearTable = false,
            BatchSize = 100
        };

        // Parse optional parameters
        for (int i = 2; i < args.Length; i++)
        {
            var arg = args[i];
            
            if (arg.StartsWith("--table="))
                config.TableName = arg[8..];
            else if (arg == "--no-create-table")
                config.CreateTable = false;
            else if (arg == "--clear-table")
                config.ClearTable = true;
            else if (arg.StartsWith("--batch-size="))
            {
                if (int.TryParse(arg[13..], out int batchSize) && batchSize > 0)
                    config.BatchSize = batchSize;
                else
                    throw new ArgumentException($"Invalid batch size: {arg[13..]}");
            }
        }

        return config;
    }

    static void ShowUsage()
    {
        System.Console.WriteLine("""
        
        Usage:
          CsvToSqlServerImporter.Console.exe <csv_path> <connection_string> [options]
        
        Options:
          --table=<name>           Set table name (default: Districts)
          --no-create-table        Skip table creation (table must exist)
          --clear-table            Clear all data from table before import
          --batch-size=<size>      Set batch size for bulk insert (default: 100)
        
        Examples:
          # Using Windows Authentication
          CsvToSqlServerImporter.Console.exe input.csv "Server=localhost;Database=GeoData;Trusted_Connection=true;"
          
          # Using SQL Server Authentication
          CsvToSqlServerImporter.Console.exe input.csv "Server=localhost;Database=GeoData;User Id=sa;Password=mypass;"
          
          # Custom table and batch size
          CsvToSqlServerImporter.Console.exe input.csv "connectionstring" --table=MyDistricts --batch-size=1000
          
          # Don't create table (table must exist)
          CsvToSqlServerImporter.Console.exe input.csv "connectionstring" --no-create-table
          
          # Clear table data before import
          CsvToSqlServerImporter.Console.exe input.csv "connectionstring" --clear-table
        
        Connection String Examples:
          - Windows Auth: "Server=localhost;Database=GeoData;Trusted_Connection=true;"
          - SQL Auth: "Server=localhost;Database=GeoData;User Id=sa;Password=yourpass;"
          - Remote: "Server=192.168.1.100;Database=GeoData;User Id=user;Password=pass;"
        """);
    }
}

public class ImportConfig
{
    public string CsvPath { get; set; } = "";
    public string ConnectionString { get; set; } = "";
    public string TableName { get; set; } = "Districts";
    public bool CreateTable { get; set; } = false;
    public bool ClearTable { get; set; } = false;
    public int BatchSize { get; set; } = 100;
}
