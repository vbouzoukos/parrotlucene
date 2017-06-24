using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Reflection;

using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

using Lucene.Net.Spatial;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Spatial.Vector;
using Lucene.Net.Spatial.Util;

using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using Spatial4n.Core.Io;

using LuceneSearchEngine.Indexing.Entity;
using LuceneSearchEngine.Indexing.Metadata;
using LuceneSearchEngine.Error;

namespace LuceneSearchEngine.Search.Core
{
    abstract public class CoreSearch<T>:IDisposable where T : IBaseLuceneEntity
    {
        #region Privates
        private FSDirectory directoryTemp=null;
        private string indexname = "lucene_index";

        private SpatialContext sptctx=null;
        private SpatialStrategy strategy = null;
        private IndexSearcher searcher = null;
        private IndexReader indexReader = null;
        private ACIAnalyzer sanalyzer =null;

        private ShapeReadWriter shaperw = null;
        #endregion

        #region Lucene Directory Index
        /// <summary>
        /// Returns Index Name
        /// </summary>
        public string IndexName
        {
            get { return indexname; }
            set{ indexname = value;}
        }
        /// <summary>
        /// Returns Index File Directory Path
        /// </summary>
        private string luceneDir
        {
            get {return Path.Combine(LuceneSettings.IndexPath, indexname);}
        }

        /// <summary>
        /// Lucene Directory Object
        /// </summary>
        private FSDirectory directory
        {
            get
            {
                //Create directory if it does not exists
                if (directoryTemp == null)
                    directoryTemp = FSDirectory.Open(new DirectoryInfo(luceneDir));
                return directoryTemp;
            }
        }
        #endregion

