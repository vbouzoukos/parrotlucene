using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
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

using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;
using Spatial4n.Core.Shapes;
using Spatial4n.Core.Io;

namespace LuceneSearchEngine
{
    abstract public class LuceneSearch<T> where T : IBaseLuceneEntity
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
                if (directoryTemp == null) directoryTemp = FSDirectory.Open(new DirectoryInfo(luceneDir));
                if (IndexWriter.IsLocked(directoryTemp)) IndexWriter.Unlock(directoryTemp);
                var lockFilePath = Path.Combine(luceneDir, string.Format( "{0}_write.lock", indexname));
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return directoryTemp;
            }
        }
        #endregion

        #region Spatial Data
        public SpatialStrategy Strategy
        {
            get
            {
                return strategy;
            }
        }
        public SpatialContext SpatialContext
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
        public void SetPointVectorStrategy(string locationField)
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
        public void AddLocation(Document doc, double x, double y)
        {
            Shape point = SpatialContext.MakePoint(x, y);
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
        public double DocumentDistance(Document doc, SpatialArgs args)
        {
            var docPoint = (Point)ShapeReaderWriter.ReadShape(doc.Get(strategy.GetFieldName()));
            double docDistDEG = SpatialContext.GetDistCalc().Distance(args.Shape.GetCenter(), docPoint);
            double docDistInKM = DistanceUtils.Degrees2Dist(docDistDEG, DistanceUtils.EARTH_EQUATORIAL_RADIUS_KM);
            return docDistInKM;
        }
        #endregion

        #region Indexing
        /// <summary>
        /// Creates an indexwriter in order to use for batch insert
        /// </summary>
        /// <returns>The index writer</returns>
        public IndexWriter InitBatchIndexing()
        {
            ACIAnalyzer analyzer = new ACIAnalyzer(Version.LUCENE_30);//ASCIIFoldingFilterFactory
            IndexWriter writer = new IndexWriter(directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            return writer;
        }

        /// <summary>
        /// Clears Index Writer
        /// </summary>
        /// <param name="writer"></param>
        public void CleanUpBatchIndexing(IndexWriter writer)
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
        public abstract void Indexing(Document doc ,T lce);

        /// <summary>
        /// Adds Single document to Index
        /// </summary>
        /// <param name="entitydata">Entity with data</param>
        /// <param name="writer">Writer to store data</param>
        private void addToLuceneIndex(T entitydata, IndexWriter writer)
        {
            // remove older index entry
            var searchQuery = new TermQuery(new Term("EntityId", entitydata.EntityId));
            writer.DeleteDocuments(searchQuery);
            // add new index entry
            var doc = new Document();
            doc.Add(new Field("EntityId", entitydata.EntityId, Field.Store.YES, Field.Index.NOT_ANALYZED));

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
        public void AddUpdateLuceneIndex(T entitydata, IndexWriter writer)
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
        #endregion

        #region Query
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

        /// <summary>
        /// Base function Transforms Entity from a document
        /// </summary>
        /// <param name="doc">Document to transform</param>
        /// <returns>Entity of document</returns>
        public abstract T MapDocToData(Document doc);
        /// <summary>
        /// Base function Transforms Entity from a document
        /// </summary>
        /// <param name="doc">Document to transform</param>
        /// <param name="args">Search spatial arguments</param>
        /// <returns>Entity of document</returns>
        public abstract T MapDocToData(Document doc, SpatialArgs args);

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
        private IndexSearcher Searcher
        {
            get
            {
                if (searcher == null)
                {
                    indexReader = IndexReader.Open(directory, true);
                    searcher = new IndexSearcher(indexReader);
                }
                return searcher;
            }
        }
        /// <summary>
        /// Set ups a searching object to reuse for better performance
        /// </summary>
        public void SetSearcher()
        {
            indexReader = IndexReader.Open(directory, true);
            searcher = new IndexSearcher(indexReader);
        }
        /// <summary>
        /// Removes index searcher
        /// </summary>
        public void CleanSearcher()
        {
            if (sanalyzer != null)
                sanalyzer.Close();
            indexReader.Dispose();
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
            results.Results = hits.Select(hit => MapDocToData(Searcher.Doc(hit.Doc), args)).Skip(resultsperpage *(cpage)).Take(resultsperpage).ToList();
            results.ResultsCount = hits.Length;
            return results;
        }

        private BooleanQuery SearchTermQuery(Dictionary<string, SearchTerm> SearchTermFields)
        {
            BooleanQuery query = new BooleanQuery();

            foreach (KeyValuePair<string, SearchTerm> pair in SearchTermFields)
            {
                if (pair.Value.RangeTerm)
                {
                    NumericRangeQuery<int> nq = NumericRangeQuery.NewIntRange(pair.Key, pair.Value.iFrom, pair.Value.iTo, true, true);
                    query.Add(nq, pair.Value.TermOccur);
                }
                else
                {
                    QueryParser parser = new QueryParser(Version.LUCENE_30, pair.Key, Analyzer);
                    Query pquery = parseQuery(pair.Value.Term, parser);

                    query.Add(pquery, pair.Value.TermOccur);
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
        public LuceneResults<T> Search(Dictionary<string, SearchTerm> SearchTermFields, int page, int ResultsPerPage, int limit = 20)
        {
            // set up lucene searcher
            BooleanQuery query = SearchTermQuery(SearchTermFields);
            ScoreDoc[] hits = Searcher.Search(query, null, limit, Sort.RELEVANCE).ScoreDocs;
            return PageResult(hits, page, ResultsPerPage);
        }
        /// <summary>
        /// Performs a boolean search into a location circle area
        /// </summary>
        /// <param name="SearchTermFields">Query input</param>
        /// <param name="latitude">latitude of search area</param>
        /// <param name="longitude">longitude of search area</param>
        /// <param name="distance">Radius distance in km</param>
        /// <param name="page">Results Page</param>
        /// <param name="ResultsPerPage">Results per page</param>
        /// <param name="limit">limit result default is 20</param>
        /// <returns>Query result</returns>
        public LuceneResults<T> Search(Dictionary<string, SearchTerm> SearchTermFields, 
            double latitude, double longitude, double distance, 
            int page, int ResultsPerPage, int limit = 20)
        {
            //System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            //stopWatch.Start();
            BooleanQuery query = SearchTermQuery(SearchTermFields);

            Point p = SpatialContext.MakePoint(latitude, longitude);
            var circle = SpatialContext.MakeCircle(latitude, longitude, DistanceUtils.Dist2Degrees(distance, DistanceUtils.EARTH_EQUATORIAL_RADIUS_MI));
            var args = new SpatialArgs(SpatialOperation.IsWithin, circle);
            var filter = strategy.MakeFilter(args);
            Query q = ((PointVectorStrategy)strategy).MakeQueryDistanceScore(args);
            Sort sort = new Sort(new SortField("Distance", SortField.SCORE, true));
            query.Add(q, Occur.MUST);
            TopDocs topDocs = Searcher.Search(query, filter, limit, sort);
            ScoreDoc[] hits = topDocs.ScoreDocs;
            //stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            //TimeSpan ts = stopWatch.Elapsed;
            //System.Diagnostics.Debug.WriteLine(ts);
            return PageResult(hits, page, ResultsPerPage,args);
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
        public LuceneResults<T> Search (string searchQuery, List<string> searchFields, int page, int ResultsPerPage, int limit=20)
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
            ScoreDoc[] hits = Searcher.Search(query, null, limit, Sort.RELEVANCE).ScoreDocs;
            return PageResult(hits, page, ResultsPerPage);
        }

        /// <summary>
        /// Returns all documents
        /// </summary>
        /// <returns></returns>
        public LuceneResults<T> GetAllIndexRecords()
        {
            // validate search index
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
            { Results = docs.Select(MapDocToData).ToList(), ResultsCount = docs.Count };
        }
        #endregion
    }
}