using System;
using System.Reflection;
using Lucene.Net.Documents;
using ParrotLucene.IndexedDocument;
using ParrotLucene.Indexing.Metadata;
using ParrotLucene.Base;
using System.Linq;
using ParrotLucene.Indexing.Core;

namespace ParrotLucene.Indexing
{

    /// <summary>
    /// The main Lucene indexing ducuments class
    /// </summary>
    /// <typeparam name="T">The indexed entity we use to do the search</typeparam>
    public class Indexer<T> : Rooster<T> where T : ParrotDocument
    {
        #region Static
        private static string IndexNaming(string indexName)
        {
            //If annotation sets name we use this name
            LuceneIndexedEntity attranalysis = MetaFinder.TypeLuceneInfo<LuceneIndexedEntity>(typeof(T));
            if (attranalysis != null)
            {
                if (!string.IsNullOrWhiteSpace(attranalysis.Name))
                    indexName = attranalysis.Name.Trim();
            }
            return indexName;
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Default Constractor
        /// </summary>
        public Indexer(Wings wings) : base(wings, IndexNaming(typeof(T).Name))
        {
            Type leType = typeof(T);

            var properties = leType.GetProperties();
            var geoIndex = properties.Where(x => MetaFinder.PropertyLuceneInfo<LuceneGeoIndex>(x) != null).FirstOrDefault();

            //we use spatial location strategy for points
            if (geoIndex != null)
            {
                this.SetPointVectorStrategy($"pvs_{geoIndex.Name}");
            }

        }
        #endregion
        #region Override
        /// <summary>
        /// Indexing the entity
        /// </summary>
        /// <param name="doc">Lucene document</param>
        /// <param name="lce">Entity to index(annotations are used for indexing)</param>
        internal override void Indexing(Document doc, T lce)
        {
            Type leType = lce.GetType();

            var properties = leType.GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                LuceneAnalysis attranalysis = MetaFinder.PropertyLuceneInfo<LuceneAnalysis>(pi);
                LuceneField fieldanalysis = MetaFinder.PropertyLuceneInfo<LuceneField>(pi);
                LuceneNoIndex noindex = MetaFinder.PropertyLuceneInfo<LuceneNoIndex>(pi);
                LuceneGeoIndex geoIndex = MetaFinder.PropertyLuceneInfo<LuceneGeoIndex>(pi);
                object pov = pi.GetValue(lce);
                ParrotConvertor value = lce.LuceneConvert(pov);
                if (noindex == null)
                {
                    string fieldName = (attranalysis != null) ? attranalysis.Name : (fieldanalysis != null) ? fieldanalysis.Name : pi.Name;
                    if (value.IsSpatial)
                    {
                        //set strategy for only one field
                        if (geoIndex != null)
                        {
                            AddLocation(doc, value.Phi.Value, value.Lamda.Value);
                        }
                        doc.Add(new StringField(fieldName, $"{value.Phi};{value.Lamda}", Field.Store.YES));
                    }
                    else
                    {
                        if (attranalysis != null)
                        {
                            StoreFieldData(doc, attranalysis.Name, pi.PropertyType, pov, value.Conversion);
                            doc.Add(new TextField(attranalysis.AnalysisName, value.Conversion, Field.Store.YES));
                        }
                        else
                        {
                            StoreFieldData(doc, fieldName, pi.PropertyType, pov, value.Conversion);
                        }
                    }
                }
            }
        }
        private void StoreFieldData(Document doc, string fieldName, Type attrType, object value, string converted)
        {
            if (attrType == typeof(int) || attrType == typeof(byte) || attrType == typeof(short) || attrType == typeof(uint) || attrType == typeof(ushort))
            {
                doc.Add(new Int32Field(fieldName, (int)value, Field.Store.YES));
            }
            else if (attrType == typeof(long) || attrType == typeof(ulong))
            {
                doc.Add(new Int64Field(fieldName, (long)value, Field.Store.YES));
            }
            else if (attrType == typeof(float) || attrType == typeof(double))
            {
                doc.Add(new DoubleField(fieldName, (double)value, Field.Store.YES));
            }
            else if (attrType == typeof(decimal))
            {
                doc.Add(new DoubleField(fieldName, Convert.ToDouble(value), Field.Store.YES));
            }
            else
            {//decimal not supported on range query
                doc.Add(new StringField(fieldName, converted, Field.Store.YES));
            }
        }
        #endregion
    }

}

