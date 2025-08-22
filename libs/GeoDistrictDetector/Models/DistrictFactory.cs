using System.Collections.Generic;


namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// District factory class, responsible for loading District instances.
    /// </summary>
    public static class DistrictFactory
    {
        /// <summary>
        /// Loads a list of Districts from a CSV file.
        /// </summary>
        /// <param name="filePath">CSV file path</param>
        /// <returns>List of Districts</returns>
        public static List<District> LoadFromCsv(string filePath)
        {
            return CsvReader.ReadDistrictsFromCsv(filePath);
        }

    }
}
