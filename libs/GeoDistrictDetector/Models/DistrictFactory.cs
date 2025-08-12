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
        /// 从*.csv 文件批量加载 District 列表
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
                var parts = ParseCsvLine(line);
                if (parts.Count < 7) continue;
                try
                {
                    int id = int.TryParse(parts[0].Trim(), out int tid) ? tid : 0;
                    int pid = int.TryParse(parts[1].Trim(), out int tpid) ? tpid : 0;
                    DistrictLevel deep = int.TryParse(parts[2].Trim(), out int tdeep) ? (DistrictLevel)tdeep : DistrictLevel.Province;
                    string name = parts[3].Trim().Trim('"', '\\');
                    string extPath = parts[4].Trim().Trim('"', '\\');
                    string geoStr = parts[5].Trim().Trim('"', '\\');
                    Coordinate geoCoord = ParseCoordinate(geoStr);
                    string polygonStr = parts[6].Trim().Trim('"', '\\');
                    var multiCoords = ParseCoordinates(polygonStr);
                    Geometry polygon = ParsePolygon(multiCoords);
                    var district = new District(id, pid, deep, name, extPath, geoCoord, polygon);
                    districts.Add(district);
                }
                catch (System.Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[DistrictFactory] CSV解析错误: 行号={i + 1}, 内容={line}, 错误={ex.Message}");
                }
            }
            return districts;
        }

        /// <summary>
        /// 解析多段坐标字符串为 List<List<Coordinate>>
        /// </summary>
        private static List<List<Coordinate>> ParseCoordinates(string polygonStr)
        {
            var result = new List<List<Coordinate>>();
            if (string.IsNullOrEmpty(polygonStr) || polygonStr == "EMPTY")
                return result;
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
                if (points.Count > 0)
                    result.Add(points);
            }
            return result;
        }

        /// <summary>
        /// 解析多段坐标为 Geometry 对象
        /// </summary>
        private static Geometry ParsePolygon(List<List<Coordinate>> multiCoords)
        {
            var factory = GeometryFactory.Default;
            var polygons = new List<Polygon>();
            foreach (var points in multiCoords)
            {
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
            return factory.CreateGeometryCollection(null);
        }

        /// <summary>
        /// 简单的 CSV 行解析器，支持引号包裹字段，保留引号
        /// </summary>
        private static List<string> ParseCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var field = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                    field.Append(c); // 保留引号
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }
            result.Add(field.ToString());
            return result;
        }

        /// <summary>
        /// 解析 'lng lat' 字符串为 Coordinate 类型
        /// </summary>
        private static Coordinate ParseCoordinate(string geoStr)
        {
            var arr = geoStr.Split(' ');
            if (arr.Length == 2 &&
                double.TryParse(arr[0], out double lng) &&
                double.TryParse(arr[1], out double lat))
            {
                return new Coordinate(lng, lat);
            }
            return new Coordinate();
        }
    }
}
