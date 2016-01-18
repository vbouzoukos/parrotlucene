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

namespace LuceneSearchEngine
{
    abstract public class LuceneSearch<T> where T : IBaseLuceneEntity
    {
        //todo: we will add required Lucene methods here, step-by-step...
        private string _indexname = "lucene_index";
        private SpatialContext sptctx;
        private SpatialStrategy strategy;
        private IndexSearcher searcher;
        private IndexReader indexReader;

        public string IndexName
        {
            get { return _indexname; }
            set{ _indexname = value;}
        }
        private string _luceneDir
        {
            get {return Path.Combine(LuceneSettings.IndexPath, _indexname);}
        }
        private FSDirectory _directoryTemp;
        private FSDirectory _directory
        {
            get
            {
                if (_directoryTemp == null) _directoryTemp = FSDirectory.Open(new DirectoryInfo(_luceneDir));
                if (IndexWriter.IsLocked(_directoryTemp)) IndexWriter.Unlock(_directoryTemp);
                var lockFilePath = Path.Combine(_luceneDir, string.Format( "{0}_write.lock", _indexname));
                if (File.Exists(lockFilePath)) File.Delete(lockFilePath);
                return _directoryTemp;
            }
        }

        private void _addToLuceneIndex(T entitydata, IndexWriter writer)
        {
            // remove older index entry
            var searchQuery = new TermQuery(new Term("EntityId", entitydata.EntityId));
            writer.DeleteDocuments(searchQuery);
            // add new index entry
            var doc = new Document();
            doc.Add(new Field("EntityId", entitydata.EntityId, Field.Store.YES, Field.Index.NOT_ANALYZED));

            // add lucene fields mapped to db fields
            //doc.Add(new Field("Id", sampleData.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            //doc.Add(new Field("Name", sampleData.Name, Field.Store.YES, Field.Index.ANALYZED));
            //doc.Add(new Field("Description", sampleData.Description, Field.Store.YES, Field.Index.ANALYZED));
            Indexing(doc,entitydata);
            // add entry to index
            writer.AddDocument(doc);
        }
        public void SetPointVectorStrategy(string locationField)
        {
            sptctx = SpatialContext.GEO;
            strategy = new PointVectorStrategy(sptctx, locationField);          
        }
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

        public abstract void Indexing(Document doc ,T lce);

        public void AddLocation(Document doc,double x,double y)
        {
            Shape point = SpatialContext.MakePoint(x, y);
            foreach (AbstractField field in strategy.CreateIndexableFields(point))
            {
                doc.Add(field);
            }
            Spatial4n.Core.Io.ShapeReadWriter srw = new Spatial4n.Core.Io.ShapeReadWriter(SpatialContext);         
            doc.Add(new Field(strategy.GetFieldName(), srw.WriteShape(point), Field.Store.YES, Field.Index.NOT_ANALYZED));
        }
        public void AddUpdateLuceneIndex(T luceneData)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                // add data to lucene search index (replaces older entry if any)
                _addToLuceneIndex(luceneData, writer);
                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }

