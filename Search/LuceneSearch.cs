using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Lucene.Net.Documents;
using Lucene.Net.Util;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Queries;

using LuceneSearchEngine;
using LuceneSearchEngine.Search;
using LuceneSearchEngine.Indexing.Entity;
using LuceneSearchEngine.Indexing.Metadata;
using LuceneSearchEngine.Search.Core;
using LuceneSearchEngine.Error;

using Newtonsoft.Json;

namespace LuceneSearchEngine.Search
{
    /// <summary>
    /// Lucene Searching Class
    /// </summary>
    /// <typeparam name="T">The indexed entity we use to do the search</typeparam>
    public class LuceneSearch<T> : CoreSearch<T> where T : IBaseLuceneEntity
    {
        #region Constructor
        /// <summary>
        /// Default Constractor
        /// </summary>
        public LuceneSearch()
        {
            //Default name for lucene Index is the class name
            string indexName = typeof(T).Name;
            //If annotation sets name we use this name
            LuceneIndexedEntity attranalysis = MetaFinder.TypeLuceneInfo<LuceneIndexedEntity>(typeof(T));
            if (attranalysis != null)
            {
                if(!string.IsNullOrWhiteSpace(attranalysis.Name))
                    indexName = attranalysis.Name.Trim();
            }
            this.IndexName = indexName;
            //we use spatial location strategy for points
            this.SetPointVectorStrategy("pLocation");
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
                bool doanalysis = true;
                string value = "";
                object pov = pi.GetValue(lce);
                if (pov != null)
                {
                    if (pi.PropertyType == typeof(DateTime))
                    {
                        value = Utility.DateSerialize((DateTime)pov);
                    }
                    else if (pi.PropertyType == typeof(float))
                    {

                        value = ((float)pov).ToString(new System.Globalization.CultureInfo("en"));
                    }
                    else if (pi.PropertyType == typeof(double))
                    {
                        value = ((double)pov).ToString(new System.Globalization.CultureInfo("en"));
                    }
                    else
                    {
                        if (lce.Mapper == null)
                        {//JSON serialise
                            value = JsonConvert.SerializeObject(pov);
                        }
                        else
                        {//Custom Mapper
                            lce.Mapper.Save(lce.EntityId, pov);
                            doanalysis = false;
                        }
                    }
                }
                if (doanalysis)
                {
                    if (attranalysis != null)
                    {
                        doc.Add(new Field(attranalysis.Name, value, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        doc.Add(new Field(attranalysis.AnalysisName, value, Field.Store.YES, Field.Index.ANALYZED));
                    }
                    else
                    {
                        LuceneField fieldanalysis = MetaFinder.PropertyLuceneInfo<LuceneField>(pi);
                        if (fieldanalysis != null)
                        {
                            doc.Add(new Field(fieldanalysis.Name, value, Field.Store.YES, Field.Index.NOT_ANALYZED));
                        }
                    }
                }  
            }
            AddLocation(doc, lce.Phi, lce.Lamda);
        }
        /// <summary>
        /// Maps the document data into the entity
        /// </summary>
        /// <param name="doc">Lucene document</param>
        /// <param name="args">Spatial Arguments</param>
        /// <param name="score">Lucene score</param>
        /// <returns>The Mapped Entity T</returns>
        internal override T MapDocToData(Document doc, SpatialArgs args=null,float? score=null)
        {
            T lce = Activator.CreateInstance<T>();
            if(args!=null)
                lce.Distance = DocumentDistance(doc, args);

            Type leType = lce.GetType();
            var properties = leType.GetProperties();
            foreach (PropertyInfo pi in properties)
            {
                LuceneAnalysis attranalysis = MetaFinder.PropertyLuceneInfo<LuceneAnalysis>(pi);
                object value = null;
                string svalue = "";
                if (attranalysis != null)
                {
                    svalue =doc.Get(attranalysis.Name);
                }
                else
                {
                    LuceneField fieldanalysis = MetaFinder.PropertyLuceneInfo<LuceneField>(pi);
                    if (fieldanalysis != null)
                    {
                        svalue = doc.Get(fieldanalysis.Name);
                    }
                }
                if (!string.IsNullOrEmpty(svalue))
                {
                    if (pi.PropertyType == typeof(string))
                    {
                        value = svalue;
                    }
                    else if (pi.PropertyType == typeof(DateTime))
                    {
                        value = Utility.DateDeserialize(svalue);
                    }
                    else if (pi.PropertyType == typeof(int))
                    {
                        value = Utility.StringToInt(svalue);
                    }
                    else if (pi.PropertyType == typeof(double))
                    {
                        value = Utility.StringToDouble(svalue);
                    }
                    else if (pi.PropertyType == typeof(float))
                    {
                        value = Utility.StringToFloat(svalue);
                    }
                    else
                    {
                        if (lce.Mapper == null)
                        {
                            value = JsonConvert.DeserializeObject(svalue);
                        }
                        else
                        {
                            value = lce.Mapper.Load(lce.EntityId);
                        }
                    }
                    if (value != null)
                    {
                        if (pi.CanWrite)
                            pi.SetValue(lce, value);
                    }
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
        /// <param name="phi">Coordinate latitude</param>
        /// <param name="lamda">Coordinate longtitude</param>
        public LuceneResults<T> Search(List<SearchTerm> qsearch,int page,int pagerows,int limit=500,List<SortOption> sortby = null, double? phi=null, double? lamda=null)
        {
            //TODO validate data
            try
            {
                List<SortField> sort = null;
                if (sortby != null)
                {
                    sort = sortby.Select(x => new SortField(x.Field, SortField.STRING_VAL, x.Descending)).ToList();
                }
                if (phi.HasValue && lamda.HasValue)
                    return Search(qsearch, phi.Value, lamda.Value, 250, 1, 600, 600, sort);
                else
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
        public LuceneResults<T> AllPaged(int page, int iperpage,  List<SortOption> sortby = null)
        {
            //TODO validate data
            try
            {
                List<SortField> sort = null;
                if (sortby != null)
                {
                    sort = sortby.Select(x => new SortField(x.Field, SortField.STRING_VAL, x.Descending)).ToList();
                }
                return GetAllPaged(page,iperpage,sort);
            }
            catch (NoIndexException nie)
            {
                throw nie;
            }
        }
        #endregion
    }

}
