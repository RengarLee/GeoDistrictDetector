using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoDistrictDetector.Models;

namespace GeoDistrictDetector.Services
{
    /// <summary>
    /// 行政区定位服务接口
    /// </summary>
    public interface IDistrictDetector
    {
        /// <summary>
        /// 获取所有已加载的城市数据
        /// </summary>
        /// <returns>城市列表</returns>
        List<District> GetAllDistricts();

        /// <summary>
        /// 根据经纬度查找所属城市
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>匹配的城市District，如果没找到返回null</returns>
        District? FindCityByCoordinate(double longitude, double latitude);

        /// <summary>
        /// 根据经纬度查找 DistrictLevel=2 且点在 Polygon 内的 District
        /// </summary>
        District? FindDistrictLevel2ByPoint(double longitude, double latitude);

        /// <summary>
        /// 根据经纬度查找所属省份
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>匹配的省份District，如果没找到返回null</returns>
        District? FindProvinceByCoordinate(double longitude, double latitude);

        /// <summary>
        /// 根据经纬度查找所属县区
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>匹配的县区District，如果没找到返回null</returns>
        District? FindCountyByCoordinate(double longitude, double latitude);

        /// <summary>
        /// 根据经纬度查找完整的行政区划信息（省、市、县）
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>包含省市县信息的元组</returns>
        (District? Province, District? City, District? District) FindCompleteAddressByCoordinate(double longitude, double latitude);

        /// <summary>
        /// 加载District数据并构建空间索引
        /// </summary>
        /// <param name="districts">District数据列表</param>
        void LoadDistrictData(List<District> districts);

        /// <summary>
        /// 异步加载District数据并构建空间索引
        /// </summary>
        /// <param name="districts">District数据列表</param>
        Task LoadDistrictDataAsync(List<District> districts);
    }
}
