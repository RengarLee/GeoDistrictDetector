namespace GeoLocationCityDetector.Models
{
    /// <summary>
    /// 城市信息
    /// </summary>
    public class CityInfo
    {
        public string Province { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public GeoPoint CenterPoint { get; set; }
        public List<GeoPoint> Boundary { get; set; } = new List<GeoPoint>();

        public CityInfo(string province, string city, string district, GeoPoint centerPoint)
        {
            Province = province;
            City = city;
            District = district;
            CenterPoint = centerPoint;
        }

        public override string ToString()
        {
            return $"{Province} - {City} - {District}";
        }
    }
}
