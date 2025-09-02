# CoordinateCsvConverter.Console

This project converts point coordinates in a CSV file from one coordinate system to another, supporting WGS84, GCJ02, and BD09 systems. It also provides a comparison function to check the accuracy of conversions between two CSV files.

## Features

- Convert coordinates in bulk from a source system to a target system
- Supported systems: WGS84 (GPS), GCJ02 (Gaode/Mars), BD09 (Baidu)
- Compare two CSV files for coordinate differences with a specified tolerance
- Simple command-line usage and clear output

## Usage

### Convert Coordinates
```bash
dotnet run -- input.csv output.csv source target
```
- `input.csv`: Input CSV file with coordinates
- `output.csv`: Output CSV file with converted coordinates
- `source`: Source coordinate system (WGS84, GCJ02, BD09)
- `target`: Target coordinate system (WGS84, GCJ02, BD09)

### Compare Two CSV Files
```bash
dotnet run -- --compare left.csv right.csv leftSystem rightSystem tolerance
```
- `left.csv`, `right.csv`: CSV files to compare
- `leftSystem`, `rightSystem`: Coordinate systems for each file
- `tolerance`: Allowed difference in meters

### Example
```bash
dotnet run -- data/input.csv data/output.csv WGS84 GCJ02
```
```bash
dotnet run -- --compare left.csv right.csv WGS84 GCJ02 5
```

## Requirements
- .NET 8.0 or later
- Input CSV files with valid coordinate data

## Troubleshooting
- Ensure coordinate system names are correct (WGS84, GCJ02, BD09)
- Check input file format and column order
- For comparison, set a reasonable tolerance value

## License
MIT
