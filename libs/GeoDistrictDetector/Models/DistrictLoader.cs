using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GeoDistrictDetector.Models;

namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// District loader class, responsible for loading District instances from CSV files or SQL Server.
    /// Provides unified data access interface.
    /// </summary>
    public static class DistrictLoader
    {
        /// <summary>
        /// Asynchronously loads a list of Districts from a CSV file.
        /// </summary>
        /// <param name="filePath">CSV file path</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of Districts</returns>
        public static async Task<List<District>> LoadFromCsvAsync(string filePath, CancellationToken cancellationToken = default)
        {
            return await CsvLoader.ReadDistrictsAsync(filePath, cancellationToken);
        }

        /// <summary>
        /// Synchronous wrapper for LoadFromCsvAsync - provided for compatibility.
        /// </summary>
        /// <param name="filePath">CSV file path</param>
        /// <returns>List of Districts</returns>
        public static List<District> LoadFromCsv(string filePath)
        {
            return CsvLoader.ReadDistricts(filePath);
        }

        /// <summary>
        /// Asynchronously loads a list of Districts from a SQL Server table.
        /// </summary>
        /// <param name="connectionString">SQL Server connection string</param>
        /// <param name="tableName">Table name (default: "Districts")</param>
        /// <param name="batchSize">Number of records per batch for internal paging (default: 1000)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of Districts</returns>
        public static async Task<List<District>> LoadFromSqlServerAsync(string connectionString, string tableName = "Districts", int batchSize = 1000, CancellationToken cancellationToken = default)
        {
            return await SqlServerLoader.ReadDistrictsAsync(connectionString, tableName, batchSize, cancellationToken);
        }

        /// <summary>
        /// Synchronous wrapper for LoadFromSqlServerAsync - provided for compatibility.
        /// </summary>
        /// <param name="connectionString">SQL Server connection string</param>
        /// <param name="tableName">Table name (default: "Districts")</param>
        /// <param name="batchSize">Number of records per batch for internal paging (default: 1000)</param>
        /// <returns>List of Districts</returns>
        public static List<District> LoadFromSqlServer(string connectionString, string tableName = "Districts", int batchSize = 1000)
        {
            return SqlServerLoader.ReadDistricts(connectionString, tableName, batchSize);
        }
    }
}
