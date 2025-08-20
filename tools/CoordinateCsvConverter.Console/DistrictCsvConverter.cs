using GeoDistrictDetector;
using System;
using System.Collections.Generic;
using System.Globalization;
using CoordinateCsvConverter;

public class DistrictCsvConverter
{
    /// <summary>
    /// Process a parsed CSV row (List of fields) as a District row and return the modified row.
    /// Expects columns: id,pid,deep,name,ext_path,geo,polygon
    /// </summary>
    public List<string> ProcessRow(List<string> row, CoordinateSystem source, CoordinateSystem target)
    {
        if (row == null) return new List<string>();

        while (row.Count < 7) row.Add(string.Empty);

        var geoStr = row[5]?.Trim().Trim('"', '\\') ?? string.Empty;
        var polygonStr = row[6]?.Trim().Trim('"', '\\') ?? string.Empty;

        if (!string.IsNullOrEmpty(geoStr))
        {
            var converted = TryConvertCoordinateString(geoStr, source, target);
            if (converted != null) row[5] = converted;
        }

        if (!string.IsNullOrEmpty(polygonStr) && !string.Equals(polygonStr, "EMPTY", StringComparison.OrdinalIgnoreCase))
        {
            var convertedPoly = TryConvertPolygonString(polygonStr, source, target);
            if (convertedPoly != null) row[6] = convertedPoly;
        }

        return row;
    }

    public static void ConvertCsv(string inputPath, string outputPath, CoordinateSystem sourceSystem, CoordinateSystem targetSystem)
    {
        if (!File.Exists(inputPath))
            throw new FileNotFoundException("Input file does not exist", inputPath);

        // First pass: count total lines
        Console.WriteLine("Counting total lines...");
        int totalLines = 0;
        using (var counter = new StreamReader(inputPath))
        {
            while (counter.ReadLine() != null) totalLines++;
        }
        
        if (totalLines == 0)
            throw new InvalidDataException("Input CSV is empty");

        Console.WriteLine($"Processing {totalLines} lines...");

        using var reader = new StreamReader(inputPath);
        using var writer = new StreamWriter(outputPath, false);
        
        // Expect header row present
        var headerLine = reader.ReadLine();
        if (headerLine == null)
            throw new InvalidDataException("Input CSV is empty or missing header row");

        var headers = ParseCsvLine(headerLine);
        writer.WriteLine(JoinCsvHeader(headers)); // Use header-specific method

        // Process remaining rows as District rows
        var converter = new DistrictCsvConverter();
        int processedLines = 1; // Already processed header
        int lastPercent = -1;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) 
            { 
                writer.WriteLine(); 
                processedLines++;
                continue; 
            }
            
            var row = ParseCsvLine(line);
            var outRow = converter.ProcessRow(row, sourceSystem, targetSystem);
            writer.WriteLine(JoinCsv(outRow)); // Use data-specific method
            
            processedLines++;
            
            // Update progress bar
            int currentPercent = (int)((double)processedLines / totalLines * 100);
            if (currentPercent != lastPercent)
            {
                ConsoleProgressBar.Show(currentPercent, "Converting");
                lastPercent = currentPercent;
            }
        }
        
        ConsoleProgressBar.Complete("Conversion");
        Console.WriteLine($"Output file: {outputPath}");
    }

    public static string? TryConvertCoordinateString(string geoStr, CoordinateSystem source, CoordinateSystem target)
    {
        // Handle EMPTY case
        if (string.IsNullOrWhiteSpace(geoStr) || string.Equals(geoStr, "EMPTY", StringComparison.OrdinalIgnoreCase))
        {
            return "EMPTY";
        }

        var parts = geoStr.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2) return null;
        if (!double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lng)) return null;
        if (!double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat)) return null;
        var (nlng, nlat) = CoordinateConverter.Convert(lng, lat, source, target);
        return nlng.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) + " " + nlat.ToString("G17", System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string? TryConvertPolygonString(string polygonStr, CoordinateSystem source, CoordinateSystem target)
    {
        // Handle EMPTY case
        if (string.IsNullOrWhiteSpace(polygonStr) || string.Equals(polygonStr, "EMPTY", StringComparison.OrdinalIgnoreCase))
        {
            return "EMPTY";
        }

        var blocks = polygonStr.Split(';');
        var outBlocks = new List<string>();
        foreach (var block in blocks)
        {
            var pts = block.Split(',', System.StringSplitOptions.RemoveEmptyEntries);
            var outPts = new List<string>();
            foreach (var pt in pts)
            {
                var coord = pt.Trim();
                var parts = coord.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2) { outPts.Add(coord); continue; }
                if (!double.TryParse(parts[0], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lng) ||
                    !double.TryParse(parts[1], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lat))
                {
                    outPts.Add(coord); continue;
                }
                var (nlng, nlat) = CoordinateConverter.Convert(lng, lat, source, target);
                outPts.Add(nlng.ToString("G17", System.Globalization.CultureInfo.InvariantCulture) + " " + nlat.ToString("G17", System.Globalization.CultureInfo.InvariantCulture));
            }
            outBlocks.Add(string.Join(',', outPts));
        }
        return string.Join(';', outBlocks);
    }

    public static List<string> ParseCsvLine(string line)
    {
        var result = new List<string>();
        if (line == null) return result;

        int i = 0; int len = line.Length;
        while (i < len)
        {
            if (line[i] == '"')
            {
                i++; // Skip opening quote
                var sb = new System.Text.StringBuilder();
                while (i < len)
                {
                    if (line[i] == '"')
                    {
                        if (i + 1 < len && line[i + 1] == '"')
                        {
                            sb.Append('"'); i += 2; // Escaped quote
                        }
                        else { i++; break; }
                    }
                    else { sb.Append(line[i]); i++; }
                }
                while (i < len && line[i] != ',') i++;
                if (i < len && line[i] == ',') i++;
                result.Add(sb.ToString());
            }
            else
            {
                var start = i;
                while (i < len && line[i] != ',') i++;
                var token = line.Substring(start, i - start).Trim();
                if (i < len && line[i] == ',') i++;
                result.Add(token);
            }
        }
        if (line.EndsWith(",")) result.Add(string.Empty);
        return result;
    }

    public static string JoinCsvHeader(IEnumerable<string> fields)
    {
        // Headers don't need quotes unless they contain special characters
        return string.Join(",", fields.Select(f => (f != null && (f.Contains(',') || f.Contains('"') || f.Contains('\n') || f.Contains('\r'))) ? "\"" + f.Replace("\"", "\"\"") + "\"" : f));
    }

    public static string JoinCsv(IEnumerable<string> fields)
    {
        return string.Join(",", fields.Select(f => NeedsQuoting(f) ? "\"" + f.Replace("\"", "\"\"") + "\"" : f));
    }

    private static bool NeedsQuoting(string s)
    {
        if (s == null) return false;
        // Always quote non-numeric fields that might contain text
        return s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r') || 
               s.StartsWith(" ") || s.EndsWith(" ") || 
               (!string.IsNullOrEmpty(s) && !IsNumericField(s));
    }

    private static bool IsNumericField(string s)
    {
        // Check if field is purely numeric (id, pid, deep are typically numeric)
        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out _);
    }
}
