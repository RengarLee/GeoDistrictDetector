using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GeoDistrictDetector.Models;

namespace GeoDistrictDetector.Services
{
    /// <summary>
    /// Administrative district location service interface
    /// </summary>
    public interface IDistrictDetector
    {
        /// <summary>
        /// Get all loaded city data
        /// </summary>
        /// <returns>City list</returns>
        List<District> GetAllDistricts();

        /// <summary>
        /// Find the city by longitude and latitude coordinates
        /// </summary>
        /// <param name="longitude">Longitude</param>
        /// <param name="latitude">Latitude</param>
        /// <returns>Matching city District, returns null if not found</returns>
        District? FindCityByCoordinate(double longitude, double latitude);

        /// <summary>
        /// Find the province by longitude and latitude coordinates
        /// </summary>
        /// <param name="longitude">Longitude</param>
        /// <param name="latitude">Latitude</param>
        /// <returns>Matching province District, returns null if not found</returns>
        District? FindProvinceByCoordinate(double longitude, double latitude);

        /// <summary>
        /// Find the county by longitude and latitude coordinates
        /// </summary>
        /// <param name="longitude">Longitude</param>
        /// <param name="latitude">Latitude</param>
        /// <returns>Matching county District, returns null if not found</returns>
        District? FindCountyByCoordinate(double longitude, double latitude);

        /// <summary>
        /// Find complete administrative division information (province, city, county) by longitude and latitude coordinates
        /// </summary>
        /// <param name="longitude">Longitude</param>
        /// <param name="latitude">Latitude</param>
        /// <returns>Tuple containing province, city, and county information</returns>
        (District? Province, District? City, District? District) FindCompleteAddressByCoordinate(double longitude, double latitude);

        /// <summary>
        /// Load District data and build spatial index
        /// </summary>
        /// <param name="districts">District data list</param>
        void LoadDistrictData(List<District> districts);

        /// <summary>
        /// Asynchronously load District data and build spatial index
        /// </summary>
        /// <param name="districts">District data list</param>
        Task LoadDistrictDataAsync(List<District> districts);
    }
}
