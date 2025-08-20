using System;

/// <summary>
/// Console progress bar utility class
/// Provides unified progress display functionality with multiple display modes
/// </summary>
public static class ConsoleProgressBar
{
    /// <summary>
    /// Display progress bar based on percentage
    /// </summary>
    /// <param name="percent">Progress percentage (0-100)</param>
    /// <param name="operation">Operation description</param>
    /// <param name="barWidth">Progress bar width</param>
    public static void Show(int percent, string operation = "Processing", int barWidth = 40)
    {
        // Ensure percentage is within valid range
        percent = Math.Max(0, Math.Min(100, percent));
        
        int filledWidth = (int)((double)percent / 100 * barWidth);
        
        Console.Write($"\r{operation}: [");
        Console.Write(new string('█', filledWidth));
        Console.Write(new string('░', barWidth - filledWidth));
        Console.Write($"] {percent}%");
    }

    /// <summary>
    /// Display progress bar based on current count / total count
    /// </summary>
    /// <param name="current">Current processed count</param>
    /// <param name="total">Total count</param>
    /// <param name="operation">Operation description</param>
    /// <param name="barWidth">Progress bar width</param>
    public static void ShowWithCount(int current, int total, string operation = "Processing", int barWidth = 40)
    {
        if (total <= 0) return;
        
        int percent = (int)((double)current / total * 100);
        int filledWidth = (int)((double)current / total * barWidth);
        
        Console.Write($"\r{operation}: [");
        Console.Write(new string('█', filledWidth));
        Console.Write(new string('░', barWidth - filledWidth));
        Console.Write($"] {current}/{total} ({percent}%)");
    }

    /// <summary>
    /// Clear current progress bar line
    /// </summary>
    public static void Clear()
    {
        Console.Write('\r');
        Console.Write(new string(' ', Console.WindowWidth - 1));
        Console.Write('\r');
    }

    /// <summary>
    /// Display operation completion status
    /// </summary>
    /// <param name="operation">Operation description</param>
    public static void Complete(string operation = "Processing")
    {
        Console.WriteLine();
        Console.WriteLine($"{operation} completed!");
    }

    /// <summary>
    /// Display operation completion status with count information
    /// </summary>
    /// <param name="total">Total processed count</param>
    /// <param name="operation">Operation description</param>
    public static void CompleteWithCount(int total, string operation = "Processing")
    {
        Console.WriteLine();
        Console.WriteLine($"{operation} completed! Total processed: {total}");
    }
}