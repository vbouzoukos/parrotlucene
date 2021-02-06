using System;
using System.Collections.Generic;
using System.Text;

namespace ParrotLucene.Base
{
    /// <summary>
    /// Coordinate structure used for spatial queries
    /// </summary>
    public class GeoCoordinate
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public GeoCoordinate() { }
        public GeoCoordinate(double lat, double lon)
        { 
            Latitude = lat;
            Longitude = lon;
        }
    }
}
