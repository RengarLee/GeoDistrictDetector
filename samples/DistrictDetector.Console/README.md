
# DistrictDetector.Console

This project demonstrates how to query the administrative region (province, city, county) for a given geographic point.

## Features

- Loads administrative division data from CSV
- Supports lookup by longitude and latitude
- Returns complete address info (province, city, county) for each point
- Includes sample test points for quick validation
- Clear error handling and informative console output

## Usage

### Run the Program
```bash
dotnet run
```
- Loads data from `libs/GeoDistrictDetector/sample-cities.csv`
- Runs built-in test queries for several city centers

### How It Works
1. Loads district data from CSV using `DistrictDetectorFactory`.
2. Prints total number of loaded provinces, cities, and counties.
3. For each test coordinate, outputs the matching province, city, and county (or “Not found”).

### Example Output
```
=== DistrictDetector.Console: Complete Address Lookup Test ===
Attempting to load data from CSV: ../../libs/GeoDistrictDetector/sample-cities.csv
✓ Successfully loaded data from CSV file
	Total loaded 45678 administrative districts
	Provinces: 34, Cities: 1234, Counties: 44410

=== Complete Address Lookup Function Test ===

Test coordinates: Shenzhen City Center (114.0579, 22.5431)
	Province: Guangdong
	City: Shenzhen
	District: Futian

Test coordinates: Invalid Coordinates (0.0, 0.0)
	No matching administrative district found
```

## Customization
- To use your own data, replace the CSV file path in `Program.cs`.
- You can add more test points or integrate with other input sources.

## Requirements
- .NET 8.0 or later
- Valid administrative division CSV data

## Troubleshooting
- If startup fails, check the CSV file path and format.
- For missing results, verify coordinates and data coverage.

## License
MIT
