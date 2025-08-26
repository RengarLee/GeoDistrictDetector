using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NetTopologySuite.Geometries;
using GeoDistrictDetector.Models;

namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// Utility class for loading District data from SQL Server.
    /// </summary>
    public static class SqlServerLoader
    {
        /// <summary>
        /// Asynchronously reads all Districts from a SQL Server table using automatic batching for optimal memory usage.
        /// </summary>
        /// <param name="connectionString">SQL Server connection string</param>
        /// <param name="tableName">Table name (default: "Districts")</param>
        /// <param name="batchSize">Number of records per batch for internal paging (default: 1000)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of all Districts from the table</returns>
        public static async Task<List<District>> ReadDistrictsAsync(string connectionString, string tableName = "Districts", int batchSize = 1000, CancellationToken cancellationToken = default)
        {
            var allDistricts = new List<District>();
            int offset = 0;
            int currentBatchCount;
            int totalErrors = 0;
            
            System.Diagnostics.Debug.WriteLine($"[SqlServerLoader] Starting to read all districts from table '{tableName}'");
            
            do
            {
                var (districts, errorCount) = await ReadDistrictsBatchAsync(connectionString, tableName, batchSize, offset, cancellationToken);
                currentBatchCount = districts.Count;
                allDistricts.AddRange(districts);
                totalErrors += errorCount;
                offset += batchSize;
                
                System.Diagnostics.Debug.WriteLine($"[SqlServerLoader] Batch completed: {currentBatchCount} districts loaded, {errorCount} errors, Total: {allDistricts.Count}");
            }
            while (currentBatchCount == batchSize); // Continue if we got a full batch
            
            System.Diagnostics.Debug.WriteLine($"[SqlServerLoader] Completed loading all districts: {allDistricts.Count} loaded, {totalErrors} total errors");
            return allDistricts;
        }

        /// <summary>
        /// Synchronous wrapper for ReadDistrictsAsync - provided for compatibility.
        /// </summary>
        /// <param name="connectionString">SQL Server connection string</param>
        /// <param name="tableName">Table name (default: "Districts")</param>
        /// <param name="batchSize">Number of records per batch for internal paging (default: 1000)</param>
        /// <returns>List of all Districts from the table</returns>
        public static List<District> ReadDistricts(string connectionString, string tableName = "Districts", int batchSize = 1000)
        {
            return ReadDistrictsAsync(connectionString, tableName, batchSize).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Asynchronously reads a single batch of Districts from a SQL Server table using OFFSET/FETCH for paging.
        /// </summary>
        /// <param name="connectionString">SQL Server connection string</param>
        /// <param name="tableName">Table name</param>
        /// <param name="batchSize">Number of records per batch</param>
        /// <param name="offset">Offset for paging</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Tuple of (Districts list, Error count)</returns>
        private static async Task<(List<District> districts, int errorCount)> ReadDistrictsBatchAsync(string connectionString, string tableName, int batchSize, int offset, CancellationToken cancellationToken)
        {
            var districts = new List<District>();
            int errorCount = 0;
            string query = $"SELECT Id, Pid, Deep, Name, ExtPath, Geo, Polygon FROM [{tableName}] ORDER BY Id OFFSET {offset} ROWS FETCH NEXT {batchSize} ROWS ONLY";
            
            using (var conn = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                await conn.OpenAsync(cancellationToken);
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    while (await reader.ReadAsync(cancellationToken))
                    {
                        try
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("Id"));
                            int pid = reader.GetInt32(reader.GetOrdinal("Pid"));
                            DistrictLevel deep = (DistrictLevel)reader.GetInt32(reader.GetOrdinal("Deep"));
                            string name = reader.GetString(reader.GetOrdinal("Name")).Trim('"', '\\');
                            string extPath = reader.GetString(reader.GetOrdinal("ExtPath")).Trim('"', '\\');
                            string geoStr = reader.GetString(reader.GetOrdinal("Geo")).Trim('"', '\\');
                            var geoCoord = DistrictParser.ParseCoordinate(geoStr);
                            string polygonStr = reader.GetString(reader.GetOrdinal("Polygon")).Trim('"', '\\');
                            var multiCoords = DistrictParser.ParseCoordinates(polygonStr);
                            var polygon = DistrictParser.ParsePolygon(multiCoords);
                            var district = new District(id, pid, deep, name, extPath, geoCoord, polygon);
                            districts.Add(district);
                        }
                        catch (System.Exception ex)
                        {
                            errorCount++;
                            System.Diagnostics.Debug.WriteLine($"[SqlServerLoader] SQL parse error: {ex.Message}");
                        }
                    }
                }
            }
            return (districts, errorCount);
        }
    }
}
