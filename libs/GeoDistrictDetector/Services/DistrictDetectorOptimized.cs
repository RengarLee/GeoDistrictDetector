using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoDistrictDetector.Models;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;

namespace GeoDistrictDetector.Services
{
        public class DistrictDetectorOptimized : IDistrictDetector
        {
            private List<District> _districts = new List<District>();
            private STRtree<District> _provinceIndex = new STRtree<District>();
            private Dictionary<int, STRtree<District>> _provinceCityTrees = new Dictionary<int, STRtree<District>>(); // 省Id->城市RTree
            private Dictionary<int, STRtree<District>> _cityCountyTrees = new Dictionary<int, STRtree<District>>();   // 城市Id->县区RTree
            private Dictionary<int, IPreparedGeometry> _preparedGeometries = new Dictionary<int, IPreparedGeometry>();

            /// <summary>
            /// 加载District数据并构建空间索引
            /// </summary>
            public void LoadDistrictData(List<District> districts)
            {
                _districts = districts ?? new List<District>();
                BuildSpatialIndex();
            }

            /// <summary>
            /// 异步加载District数据并构建空间索引
            /// </summary>
            public async Task LoadDistrictDataAsync(List<District> districts)
            {
                await Task.Run(() => LoadDistrictData(districts));
            }

            private void BuildSpatialIndex()
            {
                _provinceIndex = new STRtree<District>();
                _provinceCityTrees = new Dictionary<int, STRtree<District>>();
                _cityCountyTrees = new Dictionary<int, STRtree<District>>();
                _preparedGeometries = new Dictionary<int, IPreparedGeometry>();

                // 1. 省份空间索引
                foreach (var province in _districts.Where(d => d.Polygon != null && !d.Polygon.IsEmpty && d.Deep == DistrictLevel.Province))
                {
                    _provinceIndex.Insert(province.Polygon.EnvelopeInternal, province);
                    _preparedGeometries[province.Id] = PreparedGeometryFactory.Prepare(province.Polygon);
                }

                // 2. 每个省份构建自己的城市RTree
                var cities = _districts.Where(d => d.Polygon != null && !d.Polygon.IsEmpty && d.Deep == DistrictLevel.City);
                var provinceGroups = cities.GroupBy(c => c.Pid);
                foreach (var group in provinceGroups)
                {
                    var cityTree = new STRtree<District>();
                    foreach (var city in group)
                    {
                        cityTree.Insert(city.Polygon.EnvelopeInternal, city);
                        _preparedGeometries[city.Id] = PreparedGeometryFactory.Prepare(city.Polygon);
                    }
                    _provinceCityTrees[group.Key] = cityTree;
                }

                // 3. 每个城市构建自己的县区RTree（county）
                var counties = _districts.Where(d => d.Polygon != null && !d.Polygon.IsEmpty && d.Deep == DistrictLevel.County);
                var cityGroups = counties.GroupBy(c => c.Pid);
                foreach (var group in cityGroups)
                {
                    var countyTree = new STRtree<District>();
                    foreach (var county in group)
                    {
                        countyTree.Insert(county.Polygon.EnvelopeInternal, county);
                        _preparedGeometries[county.Id] = PreparedGeometryFactory.Prepare(county.Polygon);
                    }
                    _cityCountyTrees[group.Key] = countyTree;
                }
            }


            /// <summary>
            /// 根据经纬度查找可能的省份Id列表
            /// </summary>
            public List<int> FindProvinceIdsByCoordinate(double longitude, double latitude)
            {
                var point = new NetTopologySuite.Geometries.Point(longitude, latitude);
                var provinceCandidates = _provinceIndex.Query(point.EnvelopeInternal);
                return provinceCandidates.Where(d => _preparedGeometries[d.Id].Covers(point)).Select(d => d.Id).ToList();
            }

            /// <summary>
            /// 根据经纬度和省Id查找可能的城市Id列表
            /// </summary>
            public List<int> FindCityIdsByCoordinate(double longitude, double latitude, int provinceId)
            {
                if (!_provinceCityTrees.TryGetValue(provinceId, out var cityTree)) return new List<int>();
                var point = new NetTopologySuite.Geometries.Point(longitude, latitude);
                var cityCandidates = cityTree.Query(point.EnvelopeInternal);
                return cityCandidates.Where(d => _preparedGeometries[d.Id].Covers(point)).Select(d => d.Id).ToList();
            }

