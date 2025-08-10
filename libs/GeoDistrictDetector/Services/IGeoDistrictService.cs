using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoDistrictDetector.Models;

namespace GeoDistrictDetector.Services
{
    /// <summary>
    /// 地理位置服务接口
    /// </summary>
    public interface IGeoDistrictService
    {
        /// <summary>
        /// 根据坐标点查找所属城市
        /// </summary>
        /// <param name="point">坐标点</param>
        /// <returns>城市信息，如果未找到返回null</returns>
        Task<District?> FindDistrictByPointAsync(GeoPoint point);

        /// <summary>
        /// 加载城市数据
        /// </summary>
        /// <param name="dataFilePath">数据文件路径</param>
        /// <returns></returns>
        Task LoadDistrictDataAsync(string dataFilePath);

        /// <summary>
        /// 获取所有已加载的城市数据
        /// </summary>
        /// <returns>城市列表</returns>
        List<District> GetAllDistricts();
    }
}
