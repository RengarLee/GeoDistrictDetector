# GeoLocationCityDetector

A .NET command-line tool for determining city information based on geographic coordinates.

## Features

- 🗺️ **Coordinate City Query**: Find city information based on latitude and longitude coordinates
- 📁 **Multi-format Support**: Supports CSV and JSON format city data files
- 🎯 **Precise Algorithm**: Uses Haversine formula for distance calculation, supports polygon boundary detection
- 💻 **CLI Friendly**: Supports command-line parameters and interactive mode
- ⚡ **High Performance**: Asynchronous processing, supports large city datasets

## Quick Start

### Build Project

```bash
dotnet build
```

### Run Project

#### 1. Interactive Mode
```bash
dotnet run
```

#### 2. Command Line Mode
```bash
dotnet run sample-cities.csv 39.9042 116.4074
```

## Usage

### Data File Format

#### CSV Format
```csv
Province,City,District,Longitude,Latitude
Beijing,Beijing,Dongcheng District,116.4074,39.9042
Shanghai,Shanghai,Huangpu District,121.4692,31.2301
```

#### JSON Format
```json
[
  {
    "Province": "Beijing",
    "City": "Beijing",
    "District": "Dongcheng District",
    "CenterPoint": {
      "Latitude": 39.9042,
      "Longitude": 116.4074
    },
    "Boundary": []
  }
]
```

### Coordinate Format

Supports the following coordinate input formats:
- `39.9042,116.4074`
- `39.9042, 116.4074`
- `39.9042 116.4074`
- `(39.9042, 116.4074)`

## Project Structure

```
GeoLocationCityDetector/
├── Models/
│   ├── GeoPoint.cs          # Geographic coordinate point model
│   └── CityInfo.cs          # City information model
├── Services/
│   ├── IGeoLocationService.cs   # Geographic location service interface
│   └── GeoLocationService.cs    # Geographic location service implementation
├── Utils/
│   └── InputValidator.cs    # Input validation utility
├── Program.cs               # Main program entry
└── sample-cities.csv        # Sample city data
```

## Core Algorithm

### 1. Distance Calculation
Uses Haversine formula to calculate spherical distance between two geographic coordinate points:

```csharp
double a = Math.Sin(deltaLatRad / 2) * Math.Sin(deltaLatRad / 2) +
           Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
           Math.Sin(deltaLonRad / 2) * Math.Sin(deltaLonRad / 2);
double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
return earthRadius * c;
```

### 2. Point in Polygon Detection
Uses ray casting algorithm to determine if a point is inside polygon boundaries (when boundary data is available).

## Example Usage

### Interactive Mode Example

```
=== Geographic Location City Detector ===

Please select an operation:
1. Load city data file
2. Query city by coordinates
3. Display loaded city count
4. Exit
Please enter option (1-4): 1

Please enter city data file path (supports CSV and JSON formats): sample-cities.csv
Successfully loaded 22 city data entries
City data loaded successfully!

Please select an operation:
1. Load city data file
2. Query city by coordinates
3. Display loaded city count
4. Exit
Please enter option (1-4): 2

Please enter coordinates (format: latitude,longitude, e.g.: 39.9042,116.4074): 39.9042,116.4074
Query result:
  Coordinates: (39.9042, 116.4074)
  City: Beijing - Beijing - Dongcheng District
  City center: (39.9042, 116.4074)
```

### Command Line Mode Example

```bash
PS> dotnet run sample-cities.csv 39.9042 116.4074
=== Geographic Location City Detector ===

Loading data file: sample-cities.csv
Successfully loaded 22 city data entries
Query coordinates: (39.9042, 116.4074)
Result: Beijing - Beijing - Dongcheng District
```

## Extended Features

### Adding Boundary Data Support
You can add boundary coordinate points to city data for more precise city boundary detection:

```json
{
  "Province": "Beijing",
  "City": "Beijing",
  "District": "Dongcheng District",
  "CenterPoint": {
    "Latitude": 39.9042,
    "Longitude": 116.4074
  },
  "Boundary": [
    {"Latitude": 39.9100, "Longitude": 116.4000},
    {"Latitude": 39.9100, "Longitude": 116.4100},
    {"Latitude": 39.9000, "Longitude": 116.4100},
    {"Latitude": 39.9000, "Longitude": 116.4000}
  ]
}
```

## Technology Stack

- .NET 8.0
- C# 12
- System.Text.Json (JSON processing)

## Contributing

Welcome to submit Issues and Pull Requests to improve this project!
