# CSV to SQL Server Importer

Fast bulk import tool for CSV district data to SQL Server.

## Quick Start

### Basic Import (Create Table + Append Data)
```bash
dotnet run "./data/data.csv" "Server=localhost;Database=GeoData;Trusted_Connection=true;"
```
✅ Auto-create table ❌ Don't delete existing data ➕ Append new data

### Clear Table Import (Create Table + Clear + Import)
```bash
dotnet run "./data/data.csv" "connectionstring" --clear-table
```
✅ Auto-create table ✅ Delete all existing data ➕ Import new data

### No Create Table (Table must exist)
```bash
dotnet run "./data/data.csv" "connectionstring" --no-create-table
```
❌ Don't create table ❌ Don't delete existing data ➕ Append new data

### Custom Settings
```bash
dotnet run "./data/data.csv" "connectionstring" --table=MyTable --batch-size=500
```
✅ Custom table name ✅ Custom batch size

## Features

- 🚀 **10-30x faster** than SQL INSERT statements
- 📊 **Real-time progress** bars for large datasets  
- 🔧 **Auto table creation** with optimized schema
- 🗑️ **Clear table option** for data replacement
- 💾 **Memory efficient** batch processing
- ✅ **Connection testing** before import

## Command Options

| Option | Description | Default |
|--------|-------------|---------|
| `--table=<name>` | Table name | `Districts` |
| `--clear-table` | Delete existing data first | `false` |
| `--no-create-table` | Skip table creation | `false` |
| `--batch-size=<size>` | Records per batch | `100` |

## CSV Format

Expected format (original CSV data is preserved as-is):
```csv
id,pid,deep,name,ext_path,geo,polygon
1,0,1,"Beijing","Beijing","116.4074 39.9042","116.4,39.9,116.5,39.8,..."
```

- **geo**: `"lng lat"` format (space-separated)
- **polygon**: `"lng1 lat1,lng2 lat2,..."` format

## Connection Examples

```bash
# Windows Authentication
"Server=localhost;Database=GeoData;Trusted_Connection=true;"

# SQL Authentication  
"Server=localhost;Database=GeoData;User Id=sa;Password=pass;"

# Remote with encryption (actual usage example)
"Data Source=192.168.9.62;Initial Catalog=GpsData;User ID=appuser;Password=TY5s2x9f4iK1lpYV;Encrypt=True;Trust Server Certificate=True;"
```

### Complete command example
```bash
# Using test data from project
dotnet run "./data/data.csv" "Data Source=192.168.9.62;Initial Catalog=GpsData;User ID=appuser;Password=TY5s2x9f4iK1lpYV;Encrypt=True;Trust Server Certificate=True;" --clear-table
```

## Table Schema

Auto-created table (if not exists):
```sql
CREATE TABLE Districts (
    Id INT PRIMARY KEY,
    Pid INT,
    Deep INT, 
    Name NVARCHAR(255),
    ExtPath NVARCHAR(500),
    Geo TEXT,        -- Original: "lng lat"
    Polygon TEXT     -- Original: "lng1 lat1,lng2 lat2,..."
);
```

## Requirements

- .NET 8.0
- SQL Server 
- CSV file in district format

