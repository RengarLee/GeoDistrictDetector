using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NetTopologySuite.Geometries;

namespace CoordinateCsvConverter
{
    /// <summary>
    /// Simplified geographic region data class, contains id, geo coordinate, and polygon coordinates.
    /// </summary>
    public class DistrictData
    {
        /// <summary>
        /// Region ID identifier
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Center coordinate point (longitude, latitude), null if EMPTY
        /// </summary>
        public Coordinate? Geo { get; set; } = new Coordinate();

        /// <summary>
        /// Polygon boundary coordinate list
        /// </summary>
        public List<Coordinate> Polygon { get; set; } = new List<Coordinate>();

        public DistrictData() { }

        public DistrictData(string id, Coordinate? geo, List<Coordinate> polygon)
        {
            Id = id;
            Polygon = polygon ?? new List<Coordinate>();
        }

        public override string ToString()
        {
            var geoString = Geo != null ? $"({Geo.X:F6},{Geo.Y:F6})" : "EMPTY";
            return $"ID:{Id}, Geo:{geoString}, Polygon:{Polygon.Count} points";
        }

        /// <summary>
        /// Load DistrictData list from CSV file
        /// CSV format compatible with DistrictFactory format: id,pid,deep,name,ext_path,geo,polygon
        /// - id: string identifier (corresponds to CSV column 1)
        /// - geo: "longitude latitude" format (corresponds to CSV column 6, space separated)
        /// - polygon: "lng1 lat1,lng2 lat2;..." format (corresponds to CSV column 7, DistrictFactory format)
        /// </summary>
        /// <param name="csvFilePath">CSV file path</param>
        /// <returns>DistrictData list</returns>
    public static List<DistrictData> LoadFromCsv(string csvFilePath)
        {
            var result = new List<DistrictData>();

            if (!File.Exists(csvFilePath))
            {
                throw new FileNotFoundException($"CSV file does not exist: {csvFilePath}");
            }

            var lines = File.ReadAllLines(csvFilePath);
            if (lines.Length == 0)
            {
                return result;
            }

            // Skip header row (assume first row is header)
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                try
                {
                    var data = ParseCsvLine(line);
                    if (data != null)
                    {
                        result.Add(data);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing CSV line {i + 1}: {ex.Message}");
                    Console.WriteLine($"Problematic line: {line}");
                }
            }

            return result;
        }

        /// <summary>
        /// <summary>
        /// Parse a single CSV row (compatible with DistrictFactory format)
        /// </summary>
        /// <param name="csvLine">CSV row content</param>
        /// <returns>Parsed DistrictData object, null if failed</returns>
        private static DistrictData? ParseCsvLine(string csvLine)
        {
            var fields = SplitCsvLine(csvLine);
            if (fields.Count < 7) // At least 7 fields required: id,pid,deep,name,ext_path,geo,polygon
            {
                throw new ArgumentException($"CSV row has less than 7 fields: {csvLine}");
            }

            // Parse ID (column 1)
            string id = fields[0].Trim().Trim('"');

            // Parse geo coordinate (column 6, index 5)
            var geo = ParseGeoCoordinate(fields[5].Trim().Trim('"', '\\'));

            // Parse polygon coordinate list (column 7, index 6)
            var polygon = ParsePolygonCoordinates(fields[6].Trim().Trim('"', '\\'));

            return new DistrictData(id, geo, polygon);
        }

        /// <summary>
        /// <summary>
        /// Parse geographic coordinate string (compatible with DistrictFactory format)
        /// Supported formats:
        /// - "longitude latitude" (DistrictFactory format, space separated)
        /// - "longitude,latitude" (comma separated)
        /// - "EMPTY" (returns null)
        /// </summary>
        /// <param name="geoString">Coordinate string</param>
        /// <returns>Coordinate object, or null if EMPTY</returns>
        private static Coordinate? ParseGeoCoordinate(string geoString)
        {
            if (string.IsNullOrWhiteSpace(geoString))
            {
                return new Coordinate(0, 0);
            }

            if (string.Equals(geoString, "EMPTY", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            // Try space-separated format first (DistrictFactory format)
            var parts = geoString.Split(' ');
            if (parts.Length == 2)
            {
                if (double.TryParse(parts[0].Trim(), out double longitude) &&
                    double.TryParse(parts[1].Trim(), out double latitude))
                {
                    return new Coordinate(longitude, latitude);
                }
            }

            // Fall back to comma-separated format
            parts = geoString.Split(',');
            if (parts.Length == 2)
            {
                if (double.TryParse(parts[0].Trim(), out double longitude) &&
                    double.TryParse(parts[1].Trim(), out double latitude))
                {
                    return new Coordinate(longitude, latitude);
                }
            }

            throw new ArgumentException($"Invalid geo coordinate format, should be 'longitude latitude' or 'longitude,latitude': {geoString}");
        }

        /// <summary>
        /// <summary>
        /// Parse polygon coordinate list string (DistrictFactory format)
        /// Format: semicolon separates blocks, comma separates coordinate pairs, space separates longitude/latitude
        /// Example: "lng1 lat1,lng2 lat2;lng3 lat3,lng4 lat4"
        /// </summary>
        /// <param name="polygonString">Polygon coordinate string</param>
        /// <returns>Coordinate list</returns>
        private static List<Coordinate> ParsePolygonCoordinates(string polygonString)
        {
            var coordinates = new List<Coordinate>();

            if (string.IsNullOrWhiteSpace(polygonString) || string.Equals(polygonString, "EMPTY", StringComparison.OrdinalIgnoreCase))
            {
                return coordinates;
            }

            // DistrictFactory format: semicolon separates multiple coordinate blocks
            var blocks = polygonString.Split(';');
            foreach (var block in blocks)
            {
                if (string.IsNullOrWhiteSpace(block))
                    continue;

                // Comma separates coordinate pairs
                var coordPairs = block.Split(',');
                foreach (var coordPair in coordPairs)
                {
                    var trimmedPair = coordPair.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedPair))
                        continue;

                    // Space separates longitude/latitude (DistrictFactory format)
                    var lngLat = trimmedPair.Split(' ');
                    if (lngLat.Length == 2 &&
                        double.TryParse(lngLat[0], out double lng) &&
                        double.TryParse(lngLat[1], out double lat))
                    {
                        coordinates.Add(new Coordinate(lng, lat));
                    }
                }
            }

            return coordinates;
        }

        /// <summary>
        /// <summary>
        /// Simple CSV row splitter, supports quoted fields
        /// </summary>
        /// <param name="csvLine">CSV row</param>
        /// <returns>Field list</returns>
        private static List<string> SplitCsvLine(string csvLine)
        {
            var fields = new List<string>();
            var currentField = new System.Text.StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csvLine.Length; i++)
            {
                char c = csvLine[i];

                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    fields.Add(currentField.ToString());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            fields.Add(currentField.ToString());
            return fields;
        }
    }
}
