using ParrotLucene.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParrotLucene.Search
{
    /// <summary>
    /// Search area definition class used in spatial queries
    /// </summary>
    public class SearchArea
    {
        public GeoCoordinate Center { get; set; }
        public double Radius { get; set; }
        public SearchArea(double lat, double lon, double radius)
        {
            Center = new GeoCoordinate() { Latitude = lat, Longitude = lon };
            Radius = radius;
        }
    }
}
