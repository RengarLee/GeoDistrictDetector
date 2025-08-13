using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using GeoDistrictDetector.Models;
using System.Text.Json;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Geometries.Prepared;

namespace GeoDistrictDetector.Services
{
    /// <summary>
    /// 地理位置服务实现
    /// </summary>
    public class GeoDistrictService : IGeoDistrictService
    {
        private List<District> _districts = new List<District>();
        
        // 空间索引相关 - 按行政级别分别建索引
        private STRtree<District> _provinceIndex = new STRtree<District>();
        private STRtree<District> _cityIndex = new STRtree<District>();
        private STRtree<District> _districtIndex = new STRtree<District>();
        
        private Dictionary<int, IPreparedGeometry> _preparedGeometries = new Dictionary<int, IPreparedGeometry>();

        /// <summary>
        /// 根据经纬度查找 DistrictLevel=2 且点在 Polygon 内的 District
        /// </summary>
        public District? FindDistrictLevel2ByPoint(double longitude, double latitude)
        {
            if (_districts.Count == 0)
                throw new InvalidOperationException("区域数据未加载，请先调用 LoadDistrictDataAsync 方法");

            // 只筛选 DistrictLevel=2
            var candidates = _districts.Where(d => d.Deep == DistrictLevel.District && d.Polygon != null && !d.Polygon.IsEmpty);
            var point = new NetTopologySuite.Geometries.Point(longitude, latitude);
            foreach (var district in candidates)
            {
                if (district.Polygon.Covers(point))
                    return district;
            }
            return null;
        }

        /// <summary>
        /// 构建空间索引，提升查询性能
        /// </summary>
        private void BuildSpatialIndex()
        {
            _provinceIndex = new STRtree<District>();
            _cityIndex = new STRtree<District>();
            _districtIndex = new STRtree<District>();
            _preparedGeometries = new Dictionary<int, IPreparedGeometry>();
            
            // 按级别分别构建索引
            foreach (var district in _districts.Where(d => d.Polygon != null && !d.Polygon.IsEmpty))
            {
                // 根据行政级别添加到对应的索引
                switch (district.Deep)
                {
                    case DistrictLevel.Province:
                        _provinceIndex.Insert(district.Polygon.EnvelopeInternal, district);
                        break;
                    case DistrictLevel.City:
                        _cityIndex.Insert(district.Polygon.EnvelopeInternal, district);
                        break;
                    case DistrictLevel.District:
                        _districtIndex.Insert(district.Polygon.EnvelopeInternal, district);
                        break;
                }
                
                // 为所有级别创建PreparedGeometry
                _preparedGeometries[district.Id] = PreparedGeometryFactory.Prepare(district.Polygon);
            }
        }

        /// <summary>
        /// 根据经纬度查找所属城市
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>匹配的城市District，如果没找到返回null</returns>
        public District? FindCityByCoordinate(double longitude, double latitude)
        {
            if (_districts.Count == 0)
                throw new InvalidOperationException("区域数据未加载，请先调用 LoadDistrictDataAsync 方法");

            // 验证输入参数
            if (longitude < -180 || longitude > 180 || latitude < -90 || latitude > 90)
                throw new ArgumentException("经纬度参数超出有效范围");

            var point = new NetTopologySuite.Geometries.Point(longitude, latitude);
            
            // 使用STRtree快速筛选候选区域
            var candidates = _cityIndex.Query(point.EnvelopeInternal);
            
            // 使用PreparedGeometry进行精确的空间查询
            foreach (var city in candidates)
            {
                if (_preparedGeometries.ContainsKey(city.Id) && 
                    _preparedGeometries[city.Id].Covers(point))
                {
                    return city;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 根据经纬度查找所属省份
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>匹配的省份District，如果没找到返回null</returns>
        public District? FindProvinceByCoordinate(double longitude, double latitude)
        {
            return FindDistrictByCoordinateAndLevel(longitude, latitude, DistrictLevel.Province, _provinceIndex);
        }

        /// <summary>
        /// 根据经纬度查找所属县区
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>匹配的县区District，如果没找到返回null</returns>
        public District? FindDistrictByCoordinate(double longitude, double latitude)
        {
            return FindDistrictByCoordinateAndLevel(longitude, latitude, DistrictLevel.District, _districtIndex);
        }

        /// <summary>
        /// 根据经纬度和行政级别查找行政区域的通用方法
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <param name="level">行政级别</param>
        /// <param name="index">对应的空间索引</param>
        /// <returns>匹配的District，如果没找到返回null</returns>
        private District? FindDistrictByCoordinateAndLevel(double longitude, double latitude, DistrictLevel level, STRtree<District> index)
        {
            if (_districts.Count == 0)
                throw new InvalidOperationException("区域数据未加载，请先调用 LoadDistrictDataAsync 方法");

            // 验证输入参数
            if (longitude < -180 || longitude > 180 || latitude < -90 || latitude > 90)
                throw new ArgumentException("经纬度参数超出有效范围");

            var point = new NetTopologySuite.Geometries.Point(longitude, latitude);
            
            // 使用对应级别的STRtree快速筛选候选区域
            var candidates = index.Query(point.EnvelopeInternal);
            
            // 使用PreparedGeometry进行精确的空间查询
            foreach (var district in candidates)
            {
                if (_preparedGeometries.ContainsKey(district.Id) && 
                    _preparedGeometries[district.Id].Covers(point))
                {
                    return district;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 根据经纬度查找完整的行政区划信息（省、市、县）
        /// </summary>
        /// <param name="longitude">经度</param>
        /// <param name="latitude">纬度</param>
        /// <returns>包含省市县信息的对象</returns>
        public (District? Province, District? City, District? District) FindCompleteAddressByCoordinate(double longitude, double latitude)
        {
            var province = FindProvinceByCoordinate(longitude, latitude);
            var city = FindCityByCoordinate(longitude, latitude);
            var district = FindDistrictByCoordinate(longitude, latitude);
            
            return (province, city, district);
        }

        /// <summary>
        /// 加载District数据并构建空间索引
        /// </summary>
        /// <param name="districts">District数据列表</param>
        public void LoadDistrictData(List<District> districts)
        {
            _districts = districts ?? new List<District>();
            
            // 数据加载完成后构建空间索引
            BuildSpatialIndex();
        }

        /// <summary>
        /// 异步加载District数据并构建空间索引
        /// </summary>
        /// <param name="districts">District数据列表</param>
        public async Task LoadDistrictDataAsync(List<District> districts)
        {
            await Task.Run(() => LoadDistrictData(districts));
        }

        public List<District> GetAllDistricts()
        {
            return _districts.ToList();
        }
    }
}
