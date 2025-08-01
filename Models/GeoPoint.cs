namespace GeoLocationCityDetector.Models
{
    /// <summary>
    /// 表示地理坐标点
    /// </summary>
    public class GeoPoint
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public GeoPoint(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public override string ToString()
        {
            return $"({Latitude}, {Longitude})";
        }
    }
}
