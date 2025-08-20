using System;

namespace GeoDistrictDetector.Tools
{
    /// <summary>
    /// 控制台进度条工具类
    /// 提供统一的进度显示功能，支持多种显示模式
    /// </summary>
    public static class ConsoleProgressBar
    {
        /// <summary>
        /// 显示进度条（基于百分比）
        /// </summary>
        /// <param name="percent">进度百分比 (0-100)</param>
        /// <param name="operation">操作描述</param>
        /// <param name="barWidth">进度条宽度</param>
        public static void Show(int percent, string operation = "Processing", int barWidth = 40)
        {
            // 确保百分比在有效范围内
            percent = Math.Max(0, Math.Min(100, percent));
            
            int filledWidth = (int)((double)percent / 100 * barWidth);
            
            Console.Write($"\r{operation}: [");
            Console.Write(new string('█', filledWidth));
            Console.Write(new string('░', barWidth - filledWidth));
            Console.Write($"] {percent}%");
        }

        /// <summary>
        /// 显示进度条（基于当前数量/总数量）
        /// </summary>
        /// <param name="current">当前处理数量</param>
        /// <param name="total">总数量</param>
        /// <param name="operation">操作描述</param>
        /// <param name="barWidth">进度条宽度</param>
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
        /// 清除当前进度条行
        /// </summary>
        public static void Clear()
        {
            Console.Write('\r');
            Console.Write(new string(' ', Console.WindowWidth - 1));
            Console.Write('\r');
        }

        /// <summary>
        /// 显示操作完成状态
        /// </summary>
        /// <param name="operation">操作描述</param>
        public static void Complete(string operation = "Processing")
        {
            Console.WriteLine();
            Console.WriteLine($"{operation} completed!");
        }

        /// <summary>
        /// 显示操作完成状态（带计数信息）
        /// </summary>
        /// <param name="total">总处理数量</param>
        /// <param name="operation">操作描述</param>
        public static void CompleteWithCount(int total, string operation = "Processing")
        {
            Console.WriteLine();
            Console.WriteLine($"{operation} completed! Total processed: {total}");
        }
    }
}