        public void AddUpdateLuceneIndex(T entitydata, IndexWriter writer)
        {
            _addToLuceneIndex(entitydata, writer);
        }
        public IndexWriter InitBatchIndexing()
        {
            StandardAnalyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);
            IndexWriter writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED);
            return writer;
        }

        public void CleanUpBatchIndexing(IndexWriter writer)
        {
            writer.Commit();
            writer.Optimize();
            writer.Analyzer.Close();
            writer.Dispose();
        }

        public void ClearLuceneIndexRecord(T entitydata)
        {
            // init lucene
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
            // remove older index entry
                var searchQuery = new TermQuery(new Term("EntityId", entitydata.EntityId));
                writer.DeleteDocuments(searchQuery);

                // close handles
                analyzer.Close();
                writer.Dispose();
            }
        }
        public bool ClearLuceneIndex()
        {
            try
            {
                var analyzer = new StandardAnalyzer(Version.LUCENE_30);
                using (var writer = new IndexWriter(_directory, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
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

        public void Optimize()
        {
            var analyzer = new StandardAnalyzer(Version.LUCENE_30);
            using (var writer = new IndexWriter(_directory, analyzer, IndexWriter.MaxFieldLength.UNLIMITED))
            {
                analyzer.Close();
                writer.Optimize();
                writer.Dispose();
            }
        }

    
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
        public abstract T MapDocToData(Document doc);
        public abstract T MapDocToData(Document doc, SpatialArgs args);
        private IndexSearcher Searcher
        {
            get
            {
                if (searcher == null)
                {
                    indexReader = IndexReader.Open(_directory, true);
                    searcher = new IndexSearcher(indexReader);
                }
                return searcher;
            }
        }
        public void SetSearcher()
        {
            indexReader = IndexReader.Open(_directory, true);
            searcher = new IndexSearcher(indexReader);
        }
        public void CleanSearcher()
        {
            indexReader.Dispose();
            searcher.Dispose();
            searcher = null;
            indexReader = null;
        }

        private IEnumerable<T> PageResult(ScoreDoc[] hits,int page, int resultsperpage,SpatialArgs args=null)
        {
            IEnumerable<T> results = hits.Select(hit => MapDocToData(Searcher.Doc(hit.Doc), args)).Skip(resultsperpage * page).Take(resultsperpage).ToList();
            return results;
        }
        public IEnumerable<T> Search(Dictionary<string, SearchTerm> SearchTermFields, int page, int ResultsPerPage, int limit = 20)
        {
            // set up lucene searcher
            BooleanQuery query = new BooleanQuery();
            StandardAnalyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);

            foreach (KeyValuePair<string, SearchTerm> pair in SearchTermFields)
            {
                if (pair.Value.RangeTerm)
                {
                    NumericRangeQuery<int> nq = NumericRangeQuery.NewIntRange(pair.Key, pair.Value.iFrom, pair.Value.iTo, true, true);
                    query.Add(nq, pair.Value.TermOccur);
                }
                else
                {
                    QueryParser parser = new QueryParser(Version.LUCENE_30, pair.Key, analyzer);
                    Query pquery = parseQuery(pair.Value.Term, parser);

                    query.Add(pquery, pair.Value.TermOccur);
                }
            }
            ScoreDoc[] hits = Searcher.Search(query, null, limit, Sort.RELEVANCE).ScoreDocs;
            analyzer.Close();
            return PageResult(hits, page, ResultsPerPage);
        }

        public IEnumerable<T> Search(Dictionary<string, SearchTerm> searchTermFields, 
            Double latitude, Double longitude, int distance, 
            int page, int ResultsPerPage, int limit = 20)
        {
            System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            BooleanQuery query = new BooleanQuery();
            StandardAnalyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);

            foreach (KeyValuePair<string, SearchTerm> pair in searchTermFields)
            {
                if (pair.Value.RangeTerm)
                {
                    NumericRangeQuery<int> nq = NumericRangeQuery.NewIntRange(pair.Key, pair.Value.iFrom, pair.Value.iTo, true, true);
                    query.Add(nq, pair.Value.TermOccur);
                }
                else
                {
                    QueryParser parser = new QueryParser(Version.LUCENE_30, pair.Key, analyzer);
                    Query pquery = parseQuery(pair.Value.Term, parser);

                    query.Add(pquery, pair.Value.TermOccur);
                }
            }
            Point p = SpatialContext.MakePoint(latitude, longitude);
            var circle = SpatialContext.MakeCircle(latitude, longitude, DistanceUtils.Dist2Degrees(distance, DistanceUtils.EARTH_EQUATORIAL_RADIUS_MI));
            var args = new SpatialArgs(SpatialOperation.IsWithin, circle);
            var filter = strategy.MakeFilter(args);
            Query q = ((PointVectorStrategy)strategy).MakeQueryDistanceScore(args);
            Sort sort = new Sort(new SortField("Distance", SortField.SCORE, true));
            query.Add(q, Occur.MUST);
            TopDocs topDocs = Searcher.Search(query, filter, limit, sort);
            ScoreDoc[] hits = topDocs.ScoreDocs;
            stopWatch.Stop();
            // Get the elapsed time as a TimeSpan value.
            TimeSpan ts = stopWatch.Elapsed;
            System.Diagnostics.Debug.WriteLine(ts);
            return PageResult(hits, page, ResultsPerPage,args);
        }

        public IEnumerable<T> Search (string searchQuery, List<string> searchFields, int page, int ResultsPerPage, int limit=20)
        {
            if (string.IsNullOrEmpty(searchQuery)) return new List<T>();
            IEnumerable<string> terms = searchQuery.Trim().Replace("-", " ").Split(' ').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            string q = string.Join(" ", terms);
            // set up lucene searcher

            StandardAnalyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);

            MultiFieldQueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30, searchFields.ToArray(), analyzer);
            Query query = parseQuery(q, parser);
            ScoreDoc[] hits = Searcher.Search(query, null, limit, Sort.RELEVANCE).ScoreDocs;
            analyzer.Close();
            return PageResult(hits, page, ResultsPerPage);
        }

        public IEnumerable<T> Search (Double latitude, Double longitude, int distance, int limit = 20)
        {
            Point p = SpatialContext.MakePoint(latitude, longitude);
            var circle = SpatialContext.MakeCircle(latitude, longitude, DistanceUtils.Dist2Degrees(distance, DistanceUtils.EARTH_EQUATORIAL_RADIUS_MI));
            var args = new SpatialArgs(SpatialOperation.IsWithin, circle);
            var filter = strategy.MakeFilter(args);
            Query q = ((PointVectorStrategy)strategy).MakeQueryDistanceScore(args);
            Sort sort = new Sort(new SortField("Distance", SortField.SCORE, true));
            TopDocs topDocs = Searcher.Search(q, filter, limit, sort);


            ScoreDoc[] hits = topDocs.ScoreDocs;
            IEnumerable<T> results = hits.Select(hit => MapDocToData(Searcher.Doc(hit.Doc),args)).ToList();
            return results;
        }

        public double DocumentDistance(Document doc,SpatialArgs args)
        {
            Spatial4n.Core.Io.ShapeReadWriter srw = new Spatial4n.Core.Io.ShapeReadWriter(SpatialContext);
            var docPoint = (Point)srw.ReadShape(doc.Get(strategy.GetFieldName()));
            double docDistDEG = SpatialContext.GetDistCalc().Distance(args.Shape.GetCenter(), docPoint);
            double docDistInKM = DistanceUtils.Degrees2Dist(docDistDEG, DistanceUtils.EARTH_EQUATORIAL_RADIUS_KM);
            return docDistInKM;
        }

        public IEnumerable<T> GetAllIndexRecords()
        {
            // validate search index
            if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<T>();

            // set up lucene searcher
            List<Document> docs = new List<Document>();
            var term = indexReader.TermDocs();
            while (term.Next()) docs.Add(Searcher.Doc(term.Doc));
            return docs.Select(MapDocToData).ToList();
        }

    }
}