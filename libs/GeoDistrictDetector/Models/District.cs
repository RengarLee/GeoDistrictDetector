using System.Collections.Generic;
using NetTopologySuite.Geometries;


namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// Administrative district object, corresponding to each row in sample-distances.csv.
    /// </summary>
    public class District
    {
        public District(int id, int pid, DistrictLevel deep, string name, string extPath, Coordinate geo, Geometry polygon)
        {
            Id = id;
            Pid = pid;
            Deep = deep;
            Name = name;
            ExtPath = extPath;
            Geo = geo;
            Polygon = polygon;
        }

        public override string ToString()
        {
            return $"{Id},{Pid},{Deep},{Name},{ExtPath},{Geo},{Polygon?.GeometryType}";
        }

        /// <summary>
        /// District ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Parent ID.
        /// </summary>
        public int Pid { get; set; }

        /// <summary>
        /// Level depth.
        /// </summary>
        public DistrictLevel Deep { get; set; }

        /// <summary>
        /// District name, such as: Luohu District, full city name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Complete name of province, city, and district levels, separated by spaces, such as: Guangdong Province Shenzhen City Luohu District.
        /// </summary>
        public string? ExtPath { get; set; }

        /// <summary>
        /// City center coordinates, AMap GCJ-02 Mars coordinate system. Format: Coordinate type.
        /// </summary>
        public Coordinate Geo { get; set; }

        /// <summary>
        /// Administrative district boundary, spatial data type. Recommended to use NetTopologySuite.Geometries.Geometry for storage.
        /// </summary>
        public Geometry Polygon { get; set; } = new GeometryCollection(null);
    }
}
