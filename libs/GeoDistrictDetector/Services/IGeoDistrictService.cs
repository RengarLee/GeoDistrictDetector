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
        /// 获取所有已加载的城市数据
        /// </summary>
        /// <returns>城市列表</returns>
        List<District> GetAllDistricts();
    }
}