        #region Spatial Data
        internal SpatialStrategy Strategy
        {
            get
            {
                return strategy;
            }
        }
        internal SpatialContext SpatialContext
        {
            get
            {
                return sptctx;
            }
        }
        private ShapeReadWriter ShapeReaderWriter
        {
            get
            {
                if(shaperw== null)
                    shaperw = new Spatial4n.Core.Io.ShapeReadWriter(SpatialContext);
                return shaperw;
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
            sptctx = SpatialContext.GEO;
            strategy = new PointVectorStrategy(sptctx, locationField);          
       }
        /// <summary>
        /// Adds the location to the index
        /// </summary>
        /// <param name="doc">Document to add the location</param>
        /// <param name="x">Latitude</param>
        /// <param name="y">Longitude</param>
        internal void AddLocation(Document doc, double phi, double lamda)
        {
            Shape point = SpatialContext.MakePoint(phi, lamda);
            foreach (AbstractField field in strategy.CreateIndexableFields(point))
            {
                doc.Add(field);
            }
            doc.Add(new Field(strategy.GetFieldName(), ShapeReaderWriter.WriteShape(point), Field.Store.YES, Field.Index.NOT_ANALYZED));
        }
        /// <summary>
        /// Returns distance of the document in km from the query target
        /// </summary>
        /// <param name="doc">Document</param>
        /// <param name="args">SpatialArgs data</param>
        /// <returns></returns>
        internal double DocumentDistance(Document doc, SpatialArgs args)
        {
            var docPoint = (Point)ShapeReaderWriter.ReadShape(doc.Get(strategy.GetFieldName()));
            double docDistDEG = SpatialContext.GetDistCalc().Distance(args.Shape.GetCenter(), docPoint);
            double docDistInKM = DistanceUtils.Degrees2Dist(docDistDEG, DistanceUtils.EARTH_EQUATORIAL_RADIUS_KM);
            return docDistInKM;//docDistDEG;// 
        }
        #endregion

        #region Indexing
        /// <summary>
        /// Creates an indexwriter in order to use for batch insert
        /// </summary>
        /// <returns>The index writer</returns>
        internal IndexWriter InitBatchIndexing()
        {
            ACIAnalyzer analyzer = new ACIAnalyzer(Version.LUCENE_30);//ASCIIFoldingFilterFactory
            IndexWriter writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            return writer;
        }

        /// <summary>
        /// Clears Index Writer
        /// </summary>
        /// <param name="writer"></param>
        internal void CleanUpBatchIndexing(IndexWriter writer)
        {
            writer.Commit();
            writer.Optimize();
            writer.Analyzer.Close();
            writer.Dispose();
        }

        /// <summary>
        /// Base Function for indexing document
        /// </summary>
        /// <param name="doc">Document where we store data</param>
        /// <param name="lce">Entity which holds data</param>
        internal abstract void Indexing(Document doc ,T lce);

        /// <summary>
        /// Adds Single document to Index
        /// </summary>
        /// <param name="entitydata">Entity with data</param>
        /// <param name="writer">Writer to store data</param>
        private void addToLuceneIndex(T entitydata, IndexWriter writer)
        {
            // remove older index entry
            LuceneField attranalysis = MetaFinder.PropertyLuceneInfo<LuceneField>(typeof(IBaseLuceneEntity),nameof(IBaseLuceneEntity.EntityId));

            var searchQuery = new TermQuery(new Term(attranalysis.Name, entitydata.EntityId));
            writer.DeleteDocuments(searchQuery);
            // add new index entry
            var doc = new Document();
            // add lucene fields
            Indexing(doc, entitydata);
            // add entry to index
            writer.AddDocument(doc);
        }

        /// <summary>
        /// Adds Single document to Index
        /// </summary>
        /// <param name="luceneData">Entity to store into the index</param>
        public void AddUpdateLuceneIndex(T luceneData)
        {
            // init lucene
            using (var writer = new IndexWriter(directory, Analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entry if any)
                addToLuceneIndex(luceneData, writer);
                // close handles
                writer.Dispose();
            }
        }

        /// <summary>
        /// Adds a document into an opened Index Writer
        /// </summary>
        /// <param name="entitydata">Entity to store</param>
        /// <param name="writer">Index Writer</param>
        internal void AddUpdateLuceneIndex(T entitydata, IndexWriter writer)
        {
            addToLuceneIndex(entitydata, writer);
        }
        /// <summary>
        /// Deletes Document from index
        /// </summary>
        /// <param name="entitydata">Entity data we want to delete</param>
        public void ClearLuceneIndexRecord(T entitydata)
        {
            // init lucene
            var analyzer = new ACIAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
            // remove older index entry
                var searchQuery = new TermQuery(new Term("EntityId", entitydata.EntityId));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }
        /// <summary>
        /// Deletes all
        /// </summary>
        /// <returns>True if operation was successful</returns>
        public bool ClearLuceneIndex()
        {
            if (!IndexReader.IndexExists(directory))
                throw new NoIndexException(IndexName, directory.Directory.FullName);
            try
            {
                var analyzer = new ACIAnalyzer(Version.LUCENE_30);
                using (var writer = new IndexWriter(directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    // remove older index entries
                    writer.DeleteAll();

                    // close handles
                    analyzer.Close();
                    writer.Dispose();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Optimizes Index
        /// </summary>
        public void Optimize()
        {
            var analyzer = new ACIAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }
        public bool IndexExists
        {
            get { return IndexReader.IndexExists(directory); }
        }
        #endregion

        #region Query
        /// <summary>
        /// Base function Transforms Entity from a document
        /// </summary>
        /// <param name="doc">Document to transform</param>
        /// <param name="args">Search spatial arguments</param>
        /// <returns>Entity of document</returns>
        internal abstract T MapDocToData(Document doc, SpatialArgs args=null,float? score=null);

        private ACIAnalyzer Analyzer
        {
            get
            {
                if (sanalyzer == null)
                    sanalyzer = new ACIAnalyzer(Version.LUCENE_30);
                return sanalyzer;
            }
        }

        /// <summary>
        /// Index Searcher Object
        /// </summary>
        internal IndexSearcher Searcher
        {
            get
            {
                if (searcher == null)
                {
                    SetSearcher();
                }
                return searcher;
            }
        }
        /// <summary>
        /// Set ups a searching object to reuse for better performance
        /// </summary>
        internal void SetSearcher()
        {
            if (IndexReader.IndexExists(directory))
            {
                indexReader = IndexReader.Open(directory, true);
                searcher = new IndexSearcher(indexReader);
            }
            else
                throw new NoIndexException(IndexName,directory.Directory.FullName);
        }
        /// <summary>
        /// Removes index searcher
        /// </summary>
        internal void CleanSearcher()
        {
            if (sanalyzer != null)
                sanalyzer.Close();
            if(indexReader!=null)
                indexReader.Dispose();
            if(searcher!=null)
                searcher.Dispose();
            searcher = null;
            indexReader = null;
        }

        /// <summary>
        /// Returns Entities of a result Page
        /// </summary>
        /// <param name="hits">Documents that match the query</param>
        /// <param name="page">Result Page</param>
        /// <param name="resultsperpage">Results per page</param>
        /// <param name="args">Search spatial arguments(Used to transform Distance</param>
        /// <returns></returns>
        private LuceneResults<T> PageResult(ScoreDoc[] hits,int page, int resultsperpage,SpatialArgs args=null)
        {
            LuceneResults<T> results = new LuceneResults<T>();
            int cpage = page > 0 ? page - 1 : 1;
            
            results.Results = hits.Select(hit => MapDocToData(Searcher.Doc(hit.Doc) , args, hit.Score)).Skip(resultsperpage *(cpage)).Take(resultsperpage).ToList();
            results.ResultsCount = hits.Length;
            return results;
        }
        /// <summary>
        /// Parses a query from a search string
        /// </summary>
        /// <param name="searchQuery">User Input</param>
        /// <param name="parser">Query Parser</param>
        /// <returns></returns>
        private Query parseQuery(string searchQuery, QueryParser parser)
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
        private Query fuzzyparseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(string.Format("{0}~",searchQuery.Trim()));
            }
            catch (ParseException)
            {
                query = parser.Parse(string.Format("{0}~0.7", QueryParser.Escape(searchQuery.Trim())));
            }
            return query;
        }
        private Query wildparseQuery(string searchQuery, QueryParser parser)
        {
            Query query;
            try
            {
                query = parser.Parse(string.Format("{0}*", searchQuery.Trim()));
            }
            catch (ParseException)
            {
                query = parser.Parse(string.Format("{0}*", QueryParser.Escape(searchQuery.Trim())));
            }
            return query;
        }

        /// <summary>
        /// Constracts a boolean query from a searchterm dictionary
        /// </summary>
        /// <param name="SearchTermFields"></param>
        /// <returns></returns>
        private BooleanQuery SearchTermQuery(List<SearchTerm> SearchTermFields)
        {
            BooleanQuery query = new BooleanQuery();

            foreach (SearchTerm entry in SearchTermFields)
            {
                if (entry == null)
                    continue;
                if (entry.SearchingOption==SearchFieldOption.INTRANGE)
                {
                    NumericRangeQuery<int> nq = NumericRangeQuery.NewIntRange(entry.Field, entry.iFrom, entry.iTo, true, true);
                    query.Add(nq, entry.TermOccur);
                }
                else if (entry.SearchingOption == SearchFieldOption.DOUBLERANGE)
                {
                    NumericRangeQuery<double> nq = NumericRangeQuery.NewDoubleRange(entry.Field, entry.dFrom, entry.dTo, true, true);
                    query.Add(nq, entry.TermOccur);
                }
                else if (entry.SearchingOption == SearchFieldOption.LIKE)
                {
                    QueryParser parser = new QueryParser(Version.LUCENE_30, entry.Field, Analyzer);
                    Query pquery = wildparseQuery(AccentPhoneticTransform.Transform(entry.Term), parser);
                    query.Add(pquery, entry.TermOccur);
                }
                else if (entry.SearchingOption == SearchFieldOption.FUZZY)
                {
                    QueryParser parser = new QueryParser(Version.LUCENE_30, entry.Field, Analyzer);
                    Query pquery = fuzzyparseQuery(AccentPhoneticTransform.Transform(entry.Term), parser);
                    query.Add(pquery, entry.TermOccur);
                }

                else
                {
                    QueryParser parser = new QueryParser(Version.LUCENE_30, entry.Field, Analyzer);
                    Query pquery = parseQuery(entry.Term, parser);
                    query.Add(pquery, entry.TermOccur);
                }
            }
            return query;
        }
        /// <summary>
        /// Performs a boolean search
        /// </summary>
        /// <param name="SearchTermFields">Query input</param>
        /// <param name="page">Results Page</param>
        /// <param name="ResultsPerPage">Results per page</param>
        /// <param name="limit">limit result default is 20</param>
        /// <returns>Query result</returns>
        internal LuceneResults<T> Search(List<SearchTerm> SearchTermFields, int page, int ResultsPerPage, int limit = 500, List<SortField> sorting = null)
        {
            // set up lucene searcher
            BooleanQuery query = SearchTermQuery(SearchTermFields);
            Sort sort = null;
            if (sorting == null)
            {
                sort = Sort.RELEVANCE;
            }
            else
            {
                sort = new Sort(sorting.ToArray());
            }
            ScoreDoc[] hits = Searcher.Search(query, null, limit, sort).ScoreDocs;
            return PageResult(hits, page, ResultsPerPage);
        }
        /// <summary>
        /// Performs a boolean search into a location circle area
        /// </summary>
        /// <param name="SearchTermFields">Query input</param>
        /// <param name="latitude">latitude of search area(φ)</param>
        /// <param name="longitude">longitude of search area(λ)</param>
        /// <param name="distance">Radius distance in km</param>
        /// <param name="page">Results Page</param>
        /// <param name="ResultsPerPage">Results per page</param>
        /// <param name="limit">limit result default is 20</param>
        /// <returns>Query result</returns>
        internal LuceneResults<T> Search(List<SearchTerm> SearchTermFields, 
            double latitude, double longitude, double radius, 
            int page, int ResultsPerPage, int limit = 20,List<SortField> sorting=null)
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();

            BooleanQuery query = SearchTermQuery(SearchTermFields);

            Point p = SpatialContext.MakePoint(latitude, longitude);
            var circle = SpatialContext.MakeCircle(latitude, longitude, DistanceUtils.Dist2Degrees(radius, DistanceUtils.EARTH_EQUATORIAL_RADIUS_KM));
            var spatialArgs = new SpatialArgs(SpatialOperation.IsWithin, circle);
            var filter = strategy.MakeFilter(spatialArgs);
            Query q = ((PointVectorStrategy)strategy).MakeQueryDistanceScore(spatialArgs);

            Sort sort = null;
            if (sorting == null)
            {
                sort = new Sort(new SortField("Distance", SortField.SCORE, true));
            }
            else
            {
                sorting.Add(new SortField("Distance", SortField.SCORE, true));
                sort = new Sort(sorting.ToArray());
            }
            BooleanQuery areaQuery = new BooleanQuery();
            areaQuery.Add(query, Occur.MUST);
            areaQuery.Add(q, Occur.MUST);

            TopDocs topDocs = Searcher.Search(areaQuery, filter, limit, sort);
            ScoreDoc[] hits = topDocs.ScoreDocs;

            //stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            //TimeSpan ts = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine(ts);
            return PageResult(hits, page, ResultsPerPage, spatialArgs);
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
        internal LuceneResults<T> Search (string searchQuery, List<string> searchFields, int page, int ResultsPerPage, int limit=500, List<SortField> sorting = null)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return new LuceneResults<T>
                { Results = new List<T>(), ResultsCount = 0 };
            }
            IEnumerable<string> terms = searchQuery.Trim().Replace("-", " ").Split(' ').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            string q = string.Join(" ", terms);
            // set up lucene searcher
            MultiFieldQueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30, searchFields.ToArray(), Analyzer);
            Query query = parseQuery(q, parser);
            Sort sort = null;
            ScoreDoc[] hits;
            if (sorting == null)
            {
                hits = Searcher.Search(query,limit).ScoreDocs;
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
        internal LuceneResults<T> GetAllIndexRecords()
        {
            //validate search index
            if (!System.IO.Directory.EnumerateFiles(luceneDir).Any())
            {
                return new LuceneResults<T>
                { Results = new List<T>(), ResultsCount = 0 };
            }
            // set up lucene searcher
            List<Document> docs = new List<Document>();
            var term = indexReader.TermDocs();
            while (term.Next()) docs.Add(Searcher.Doc(term.Doc));
            return new LuceneResults<T>
            { Results = docs.Select(x => MapDocToData(x)).ToList(), ResultsCount = docs.Count };

        }
        /// <summary>
        /// Gets all document paged
        /// </summary>
        /// <param name="page">page number</param>
        /// <param name="ResultsPerPage">Results Per Page</param>
        /// <param name="sorting">Sorting</param>
        internal LuceneResults<T> GetAllPaged(int page, int ResultsPerPage, List<SortField> sorting = null)
        {
            //validate search index
            if (!System.IO.Directory.EnumerateFiles(luceneDir).Any())
            {
                return new LuceneResults<T>
                { Results = new List<T>(), ResultsCount = 0 };
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

            LuceneResults<T> results = new LuceneResults<T>();
            int cpage = page > 0 ? page - 1 : 1;
            results.Results = hits.Select(hit => MapDocToData(Searcher.Doc(hit.Doc), null, hit.Score)).Skip(ResultsPerPage * (cpage)).Take(ResultsPerPage).ToList();
            results.ResultsCount = Pages(ResultsPerPage);
            return results;
        }

        internal int DocsCount()
        {
            return Searcher.reader_ForNUnit.NumDocs();
        }

        internal int Pages(int ResultsPerPage)
        {
            int count = Searcher.reader_ForNUnit.NumDocs();
            return Utility.PagesCount(ResultsPerPage, count);
        }

        #endregion
        public void Dispose()
        {
            CleanSearcher();
        }
    }
}