# CSV to SQL Server Importer

Fast bulk import tool for CSV district data to SQL Server.

## Quick Start

### Basic Import (创建表 + 追加数据)
```bash
dotnet run "./data/data.csv" "Server=localhost;Database=GeoData;Trusted_Connection=true;"
```
✅ 自动创建表 ❌ 不删除现有数据 ➕ 追加新数据

### Clear Table Import (创建表 + 清空 + 导入)
```bash
dotnet run "./data/data.csv" "connectionstring" --clear-table
```
✅ 自动创建表 ✅ 删除所有现有数据 ➕ 导入新数据

### No Create Table (表必须已存在)
```bash
dotnet run "./data/data.csv" "connectionstring" --no-create-table
```
❌ 不创建表 ❌ 不删除现有数据 ➕ 追加新数据

### Custom Settings
```bash
dotnet run "./data/data.csv" "connectionstring" --table=MyTable --batch-size=500
```
✅ 自定义表名 ✅ 自定义批次大小

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

# Remote with encryption (实际使用示例)
"Data Source=192.168.9.62;Initial Catalog=GpsData;User ID=appuser;Password=TY5s2x9f4iK1lpYV;Encrypt=True;Trust Server Certificate=True;"
```

### 完整命令示例
```bash
# 使用项目中的测试数据
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
- SQL Server 2012+ or Azure SQL
- CSV file in district format