            /// <summary>
            /// 根据经纬度和城市Id查找可能的县区Id列表
            /// </summary>
            public List<int> FindCountyIdsByCoordinate(double longitude, double latitude, int cityId)
            {
                if (!_cityCountyTrees.TryGetValue(cityId, out var countyTree)) return new List<int>();
                var point = new NetTopologySuite.Geometries.Point(longitude, latitude);
                var countyCandidates = countyTree.Query(point.EnvelopeInternal);
                return countyCandidates.Where(d => _preparedGeometries[d.Id].Covers(point)).Select(d => d.Id).ToList();
            }

            /// <summary>
            /// 获取所有区划数据
            /// </summary>
            public List<District> GetAllDistricts()
            {
                return _districts.ToList();
            }

            /// <summary>
            /// 验证经纬度参数
            /// </summary>
            private void ValidateCoordinates(double longitude, double latitude)
            {
                if (_districts.Count == 0)
                    throw new InvalidOperationException("区域数据未加载，请先调用 LoadDistrictDataAsync 方法");
                if (longitude < -180 || longitude > 180 || latitude < -90 || latitude > 90)
                    throw new ArgumentException("经纬度参数超出有效范围");
            }

            /// <summary>
            /// 在候选区域中查找第一个包含该点的District
            /// </summary>
            private District? FindFirstMatchingDistrict(IEnumerable<District> candidates, Point point)
            {
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
            /// 根据经纬度查找所属省份
            /// </summary>
            public District? FindProvinceByCoordinate(double longitude, double latitude)
            {
                ValidateCoordinates(longitude, latitude);
                var point = new Point(longitude, latitude);
                
                var provinceCandidates = _provinceIndex.Query(point.EnvelopeInternal);
                return FindFirstMatchingDistrict(provinceCandidates, point);
            }

            /// <summary>
            /// 根据经纬度查找所属城市
            /// </summary>
            public District? FindCityByCoordinate(double longitude, double latitude)
            {
                ValidateCoordinates(longitude, latitude);
                var point = new NetTopologySuite.Geometries.Point(longitude, latitude);
                
                var provinceCandidates = _provinceIndex.Query(point.EnvelopeInternal);
                
                foreach (var province in provinceCandidates)
                {
                    if (_provinceCityTrees.TryGetValue(province.Id, out var cityTree))
                    {
                        var cityCandidates = cityTree.Query(point.EnvelopeInternal);
                        var result = FindFirstMatchingDistrict(cityCandidates, point);
                        if (result != null) return result;
                    }
                }
                
                return null;
            }

            /// <summary>
            /// 根据经纬度查找所属县区
            /// </summary>
            public District? FindCountyByCoordinate(double longitude, double latitude)
            {
                ValidateCoordinates(longitude, latitude);
                var point = new Point(longitude, latitude);
                
                var provinceCandidates = _provinceIndex.Query(point.EnvelopeInternal);
                
                foreach (var province in provinceCandidates)
                {
                    if (_provinceCityTrees.TryGetValue(province.Id, out var cityTree))
                    {
                        var cityCandidates = cityTree.Query(point.EnvelopeInternal);
                        
                        foreach (var city in cityCandidates)
                        {
                            if (_cityCountyTrees.TryGetValue(city.Id, out var countyTree))
                            {
                                var countyCandidates = countyTree.Query(point.EnvelopeInternal);
                                var result = FindFirstMatchingDistrict(countyCandidates, point);
                                if (result != null) return result;
                            }
                        }
                    }
                }
                
                return null;
            }

            /// <summary>
            /// 根据经纬度查找 DistrictLevel=2 且点在 Polygon 内的 District
            /// </summary>
            public District? FindDistrictLevel2ByPoint(double longitude, double latitude)
            {
                return FindCountyByCoordinate(longitude, latitude);
            }

            /// <summary>
            /// 根据经纬度查找完整的行政区划信息（省、市、县）
            /// </summary>
            /// <param name="longitude">经度</param>
            /// <param name="latitude">纬度</param>
            /// <returns>包含省市县信息的元组</returns>
            public (District? Province, District? City, District? District) FindCompleteAddressByCoordinate(double longitude, double latitude)
            {
                var province = FindProvinceByCoordinate(longitude, latitude);
                var city = FindCityByCoordinate(longitude, latitude);
                var district = FindCountyByCoordinate(longitude, latitude);
                
                return (province, city, district);
            }
        }
}
