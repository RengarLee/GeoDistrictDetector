using System.Data;
using Microsoft.Data.SqlClient;

namespace CsvToSqlServerImporter.Console;

/// <summary>
/// CSV to SQL Server Importer - Direct bulk import of district CSV data to SQL Server
/// </summary>
public class CsvToSqlServerImporter
{
    private readonly string _connectionString;
    private readonly string _tableName;
    private readonly bool _createTable;
    private readonly bool _clearTable;
    private readonly int _batchSize;

    public CsvToSqlServerImporter(string connectionString, string tableName = "Districts", bool createTable = true, bool clearTable = false, int batchSize = 100)
    {
        _connectionString = connectionString;
        _tableName = tableName;
        _createTable = createTable;
        _clearTable = clearTable;
        _batchSize = batchSize;
    }

    /// <summary>
    /// Import CSV file directly to SQL Server using SqlBulkCopy
    /// </summary>
    public async Task<int> ImportCsvToSqlServerAsync(string csvFilePath)
    {
        if (!File.Exists(csvFilePath))
            throw new FileNotFoundException($"CSV file not found: {csvFilePath}");

        var dataLines = (await File.ReadAllLinesAsync(csvFilePath))
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();
        
        if (dataLines.Count == 0)
        {
            System.Console.WriteLine("No valid data rows found in CSV file.");
            return 0;
        }

        System.Console.WriteLine($"Found {dataLines.Count} data rows in CSV");

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await PrepareTableAsync(connection);
        return await ImportDataAsync(connection, dataLines);
    }

    private async Task PrepareTableAsync(SqlConnection connection)
    {
        if (_createTable)
            await CreateTableAsync(connection);
        
        if (_clearTable)
            await ClearTableAsync(connection);
    }

    private async Task CreateTableAsync(SqlConnection connection)
    {
        var sql = $"""
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{_tableName}' AND xtype='U')
            CREATE TABLE {_tableName} (
                Id INT PRIMARY KEY, Pid INT, Deep INT,
                Name NVARCHAR(255), ExtPath NVARCHAR(500),
                Geo TEXT, Polygon TEXT
            );
            """;

        await ExecuteSqlAsync(connection, sql, $"Creating table {_tableName}");
    }

    private async Task ClearTableAsync(SqlConnection connection)
    {
        var deletedCount = await ExecuteSqlAsync(connection, $"DELETE FROM {_tableName}", 
            $"Clearing table {_tableName}");
        System.Console.WriteLine($"Cleared {deletedCount} existing records");
    }

    private async Task<int> ExecuteSqlAsync(SqlConnection connection, string sql, string message)
    {
        System.Console.WriteLine(message);
        using var command = new SqlCommand(sql, connection);
        return await command.ExecuteNonQueryAsync();
    }

    private async Task<int> ImportDataAsync(SqlConnection connection, List<string> csvLines)
    {
        var totalBatches = (int)Math.Ceiling((double)csvLines.Count / _batchSize);
        var importedCount = 0;
        
        System.Console.WriteLine($"Starting import: {csvLines.Count} records in {totalBatches} batches\n");
        
        for (int i = 0; i < csvLines.Count; i += _batchSize)
        {
            var batch = csvLines.Skip(i).Take(_batchSize).ToList();
            var batchNumber = (i / _batchSize) + 1;
            
            ConsoleProgressBar.ShowWithCount(batchNumber - 1, totalBatches, "Importing");
            await ImportBatchAsync(connection, batch);
            importedCount += batch.Count;
            ConsoleProgressBar.ShowWithCount(batchNumber, totalBatches, "Importing");
            
            await Task.Delay(10);
        }

        ConsoleProgressBar.Clear();
        System.Console.WriteLine($"✅ Imported {importedCount} records successfully!");
        return importedCount;
    }

    private async Task ImportBatchAsync(SqlConnection connection, List<string> batch)
    {
        var dataTable = CreateDataTable();
        
        foreach (var csvLine in batch)
        {
            var fields = ParseCsvLine(csvLine);
            if (fields.Count < 7) continue;
            
            dataTable.Rows.Add(
                ParseInt(fields[0]), ParseInt(fields[1]), ParseInt(fields[2]),
                CleanField(fields[3]), CleanField(fields[4]), 
                CleanField(fields[5]), CleanField(fields[6])
            );
        }

        using var bulkCopy = new SqlBulkCopy(connection)
        {
            DestinationTableName = _tableName,
            BatchSize = _batchSize,
            BulkCopyTimeout = 300
        };

        // Auto-map columns
        foreach (DataColumn column in dataTable.Columns)
            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);

        await bulkCopy.WriteToServerAsync(dataTable);
    }

    private static int ParseInt(string value) => 
        int.TryParse(value.Trim(), out int result) ? result : 0;

    private static string CleanField(string field) => 
        field.Trim().Trim('"', '\\');

    private static DataTable CreateDataTable()
    {
        var table = new DataTable();
        var columns = new[] { "Id", "Pid", "Deep", "Name", "ExtPath", "Geo", "Polygon" };
        var types = new[] { typeof(int), typeof(int), typeof(int), typeof(string), typeof(string), typeof(string), typeof(string) };
        
        for (int i = 0; i < columns.Length; i++)
            table.Columns.Add(columns[i], types[i]);
        
        return table;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        var field = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (char c in line)
        {
            if (c == '"')
                inQuotes = !inQuotes;
            else if (c == ',' && !inQuotes)
            {
                result.Add(field.ToString());
                field.Clear();
            }
            else
                field.Append(c);
        }
        
        result.Add(field.ToString());
        return result;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            System.Console.WriteLine($"✅ Connected to {connection.DataSource}/{connection.Database}");
            return true;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Connection failed: {ex.Message}");
            return false;
        }
    }

    public async Task<int> GetTableRecordCountAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new SqlCommand($"SELECT COUNT(*) FROM {_tableName}", connection);
            var count = Convert.ToInt32(await command.ExecuteScalarAsync());
            
            System.Console.WriteLine($"📊 Table {_tableName}: {count} records");
            return count;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Count failed: {ex.Message}");
            return -1;
        }
    }
}
