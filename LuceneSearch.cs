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

namespace LuceneSearchEngine
{
    abstract public class LuceneSearch<T> where T : IBaseLuceneEntity
    {
        //todo: we will add required Lucene methods here, step-by-step...
        private string _indexname = "lucene_index";
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
        public abstract void Indexing(Document doc ,T lce);

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
        public  void ClearLuceneIndexRecord(T entitydata)
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

        public IEnumerable<T> Search(Dictionary<string, SearchTerm> searchTermFields, int limit = 20)
        {
            // set up lucene searcher
            using (IndexSearcher searcher = new IndexSearcher(_directory, false))
            {
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
                ScoreDoc[] hits = searcher.Search(query, null, limit, Sort.RELEVANCE).ScoreDocs;
                IEnumerable<T> results = hits.Select(hit => MapDocToData(searcher.Doc(hit.Doc))).ToList();
                analyzer.Close();
                searcher.Dispose();
                return results;

            }
        }
        public IEnumerable<T> Search (string searchQuery, List<string> searchFields,int limit=20)
        {
            if (string.IsNullOrEmpty(searchQuery)) return new List<T>();

            IEnumerable<string> terms = searchQuery.Trim().Replace("-", " ").Split(' ').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim() + "*");
            string q = string.Join(" ", terms);

            // set up lucene searcher
            using (IndexSearcher searcher = new IndexSearcher(_directory, false))
            {
                StandardAnalyzer analyzer = new StandardAnalyzer(Version.LUCENE_30);

                MultiFieldQueryParser parser = new MultiFieldQueryParser(Version.LUCENE_30, searchFields.ToArray(), analyzer);
                Query query = parseQuery(q, parser);
                ScoreDoc[] hits = searcher.Search(query, null, limit, Sort.RELEVANCE).ScoreDocs;
                IEnumerable<T> results = hits.Select(hit => MapDocToData(searcher.Doc(hit.Doc))).ToList();
                analyzer.Close();
                searcher.Dispose();
                return results;
                
            }
        }
        public IEnumerable<T> GetAllIndexRecords()
        {
            // validate search index
            if (!System.IO.Directory.EnumerateFiles(_luceneDir).Any()) return new List<T>();

            // set up lucene searcher
            IndexSearcher searcher = new IndexSearcher(_directory, false);
            IndexReader reader = IndexReader.Open(_directory, false);
            List<Document> docs = new List<Document>();
            var term = reader.TermDocs();
            while (term.Next()) docs.Add(searcher.Doc(term.Doc));
            reader.Dispose();
            searcher.Dispose();
            return docs.Select(MapDocToData).ToList();
        }

    }
}