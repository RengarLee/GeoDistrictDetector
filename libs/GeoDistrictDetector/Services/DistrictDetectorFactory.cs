using System.Collections.Generic;
using System.IO;
using GeoDistrictDetector.Models;

namespace GeoDistrictDetector.Services
{
    /// <summary>
    /// DistrictDetector factory, responsible for creating and initializing DistrictDetector instances
    /// </summary>
    public static class DistrictDetectorFactory
    {
        /// <summary>
        /// Create DistrictDetector from CSV file
        /// </summary>
        public static DistrictDetector CreateFromCsv(string csvPath)
        {

            var districts = DistrictLoader.LoadFromCsv(csvPath);
            return CreateFromDistricts(districts);
        }

        /// <summary>
        /// Create DistrictDetector from District list
        /// </summary>
        public static DistrictDetector CreateFromDistricts(List<District> districts)
        {
            var detector = new DistrictDetector();
            detector.LoadDistrictData(districts);
            return detector;
        }
    }
}
