using System;
using System.Collections.Generic;
using System.Text;

namespace ParrotLucene.Base
{
    /// <summary>
    /// Holds an object value to a string value available for storage
    /// </summary>
    public class ParrotConvertor
    {
        /// <summary>
        /// Text representation of the object value to be stored into the index
        /// </summary>
        public string Conversion { get; set; }
        /// <summary>
        /// Spatial latitude coordinate
        /// </summary>
        public double? Phi { get; set; }
        /// <summary>
        /// Spatial longitude coordinate
        /// </summary>
        public double? Lamda { get; set; }
        /// <summary>
        /// The convertor holds spatial data
        /// </summary>
        public bool IsSpatial { get { return Lamda.HasValue && Phi.HasValue; } }
    }
}
