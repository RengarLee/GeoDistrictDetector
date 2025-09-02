# CoordinateConversion.Console

This project demonstrates how to use coordinate system conversion between major Chinese coordinate systems: WGS84 (GPS), GCJ02 (Gaode/"Mars"), and BD09 (Baidu).

## Features

- Convert coordinates between WGS84, GCJ02, and BD09 systems
## Usage

### Run the Program
```bash
dotnet run
```
- Runs built-in conversion tests for Beijing city center coordinates
- Shows conversion results and error analysis

### Example Output
```
=== CoordinateConversion.Console: Coordinate Conversion Test ===
Original WGS84 coordinates: (116.3974, 39.9093)
Converted to GCJ02: (116.403406, 39.910677)
Converted to BD09: (116.409357, 39.916073)
GCJ02 converted back to WGS84: (116.397400, 39.909300)
Conversion error: Longitude 0.000000E+00, Latitude 0.000000E+00
```

## Supported Systems
- **WGS84**: GPS standard, used globally
- **GCJ02**: "Mars" coordinate system, used by Gaode (Amap), required for most Chinese map APIs
- **BD09**: Baidu coordinate system, used by Baidu Maps

## Customization
- You can modify `Program.cs` to test other coordinates or conversion directions
- The core conversion logic is in `libs/GeoDistrictDetector/Tools/CoordinateConverter.cs`

## Requirements
- .NET 8.0 or later

## License
MIT
