using System.Collections.Generic;
using NetTopologySuite.Geometries;


namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// 行政区域对象，对应 sample-distances.csv 的每一行。
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
        /// 区域ID，与 ok_data_level*.csv 表中的 ID 相同，通过此 ID 关联到省市区具体数据。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 上级ID。
        /// </summary>
        public int Pid { get; set; }

        /// <summary>
        /// 层级深度。
        /// </summary>
        public DistrictLevel Deep { get; set; }

        /// <summary>
        /// 区域名称，如：罗湖区，城市完整名称。
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 省市区三级完整名称，中间用空格分隔，如：广东省 深圳市 罗湖区。
        /// </summary>
        public string? ExtPath { get; set; }

        /// <summary>
        /// 城市中心坐标，高德地图GCJ-02火星坐标系。格式：Coordinate 类型。
        /// </summary>
        public Coordinate Geo { get; set; }

        /// <summary>
        /// 行政区域边界，空间数据类型。建议用 NetTopologySuite.Geometries.Geometry 存储。
        /// </summary>
        public Geometry Polygon { get; set; } = new GeometryCollection(null);
    }
}
