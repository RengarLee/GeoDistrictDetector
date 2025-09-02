# GeoDistrictDetector

A .NET solution for geographic region detection, coordinate conversion, and data import/export, focused on Chinese administrative divisions.

## Main Features

- Administrative region detection by coordinates
- Coordinate system conversion (WGS84, GCJ02, BD09)
- CSV and SQL Server data import/export
- Console tools and demos

## Quick Start

### 1. Load Data Quickly
Load administrative division data from CSV:

```csharp
using GeoDistrictDetector.Services;

var detector = DistrictDetectorFactory.CreateFromCsv("libs/GeoDistrictDetector/sample-cities.csv");
Console.WriteLine($"Loaded {detector.GetAllDistricts().Count} districts");
```

### 2. Query Region by Coordinates
Find the administrative region for a specific point:

```csharp
var (province, city, district) = detector.FindCompleteAddressByCoordinate(116.4, 39.9);
Console.WriteLine($"Province: {province?.Name}, City: {city?.Name}, District: {district?.Name}");
```

#### Query Province Only
```csharp
var province = detector.FindProvinceByCoordinate(116.4, 39.9);
Console.WriteLine($"Province: {province?.Name ?? "Not found"}");
```

#### Query City Only
```csharp
var city = detector.FindCityByCoordinate(116.4, 39.9);
Console.WriteLine($"City: {city?.Name ?? "Not found"}");
```

#### Query County/District Only
```csharp
var county = detector.FindCountyByCoordinate(116.4, 39.9);
Console.WriteLine($"County: {county?.Name ?? "Not found"}");
```

#### Return Value Structure
All query methods return `District` objects or tuples of `District` objects. Each `District` has the following properties:

- **Id**: Unique identifier (integer)
- **Name**: Administrative division name (string)
- **Pid**: Parent division ID (integer, null for provinces)
- **Deep**: Administrative level (Province = 1, City = 2, County = 3)
- **ExtPath**: Full hierarchical path (e.g., "Beijing/Chaoyang District")
- **Geo**: Center coordinate point (longitude latitude format)
- **Polygon**: Boundary polygon coordinates (WKT format)

Example of accessing individual properties:
```csharp
var province = detector.FindProvinceByCoordinate(116.4, 39.9);
if (province != null)
{
    Console.WriteLine($"ID: {province.Id}");
    Console.WriteLine($"Name: {province.Name}");
    Console.WriteLine($"Level: {province.Deep}");
    Console.WriteLine($"Path: {province.ExtPath}");
}
```

### 3. Batch Query Points
Process multiple coordinates in a loop:

```csharp
var points = new[] { (116.4, 39.9), (121.5, 31.2), (113.3, 23.1) };
foreach (var (lng, lat) in points)
{
    var (province, city, district) = detector.FindCompleteAddressByCoordinate(lng, lat);
    Console.WriteLine($"{lng},{lat} -> {province?.Name}/{city?.Name}/{district?.Name}");
}
```

For command-line usage, run the demo:

```bash
dotnet run --project samples/DistrictDetector.Console
```


## Project Structure
GeoDistrictDetector/
├── libs/
│   └── GeoDistrictDetector/          # Core library
├── benchmarks/
│   └── DistrictDetectorBenchmark/    # Performance benchmarks
├── samples/
│   ├── CoordinateConversion.Console/ # Coordinate conversion demo
│   ├── DistrictDetector.Console/     # District detection demo
│   └── SqlServerLoader.Console/      # SQL Server loading demo
├── tools/
│   ├── CoordinateCsvConverter.Console/ # CSV coordinate converter
│   ├── CsvToSqlServerImporter.Console/ # SQL Server importer
│   └── ConsoleProgressBar.cs         # Progress bar utility

## Requirements

- .NET 8.0 or later
- For SQL Server tools: a running SQL Server instance

## License

MIT