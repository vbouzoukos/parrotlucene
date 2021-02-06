using System;
using System.Collections.Generic;
using System.Linq;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Spatial.Vector;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Analysis;

using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;

using ParrotLucene.Error;
using ParrotLucene.IndexedDocument;
using ParrotLucene.Utils;
using ParrotLucene.Base;

namespace ParrotLucene.Search.Core
{
    /// <summary>
    /// Base abstract class to search data into the lucene index the purpose is to be cunning like the eyes of the parrot
    /// </summary>
    /// <typeparam name="T">A parrot document instance</typeparam>
    abstract public class ParrotEyes<T> : Feather, IDisposable where T : ParrotDocument
    {
        #region Privates
        private IndexSearcher searcher = null;
        private IndexReader indexReader = null;

        #endregion

        #region Constructor
        public ParrotEyes(Wings wings, string indexname) : base(wings, indexname)
        {
            if (!DirectoryReader.IndexExists(LuceneDirectory))
                throw new NoIndexException(IndexName, LuceneDirectory.Directory.FullName);

            indexReader = DirectoryReader.Open(LuceneDirectory);
            searcher = new IndexSearcher(indexReader);
        }
        #endregion

        #region Query
        /// <summary>
        /// Base function Transforms Entity from a document
        /// </summary>
        /// <param name="doc">Document to transform</param>
        /// <param name="args">Search spatial arguments</param>
        /// <returns>Entity of document</returns>
        internal abstract T MapDocToData(Document doc, SpatialArgs args = null, float? score = null);

        /// <summary>
        /// The eyes of the parrot analyzer
        /// </summary>
        private Analyzer Analyzer
        {
            get
            {
                return wings.Analyzer;
            }
        }

        /// <summary>
        /// Index Searcher Object
        /// </summary>
        internal IndexSearcher Searcher
        {
            get
            {
                return searcher;
            }
        }

        /// <summary>
        /// Call when a update into the index occured
        /// </summary>
        public void Refresh()
        {
            if (indexReader != null)
                indexReader.Dispose();
            indexReader = DirectoryReader.Open(LuceneDirectory);
            searcher = new IndexSearcher(indexReader);
        }

        /// <summary>
        /// Returns Entities of a result Page
        /// </summary>
        /// <param name="hits">Documents that match the query</param>
        /// <param name="page">Result Page</param>
        /// <param name="resultsperpage">Results per page</param>
        /// <param name="args">Search spatial arguments(Used to transform Distance</param>
        /// <returns></returns>
        private SearchResults<T> PageResult(ScoreDoc[] hits, int page, int resultsperpage, SpatialArgs args = null)
        {
            SearchResults<T> results = new SearchResults<T>();
            int cpage = page > 0 ? page - 1 : 1;

            results.Documents = hits.Select(hit => MapDocToData(Searcher.Doc(hit.Doc), args, hit.Score)).Skip(resultsperpage * (cpage)).Take(resultsperpage).ToList();
            results.Count = hits.Length;
            return results;
        }

