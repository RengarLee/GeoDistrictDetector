using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GeoDistrictDetector.Models;
using NetTopologySuite.Index.Strtree;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using GeoDistrictDetector;

namespace GeoDistrictDetector.Services
{
        public class DistrictDetector : IDistrictDetector
        {
            private List<District> _districts = new List<District>();
            private STRtree<District> _provinceIndex = new STRtree<District>();
            private Dictionary<int, STRtree<District>> _provinceCityTrees = new Dictionary<int, STRtree<District>>(); // ProvinceId->City RTree
            private Dictionary<int, STRtree<District>> _cityCountyTrees = new Dictionary<int, STRtree<District>>();   // CityId->County RTree
            private Dictionary<int, IPreparedGeometry> _preparedGeometries = new Dictionary<int, IPreparedGeometry>();

            /// <summary>
            /// Load District data and build spatial index
            /// </summary>
            public void LoadDistrictData(List<District> districts)
            {
                _districts = districts ?? new List<District>();
                BuildSpatialIndex();
            }

            /// <summary>
            /// Asynchronously load District data and build spatial index
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

                // 1. Province spatial index
                foreach (var province in _districts.Where(d => d.Polygon != null && !d.Polygon.IsEmpty && d.Deep == DistrictLevel.Province))
                {
                    _provinceIndex.Insert(province.Polygon.EnvelopeInternal, province);
                    _preparedGeometries[province.Id] = PreparedGeometryFactory.Prepare(province.Polygon);
                }

                // 2. Build city RTree for each province
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

                // 3. Build county RTree for each city
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
            /// Get all district data
            /// </summary>
            public List<District> GetAllDistricts()
            {
                return _districts.ToList();
            }

            /// <summary>
            /// Validate longitude and latitude parameters
            /// </summary>
            private void ValidateCoordinates(double longitude, double latitude)
            {
                if (_districts.Count == 0)
                    throw new InvalidOperationException("District data not loaded, please call LoadDistrictDataAsync method first");
                if (longitude < -180 || longitude > 180 || latitude < -90 || latitude > 90)
                    throw new ArgumentException("Longitude and latitude parameters are out of valid range");
            }

            /// <summary>
            /// Find the first District that contains the point from candidate regions
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
            /// Find the province by longitude and latitude coordinates
            /// </summary>
            public District? FindProvinceByCoordinate(double longitude, double latitude)
            {
                ValidateCoordinates(longitude, latitude);
                var point = new Point(longitude, latitude);
                
                var provinceCandidates = _provinceIndex.Query(point.EnvelopeInternal);
                return FindFirstMatchingDistrict(provinceCandidates, point);
            }

            /// <summary>
            /// Find the city by longitude and latitude coordinates
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
            /// Find the county by longitude and latitude coordinates
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
            /// Find complete administrative division information (province, city, county) by longitude and latitude coordinates
            /// </summary>
            /// <param name="longitude">Longitude</param>
            /// <param name="latitude">Latitude</param>
            /// <returns>Tuple containing province, city, and county information</returns>
            public (District? Province, District? City, District? District) FindCompleteAddressByCoordinate(double longitude, double latitude)
            {
                var province = FindProvinceByCoordinate(longitude, latitude);
                var city = FindCityByCoordinate(longitude, latitude);
                var district = FindCountyByCoordinate(longitude, latitude);
                
                return (province, city, district);
            }
        }
}
