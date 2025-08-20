using GeoDistrictDetector;
using CoordinateCsvConverter;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 0;
            }

            if (args[0].Equals("--compare", StringComparison.OrdinalIgnoreCase))
                return HandleCompare(args);

            return HandleConvert(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 2;
        }
    }

    static int HandleCompare(string[] args)
    {
        if (args.Length < 6)
        {
            Console.WriteLine("Usage: --compare <left.csv> <right.csv> <leftSystem> <rightSystem> <tolerance>");
            return 1;
        }
        var left = args[1];
        var right = args[2];
        var leftSys = ParseCoordinateSystem(args[3]);
        var rightSys = ParseCoordinateSystem(args[4]);
        if (!double.TryParse(args[5], out double tolerance))
        {
            Console.WriteLine("Parameter 'tolerance' must be a number.");
            return 1;
        }

        var geoComparator = new DistrictDataComparator(tolerance);
        var result = geoComparator.Compare(left, right, leftSys, rightSys);
        geoComparator.PrintComparisonReport(result, left, right);
        return result.FailedCount > 0 ? 1 : 0;
    }

    static int HandleConvert(string[] args)
    {
        if (args.Length < 4)
        {
            Console.WriteLine("Usage: dotnet run -- <input.csv> <output.csv> <source> <target>");
            return 1;
        }
        var input = args[0];
        var output = args[1];
        var source = ParseCoordinateSystem(args[2]);
        var target = ParseCoordinateSystem(args[3]);

        DistrictCsvConverter.ConvertCsv(input, output, source, target);
        Console.WriteLine($"Conversion completed: {output}");
        return 0;
    }

    static void PrintUsage()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run -- <input.csv> <output.csv> <source> <target>");
        Console.WriteLine("    Convert coordinates in <input.csv> from <source> system to <target> system and output to <output.csv>.");
        Console.WriteLine("    Supported coordinate systems: WGS84, GCJ02, BD09");
        Console.WriteLine();
        Console.WriteLine("  dotnet run -- --compare <left.csv> <right.csv> <leftSystem> <rightSystem> <tolerance>");
        Console.WriteLine("    Compare two CSV files (<left.csv> and <right.csv>) with specified coordinate systems and tolerance (meters).");
        Console.WriteLine("    All parameters are required.");
        Console.WriteLine();
        Console.WriteLine("Example:");
        Console.WriteLine("  dotnet run -- data/input.csv data/output.csv WGS84 GCJ02");
        Console.WriteLine("  dotnet run -- --compare left.csv right.csv WGS84 GCJ02 5");
    }

    static CoordinateSystem ParseCoordinateSystem(string s)
    {
        if (Enum.TryParse<CoordinateSystem>(s, true, out var cs))
            return cs;
        throw new ArgumentException($"Unsupported coordinate system: {s}");
    }
}
