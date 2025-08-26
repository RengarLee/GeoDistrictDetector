# SQL Server Loader Console Test

This console application demonstrates how to load District data from SQL Server using both async and sync methods.

## Features

- ✅ **Async Loading** - Core async implementation with cancellation support
- ✅ **Sync Loading** - Compatibility wrapper for synchronous operations  
- ✅ **Performance Metrics** - Loading time and throughput measurement
- ✅ **Data Validation** - Geometry and coordinate validation
- ✅ **Batch Processing** - Configurable batch sizes for optimal performance
- ✅ **Security** - Connection string masking for safe logging

## Usage

### Basic Usage
```bash
dotnet run
```
Uses default connection string (localhost with Windows Authentication)

### With Custom Connection String
```bash
dotnet run "Server=localhost;Database=GeoData;Trusted_Connection=true;"
```

### With All Parameters
```bash
dotnet run "connection_string" "table_name" batch_size
```

## Examples

### Local SQL Server
```bash
dotnet run "Server=localhost;Database=GeoData;Trusted_Connection=true;" Districts 1000
```

### Remote SQL Server
```bash
dotnet run "Data Source=192.168.9.62;Initial Catalog=GpsData;User ID=appuser;Password=YourPassword;Encrypt=True;Trust Server Certificate=True;" Districts 2000
```

### Azure SQL Database
```bash
dotnet run "Server=tcp:yourserver.database.windows.net,1433;Initial Catalog=GeoData;User ID=yourusername;Password=yourpassword;Encrypt=True;TrustServerCertificate=False;" Districts 500
```

## Parameters

| Parameter | Description | Default | Example |
|-----------|-------------|---------|---------|
| `connection_string` | SQL Server connection string | localhost with Windows Auth | See examples above |
| `table_name` | Database table name | `Districts` | `Districts`, `GeoData`, etc. |
| `batch_size` | Records per batch | `1000` | `500`, `2000`, `5000` |

## Expected Output

```
=== SQL Server District Loader Test ===

🚀 Testing ASYNC loading from SQL Server
Connection: Server=localhost;Database=GeoData;Trusted_Connection=true;
Table: Districts
Batch Size: 1000

✅ Async loading completed successfully!
📊 Results:
   Total Districts: 45,678 districts
   Loading Time: 2.34 seconds
   Records/Second: 19,521

📋 Sample Data (first 3 records):
   1. ID:1, Name:Beijing, Level:Province, ExtPath:Beijing
   2. ID:2, Name:Shanghai, Level:Province, ExtPath:Shanghai  
   3. ID:3, Name:Guangdong, Level:Province, ExtPath:Guangdong

📈 Distribution by Level:
   Province: 34 districts
   City: 1,234 districts
   County: 44,410 districts

==================================================
🔄 Testing SYNC loading from SQL Server (compatibility)

✅ Sync loading completed successfully!
📊 Results:
   Total Districts: 45,678 districts
   Loading Time: 2.41 seconds
   Records/Second: 18,956

🔍 Data Validation:
   Valid Polygons: 45,234 (99.0%)
   Valid Coordinates: 45,678 (100.0%)
```

## Performance Tips

- **Batch Size**: Larger batches (2000-5000) are generally faster for large datasets
- **Network**: Use connection pooling for production scenarios
- **Memory**: Monitor memory usage with very large datasets
- **Async**: Prefer async methods for better scalability

## Troubleshooting

### Connection Issues
- Verify SQL Server is running and accessible
- Check firewall settings for remote connections
- Ensure credentials are correct
- Test with SQL Server Management Studio first

### Performance Issues  
- Try different batch sizes (500, 1000, 2000, 5000)
- Check network latency for remote connections
- Monitor SQL Server performance counters
- Consider indexing on the `Id` column for OFFSET/FETCH queries

### Data Issues
- Verify table schema matches expected format
- Check for NULL values in required columns
- Validate polygon data format
- Review debug output for parsing errors
