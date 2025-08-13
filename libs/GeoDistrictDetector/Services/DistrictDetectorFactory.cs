using System.Collections.Generic;
using System.IO;
using GeoDistrictDetector.Models;

namespace GeoDistrictDetector.Services
{
    /// <summary>
    /// DistrictDetector 工厂，负责创建和初始化 DistrictDetector 实例
    /// </summary>
    public static class DistrictDetectorFactory
    {
        /// <summary>
        /// 从 CSV 文件创建 DistrictDetector
        /// </summary>
        public static DistrictDetector CreateFromCsv(string csvPath)
        {

            var districts = DistrictFactory.LoadFromCsv(csvPath);
            return CreateFromDistricts(districts);
        }

        /// <summary>
        /// 从 District 列表创建 DistrictDetector
        /// </summary>
        public static DistrictDetector CreateFromDistricts(List<District> districts)
        {
            var detector = new DistrictDetector();
            detector.LoadDistrictData(districts);
            return detector;
        }
    }
}
