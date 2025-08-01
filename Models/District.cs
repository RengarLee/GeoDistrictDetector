using System;

namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// 行政区域对象，对应 sample-cities.csv 的每一行。
    /// </summary>
    public class District
    {
        /// <summary>
        /// 区域ID，与 ok_data_level*.csv 表中的 ID 相同，通过此 ID 关联到省市区具体数据。
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 上级ID。
        /// </summary>
        public int Pid { get; set; }

        /// <summary>
        /// 层级深度；0：省，1：市，2：区。
        /// </summary>
        public int Deep { get; set; }

        /// <summary>
        /// 区域名称，如：罗湖区，城市完整名称。
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 省市区三级完整名称，中间用空格分隔，如：广东省 深圳市 罗湖区。
        /// </summary>
        public string? ExtPath { get; set; }

        /// <summary>
        /// 城市中心坐标，高德地图GCJ-02火星坐标系。格式："lng lat" 或 "EMPTY"。
        /// </summary>
        public string? Geo { get; set; }

        /// <summary>
        /// 行政区域边界，高德地图GCJ-02火星坐标系。格式："lng lat,...;lng lat,..." 或 "EMPTY"。
        /// </summary>
        public string? Polygon { get; set; }
    }
}
