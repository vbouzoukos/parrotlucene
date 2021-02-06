using System;
using System.Collections.Generic;
using System.Linq;

using System.Reflection;

using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Queries;

using Newtonsoft.Json;
using ParrotLucene.Search.Core;
using ParrotLucene.IndexedDocument;
using ParrotLucene.Indexing.Metadata;
using ParrotLucene.Utils;
using ParrotLucene.Error;
using ParrotLucene.Base;

namespace ParrotLucene.Search
{
    /// <summary>
    /// Main lucene index search class implementation of the ParrotEyes
    /// </summary>
    /// <typeparam name="T">The indexed entity we use to do the search</typeparam>
    public class IndexSearch<T> : ParrotEyes<T> where T : ParrotDocument
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
        public IndexSearch(Wings wings) : base(wings, IndexNaming(typeof(T).Name))
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

        #region ovverides
        /// <summary>
        /// Maps the document data into the entity
        /// </summary>
        /// <param name="doc">Lucene document</param>
        /// <param name="args">Spatial Arguments</param>
        /// <param name="score">Lucene score</param>
        /// <returns>The Mapped Entity T</returns>
        internal override T MapDocToData(Document doc, SpatialArgs args = null, float? score = null)
        {
            T lce = Activator.CreateInstance<T>();
            if (args != null)
                lce.Distance = DocumentDistance(doc, args);

            Type leType = lce.GetType();
            var properties = leType.GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                LuceneNoIndex noindex = MetaFinder.PropertyLuceneInfo<LuceneNoIndex>(pi);
                if (noindex != null)
                    continue;
                LuceneAnalysis analysedField = MetaFinder.PropertyLuceneInfo<LuceneAnalysis>(pi);
                LuceneField storedField = MetaFinder.PropertyLuceneInfo<LuceneField>(pi);
                //first we get the analysed field the stored data located in named attribute
                //when the we have a non analysed field we check the stored field defined name else the name is the property name
                string fieldName = (analysedField != null) ? analysedField.Name : (storedField != null) ? storedField.Name : pi.Name;
                object value = null;
                string svalue;
                if (pi.PropertyType == typeof(GeoCoordinate))
                {
                    svalue = doc.Get($"{fieldName}");
                    var coords = svalue.Split(new char[] { ';' });
                    if (coords.Length > 1)
                    {
                        value = new GeoCoordinate()
                        {
                            Latitude = ParrotBeak.StringToDouble(coords[0]),
                            Longitude = ParrotBeak.StringToDouble(coords[1])
                        };
                    }
                    else
                    { value = new GeoCoordinate(); }
                }
                else
                {
                    svalue = doc.Get(fieldName);
                    if (!string.IsNullOrEmpty(svalue))
                    {
                        if (pi.PropertyType == typeof(string))
                        {
                            value = svalue;
                        }
                        else if (pi.PropertyType == typeof(DateTime))
                        {
                            value = ParrotBeak.DateDeserialize(svalue);
                        }
                        else if (pi.PropertyType == typeof(byte))
                        {
                            value = ParrotBeak.StringToByte(svalue);
                        }
                        else if (pi.PropertyType == typeof(short))
                        {
                            value = ParrotBeak.StringToShort(svalue);
                        }
                        else if (pi.PropertyType == typeof(int))
                        {
                            value = ParrotBeak.StringToInt(svalue);
                        }
                        else if (pi.PropertyType == typeof(long))
                        {
                            value = ParrotBeak.StringToLong(svalue);
                        }
                        else if (pi.PropertyType == typeof(ushort))
                        {
                            value = ParrotBeak.StringToUShort(svalue);
                        }
                        else if (pi.PropertyType == typeof(uint))
                        {
                            value = ParrotBeak.StringToUInt(svalue);
                        }
                        else if (pi.PropertyType == typeof(ulong))
                        {
                            value = ParrotBeak.StringToULong(svalue);
                        }
                        else if (pi.PropertyType == typeof(float))
                        {
                            value = ParrotBeak.StringToFloat(svalue);
                        }
                        else if (pi.PropertyType == typeof(double))
                        {
                            value = ParrotBeak.StringToDouble(svalue);
                        }
                        else if (pi.PropertyType == typeof(decimal))
                        {
                            value = ParrotBeak.StringToDecimal(svalue);
                        }
                        else if (pi.PropertyType == typeof(ParrotId))
                        {
                            value = (ParrotId)svalue;
                        }
                        else
                        {
                            value = JsonConvert.DeserializeObject(svalue);
                        }
                    }
                }
                if (value != null)
                {
                    if (pi.CanWrite)
                        pi.SetValue(lce, value);
                }
            }
            return lce;
        }
        #endregion

        #region Search
        /// <summary>
        /// Performs a search with the given options into the lucen index
        /// </summary>
        /// <param name="qsearch">The List with the search terms</param>
        /// <param name="page">The page of the results</param>
        /// <param name="pagerows">The number of entities per document</param>
        /// <param name="limit">Limit Results up to limit</param>
        /// <param name="sortby">Sort by fields</param>
        public SearchResults<T> Search(IEnumerable<SearchTerm> qsearch, int page, int pagerows, int limit = 500, List<SortOption> sortby = null)
        {
            //TODO validate data
            try
            {
                List<SortField> sort = null;
                if (sortby != null)
                {
                    sort = sortby.Select(x => new SortField(x.Field, SortFieldType.STRING_VAL, x.Descending)).ToList();
                }
                return Search(qsearch, page, pagerows, limit, sort);
            }
            catch (NoIndexException nie)
            {
                throw nie;
            }
        }

        /// <summary>
        /// Total pages for all entities
        /// </summary>
        /// <param name="resultsPerPage">results per page</param>
        public int PagesCount(int resultsPerPage)
        {
            return Pages(resultsPerPage);
        }

        /// <summary>
        /// Paging of all entities
        /// </summary>
        /// <param name="page">Page</param>
        /// <param name="iperpage">Results per page</param>
        /// <param name="sortby">sorting</param>
        /// <returns></returns>
        public SearchResults<T> AllPaged(int page, int iperpage, List<SortOption> sortby = null)
        {
            //TODO validate data
            try
            {
                List<SortField> sort = null;
                if (sortby != null)
                {
                    sort = sortby.Select(x => new SortField(x.Field, SortFieldType.STRING_VAL, x.Descending)).ToList();
                }
                return GetAllPaged(page, iperpage, sort);
            }
            catch (NoIndexException nie)
            {
                throw nie;
            }
        }
        #endregion
    }

}