        /// <summary>
        /// Parses a query from a search string
        /// </summary>
        /// <param name="searchQuery">User Input</param>
        /// <param name="parser">Query Parser</param>
        /// <returns></returns>
        private Query ParseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }
        private Query FuzzyparseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(string.Format("{0}~", searchQuery.Trim()));
            }
            catch (ParseException)
            {
                query = parser.Parse(string.Format("{0}~0.7", QueryParser.Escape(searchQuery.Trim())));
            }
            return query;
        }
        private Query WildparseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(searchQuery.Trim());
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(searchQuery.Trim()));
            }
            return query;
        }

        /// <summary>
        /// Constracts a boolean query from a searchterm dictionary
        /// </summary>
        /// <param name="SearchTermFields"></param>
        /// <returns></returns>
        private QueryDesriptor SearchTermQuery(IEnumerable<SearchTerm> SearchTermFields)
        {
            QueryDesriptor descriptor = new QueryDesriptor();
            BooleanQuery query = new BooleanQuery();
            Query areaQuery = null;
            foreach (SearchTerm entry in SearchTermFields)
            {
                if (entry == null)
                    continue;
                if (entry.InArea != null && areaQuery == null)
                {
                    IPoint p = SpatialContext.MakePoint(entry.InArea.Center.Latitude, entry.InArea.Center.Longitude);
                    var circle = SpatialContext.MakeCircle(entry.InArea.Center.Latitude, entry.InArea.Center.Longitude, DistanceUtils.Dist2Degrees(entry.InArea.Radius, DistanceUtils.EARTH_EQUATORIAL_RADIUS_KM));
                    descriptor.SpatialArguments = new SpatialArgs(SpatialOperation.IsWithin, circle);
                    descriptor.Filter = spatialStrategy.MakeFilter(descriptor.SpatialArguments);
                    descriptor.InArea = true;
                    areaQuery = ((PointVectorStrategy)spatialStrategy).MakeQueryDistanceScore(descriptor.SpatialArguments);
                }
                else if (entry.SearchingOption == SearchFieldOption.INTRANGE)
                {
                    NumericRangeQuery<int> nq = NumericRangeQuery.NewInt32Range(entry.Field, entry.FromAsInt, entry.ToAsInt, true, true);
                    query.Add(nq, entry.TermOccur);
                }
                else if (entry.SearchingOption == SearchFieldOption.DOUBLERANGE|| entry.SearchingOption == SearchFieldOption.DECIMALRANGE)
                {
                    NumericRangeQuery<double> nq = NumericRangeQuery.NewDoubleRange(entry.Field, entry.FromAsDouble, entry.ToAsDouble, true, true);
                    query.Add(nq, entry.TermOccur);
                }
                //else if (entry.SearchingOption == SearchFieldOption.DECIMALRANGE)
                //{
                //    TermRangeQuery nq = TermRangeQuery.NewStringRange(entry.Field, entry.FromAsDecimal, entry.ToAsDecimal, true, true);
                //    query.Add(nq, entry.TermOccur);
                //}
                else if (entry.SearchingOption == SearchFieldOption.LIKE)
                {
                    QueryParser parser = new QueryParser(wings.IndexVersion, entry.Field, Analyzer);
                    Query pquery = WildparseQuery(wings.Transformation.Transform(entry.Term), parser);
                    query.Add(pquery, entry.TermOccur);
                }
                else if (entry.SearchingOption == SearchFieldOption.EXACT)
                {
                    var exactQuery = new TermQuery(new Term(entry.Field, QueryParser.Escape(entry.Term.Trim())));
                    query.Add(exactQuery, entry.TermOccur);
                }
                else if (entry.SearchingOption == SearchFieldOption.FUZZY)
                {
                    QueryParser parser = new QueryParser(wings.IndexVersion, entry.Field, Analyzer);
                    Query pquery = FuzzyparseQuery(wings.Transformation.Transform(entry.Term), parser);
                    query.Add(pquery, entry.TermOccur);
                }
                else
                {
                    QueryParser parser = new QueryParser(wings.IndexVersion, entry.Field, Analyzer);
                    Query pquery = ParseQuery(entry.Term, parser);
                    query.Add(pquery, entry.TermOccur);
                }
            }
            if (areaQuery != null)
            {
                BooleanQuery areaQueries = new BooleanQuery
                {
                    { query, Occur.MUST },
                    { areaQuery, Occur.MUST }
                };

                descriptor.Query = areaQueries;
            }
            else
            {
                descriptor.Query = query;
            }
            return descriptor;
        }

        /// <summary>
        /// Used to find one document by a field attribute
        /// </summary>
        /// <param name="fieldName">Field attribute</param>
        /// <param name="value">Attribute value</param>
        /// <returns>The result set with the document</returns>
        internal SearchResults<T> SearchExactOne(string fieldName, string value)
        {
            ScoreDoc[] hits = Searcher.Search(new TermQuery(new Term(fieldName, value)), 1).ScoreDocs;
            return PageResult(hits, 1, 1);
        }
        /// <summary>
        /// Performs a boolean search into a location circle area
        /// </summary>
        /// <param name="SearchTermFields">Query input</param>
        /// <param name="page">Results Page</param>
        /// <param name="ResultsPerPage">Results per page</param>
        /// <param name="limit">limit result default is 20</param>
        /// <returns>Query result</returns>
        internal SearchResults<T> Search(IEnumerable<SearchTerm> SearchTermFields,
            int page, int ResultsPerPage, int limit = 500, List<SortField> sorting = null)
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();
            QueryDesriptor query = SearchTermQuery(SearchTermFields);

            Sort sort;
            if (sorting == null)
            {
                if (query.InArea)
                {
                    sort = new Sort(new SortField("Distance", SortFieldType.SCORE, true));
                }
                else
                {
                    sort = Sort.RELEVANCE;
                }
            }
            else
            {
                if (query.InArea)
                {
                    sorting.Add(new SortField("Distance", SortFieldType.SCORE, true));
                }
                sort = new Sort(sorting.ToArray());
            }
            ScoreDoc[] hits;
            if (query.InArea)
            {
                TopDocs topDocs = Searcher.Search(query.Query, query.Filter, limit, sort);
                hits = topDocs.ScoreDocs;
            }
            else
            {
                hits = Searcher.Search(query.Query, null, limit, sort).ScoreDocs;
            }
            //stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            //TimeSpan ts = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine(ts);
            return PageResult(hits, page, ResultsPerPage, query.SpatialArguments);
        }

        /// <summary>
        /// Search input terms on given fields
        /// </summary>
        /// <param name="searchQuery">Search input</param>
        /// <param name="searchFields">search fields</param>
        /// <param name="page">Results Page</param>
        /// <param name="ResultsPerPage">Results per page</param>
        /// <param name="limit">limit result default is 20</param>
        /// <returns>Query results</returns>
        internal SearchResults<T> Search(string searchQuery, IEnumerable<string> searchFields, int page, int ResultsPerPage, int limit = 500, List<SortField> sorting = null)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return new SearchResults<T>
                { Documents = new List<T>(), Count = 0 };
            }
            IEnumerable<string> terms = searchQuery.Trim().Replace("-", " ").Split(' ').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            string q = string.Join(" ", terms);
            // set up lucene searcher
            MultiFieldQueryParser parser = new MultiFieldQueryParser(wings.IndexVersion, searchFields.ToArray(), Analyzer);
            Query query = ParseQuery(q, parser);
            Sort sort = null;
            ScoreDoc[] hits;
            if (sorting == null)
            {
                hits = Searcher.Search(query, limit).ScoreDocs;
            }
            else
            {
                sort = new Sort(sorting.ToArray());
                hits = Searcher.Search(query, null, limit, sort).ScoreDocs;
            }

            return PageResult(hits, page, ResultsPerPage);
        }

        /// <summary>
        /// Returns all documents
        /// </summary>
        /// <returns></returns>
        internal SearchResults<T> GetAllIndexRecords()
        {
            //validate search index
            if (!System.IO.Directory.EnumerateFiles(LuceneDir).Any())
            {
                return new SearchResults<T>
                { Documents = new List<T>(), Count = 0 };
            }
            // set up lucene searcher
            List<Document> docs = new List<Document>();
            for (int i = 0; i < indexReader.MaxDoc; i++)
            {

                var doc = indexReader.Document(i);
                docs.Add(doc);
            }
            //while (term.Next()) docs.Add(Searcher.Doc(term.Doc));
            return new SearchResults<T>
            { Documents = docs.Select(x => MapDocToData(x)).ToList(), Count = docs.Count };

        }

        /// <summary>
        /// Gets all document paged
        /// </summary>
        /// <param name="page">page number</param>
        /// <param name="ResultsPerPage">Results Per Page</param>
        /// <param name="sorting">Sorting</param>
        internal SearchResults<T> GetAllPaged(int page, int ResultsPerPage, List<SortField> sorting = null)
        {
            //validate search index
            if (!System.IO.Directory.EnumerateFiles(LuceneDir).Any())
            {
                return new SearchResults<T>
                { Documents = new List<T>(), Count = 0 };
            }
            MatchAllDocsQuery aQuery = new MatchAllDocsQuery();
            Sort sort = null;
            ScoreDoc[] hits;
            int limit = (page + 1) * ResultsPerPage;
            if (sorting == null)
            {
                hits = Searcher.Search(aQuery, limit).ScoreDocs;
            }
            else
            {
                sort = new Sort(sorting.ToArray());
                hits = Searcher.Search(aQuery, null, limit, sort).ScoreDocs;
            }

            SearchResults<T> results = new SearchResults<T>();
            int cpage = page > 0 ? page - 1 : 1;
            results.Documents = hits.Select(hit => MapDocToData(Searcher.Doc(hit.Doc), null, hit.Score)).Skip(ResultsPerPage * (cpage)).Take(ResultsPerPage).ToList();
            results.Count = Pages(ResultsPerPage);
            return results;
        }
        internal int DocsCount()
        {
            return Searcher.IndexReader.NumDocs;
        }
        internal int Pages(int ResultsPerPage)
        {
            int count = Searcher.IndexReader.NumDocs; ;
            return ParrotBeak.PagesCount(ResultsPerPage, count);
        }

        #endregion

        #region Dispose
        public void Dispose()
        {
            CleanSearcher();
        }
        /// <summary>
        /// Removes index searcher
        /// </summary>
        internal void CleanSearcher()
        {
            if (indexReader != null)
                indexReader.Dispose();
            indexReader = null;
        }
        #endregion
    }
}