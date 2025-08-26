using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using GeoDistrictDetector.Models;

namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// CSV District data loader and parser utility class
    /// </summary>
    public static class CsvLoader
    {
        /// <summary>
        /// Asynchronously reads CSV file and parses it to District list
        /// </summary>
        /// <param name="filePath">CSV file path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of Districts</returns>
        public static async Task<List<District>> ReadDistrictsAsync(string filePath, CancellationToken cancellationToken = default)
        {
            var districts = new List<District>();
            if (!File.Exists(filePath))
                return districts;

            var lines = await File.ReadAllLinesAsync(filePath, cancellationToken);
            if (lines.Length == 0)
                return districts;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var district = ParseDistrict(line, i + 1);
                if (district != null)
                    districts.Add(district);
            }
            return districts;
        }

        /// <summary>
        /// Synchronous wrapper for ReadDistrictsFromCsvAsync - provided for compatibility.
        /// </summary>
        /// <param name="filePath">CSV file path</param>
        /// <returns>List of Districts</returns>
        public static List<District> ReadDistricts(string filePath)
        {
            return ReadDistrictsAsync(filePath).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Parse a CSV line to District instance
        /// </summary>
        public static District? ParseDistrict(string line, int lineNumber = 0)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;
            var parts = ParseCsvLine(line);
            if (parts.Count < 7) return null;
            try
            {
                int id = int.TryParse(parts[0].Trim(), out int tid) ? tid : 0;
                int pid = int.TryParse(parts[1].Trim(), out int tpid) ? tpid : 0;
                DistrictLevel deep = int.TryParse(parts[2].Trim(), out int tdeep) ? (DistrictLevel)tdeep : DistrictLevel.Province;
                string name = parts[3].Trim().Trim('"', '\\');
                string extPath = parts[4].Trim().Trim('"', '\\');
                string geoStr = parts[5].Trim().Trim('"', '\\');
                Coordinate geoCoord = DistrictParser.ParseCoordinate(geoStr);
                string polygonStr = parts[6].Trim().Trim('"', '\\');
                var multiCoords = DistrictParser.ParseCoordinates(polygonStr);
                Geometry polygon = DistrictParser.ParsePolygon(multiCoords);
                return new District(id, pid, deep, name, extPath, geoCoord, polygon);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CSV parse error: Line={lineNumber}, Content={line}, Error={ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse a CSV line, supporting quoted fields
        /// </summary>
        public static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var field = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    field.Append(c); // Preserve quotes
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
            result.Add(field.ToString());
            return result;
        }
    }
}
