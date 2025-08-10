using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;

namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// District 工厂类，负责从 CSV 文件批量加载 District 实例
    /// </summary>
    public static class DistrictFactory
    {
        /// <summary>
        /// 从 ok_data_level*.csv 文件批量加载 District 列表
        /// </summary>
        /// <param name="filePath">csv 文件路径</param>
        /// <returns>District 列表</returns>
        public static List<District> LoadFromCsv(string filePath)
        {
            var districts = new List<District>();
            if (!File.Exists(filePath))
                return districts;

            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0)
                return districts;

            // 假设表头为：id,pid,deep,name,ext_path,geo,polygon
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = line.Split(',');
                if (parts.Length < 7) continue;
                try
                {
                    int id = int.TryParse(parts[0].Trim(), out int tid) ? tid : 0;
                    int pid = int.TryParse(parts[1].Trim(), out int tpid) ? tpid : 0;
                    DistrictLevel deep = int.TryParse(parts[2].Trim(), out int tdeep) ? (DistrictLevel)tdeep : DistrictLevel.Province;
                    string name = parts[3].Trim();
                    string extPath = parts[4].Trim();
                    string geo = parts[5].Trim();
                    string polygonStr = parts[6].Trim();
                    Geometry polygon = ParsePolygon(polygonStr);
                    var district = new District(id, pid, deep, name, extPath, geo, polygon);
                    districts.Add(district);
                }
                catch { /* 忽略单行解析错误 */ }
            }
            return districts;
        }

        /// <summary>
        /// 解析 polygon 字符串为 Geometry 对象
        /// </summary>
        private static Geometry ParsePolygon(string polygonStr)
        {
            if (string.IsNullOrEmpty(polygonStr) || polygonStr == "EMPTY")
                return GeometryFactory.Default.CreateGeometryCollection(null);
            try
            {
                var factory = GeometryFactory.Default;
                var polygons = new List<Polygon>();
                var blocks = polygonStr.Split(';');
                foreach (var block in blocks)
                {
                    var coords = block.Split(',');
                    var points = new List<Coordinate>();
                    foreach (var coord in coords)
                    {
                        var lngLat = coord.Trim().Split(' ');
                        if (lngLat.Length == 2 &&
                            double.TryParse(lngLat[0], out double lng) &&
                            double.TryParse(lngLat[1], out double lat))
                        {
                            points.Add(new Coordinate(lng, lat));
                        }
                    }
                    if (points.Count > 2)
                    {
                        // 闭合多边形
                        if (!points[0].Equals2D(points[points.Count - 1]))
                            points.Add(points[0]);
                        var linearRing = factory.CreateLinearRing(points.ToArray());
                        polygons.Add(factory.CreatePolygon(linearRing));
                    }
                }
                if (polygons.Count == 1)
                    return polygons[0];
                else if (polygons.Count > 1)
                    return factory.CreateMultiPolygon(polygons.ToArray());
            }
            catch { /* 解析失败则保持空集合 */ }
            return GeometryFactory.Default.CreateGeometryCollection(null);
        }
    }
}
