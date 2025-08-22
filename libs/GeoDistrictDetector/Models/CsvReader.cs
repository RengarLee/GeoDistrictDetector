using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace GeoDistrictDetector.Models
{
    /// <summary>
    /// CSV 数据读取和解析工具类
    /// </summary>
    public static class CsvReader
    {
        /// <summary>
        /// 读取 CSV 文件并批量解析为 District 列表
        /// </summary>
        public static List<District> ReadDistrictsFromCsv(string filePath)
        {
            var districts = new List<District>();
            if (!System.IO.File.Exists(filePath))
                return districts;

            var lines = System.IO.File.ReadAllLines(filePath);
            if (lines.Length == 0)
                return districts;

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                var district = ParseDistrict(line, i + 1);
                if (district != null)
                    districts.Add(district);
            }
            return districts;
        }

        /// <summary>
        /// 解析一行 CSV 字符串为 District 实例
        /// </summary>
        public static District ParseDistrict(string line, int lineNumber = 0)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;
            var parts = ParseCsvLine(line);
            if (parts.Count < 7) return null;
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
                return new District(id, pid, deep, name, extPath, geoCoord, polygon);
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CSV解析错误: 行号={lineNumber}, 内容={line}, 错误={ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析一行 CSV 字符串，支持引号包裹字段，保留引号
        /// </summary>
        public static List<string> ParseCsvLine(string line)
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
        /// 解析多段坐标字符串为 List<List<Coordinate>>
        /// </summary>
        public static List<List<Coordinate>> ParseCoordinates(string polygonStr)
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
        public static Geometry ParsePolygon(List<List<Coordinate>> multiCoords)
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
        /// 解析 'lng lat' 字符串为 Coordinate 类型
        /// </summary>
        public static Coordinate ParseCoordinate(string geoStr)
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
