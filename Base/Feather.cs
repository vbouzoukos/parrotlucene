using Lucene.Net.Documents;
using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Spatial.Vector;
using Lucene.Net.Store;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using Spatial4n.Core.Shapes.Impl;
using System;
using System.IO;

namespace ParrotLucene.Base
{
    abstract public class Feather
    {
        private FSDirectory directoryTemp = null;
        protected SpatialContext spatialContext = null;
        protected SpatialStrategy spatialStrategy = null;
        protected readonly Wings wings;
        public Feather(Wings wings,string indexName)
        {
            this.IndexName = indexName;
            this.wings = wings;
        }

        #region Lucene Directory Index
        public string IndexName { get; }
        /// <summary>
        /// Returns Index File Directory Path
        /// </summary>
        public string LuceneDir
        {
            get { return Path.Combine(wings.IndexPath, IndexName); }
        }
        /// <summary>
        /// Lucene Directory Object
        /// </summary>
        protected FSDirectory LuceneDirectory
        {
            get
            {
                //Create directory if it does not exists
                if (directoryTemp == null)
                    directoryTemp = FSDirectory.Open(new DirectoryInfo(LuceneDir));
                return directoryTemp;
            }
        }
        #endregion

        #region Spatial Data
        internal SpatialStrategy Strategy
        {
            get
            {
                return spatialStrategy;
            }
        }
        internal SpatialContext SpatialContext
        {
            get
            {
                return spatialContext;
            }
        }
        #endregion

        #region PointVector Spatial Strategy
        /// <summary>
        /// Sets Spatial Searching for Points
        /// </summary>
        /// <param name="locationField"></param>
        internal void SetPointVectorStrategy(string locationField)
        {
            spatialContext = SpatialContext.GEO;
            spatialStrategy = new PointVectorStrategy(spatialContext, locationField);
        }
        /// <summary>
        /// Adds the location to the index to be used for spatial queries
        /// </summary>
        /// <param name="doc">Document to add the location</param>
        /// <param name="phi">Latitude(φ)</param>
        /// <param name="lamda">Longitude(λ)</param>
        internal void AddLocation(Document doc, double phi, double lamda)
        {
            IPoint point = SpatialContext.MakePoint(phi, lamda);
            foreach (var field in spatialStrategy.CreateIndexableFields(point))
            {
                doc.Add(field);
            }
            doc.Add(new StringField(spatialStrategy.FieldName, FormattableString.Invariant($"POINT ({point.X} {point.Y})"), Field.Store.YES));
        }
        /// <summary>
        /// Returns distance of the document in km from the query target
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="args">SpatialArgs data</param>
        /// <returns></returns>
        internal double DocumentDistance(Document doc, SpatialArgs args)
        {
            var dt = doc.Get(spatialStrategy.FieldName);
            var docPoint = (Point)SpatialContext.ReadShapeFromWkt(dt);
            double docDistDEG = SpatialContext.CalcDistance(args.Shape.Center, docPoint);
            double docDistInKM = DistanceUtils.Degrees2Dist(docDistDEG, DistanceUtils.EARTH_EQUATORIAL_RADIUS_KM);
            return docDistInKM;
        }
        #endregion
    }
}
