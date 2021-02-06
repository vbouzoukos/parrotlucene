using Lucene.Net.Search;
using Lucene.Net.Spatial.Queries;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParrotLucene.Search
{
    /// <summary>
    /// Query descriptor used to do search into the lucene index
    /// </summary>
    internal class QueryDesriptor
    {
        public BooleanQuery Query { get; set; }
        public SpatialArgs SpatialArguments { get; set; }
        public Filter Filter { get; set; }
        public bool InArea { get; set; }
    }
}
