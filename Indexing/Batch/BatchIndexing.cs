using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuceneSearchEngine.Indexing.Entity;
using LuceneSearchEngine.Search;
using Lucene.Net.Index;
namespace LuceneSearchEngine.Indexing.Batch
{
    public class BatchIndexing<T>:IDisposable where T : IBaseLuceneEntity
    {
        private LuceneSearch<T> Searcher;
        private IndexWriter batchwriter;
        public BatchIndexing()
        {
            Searcher = new LuceneSearch<T>();
            batchwriter = Searcher.InitBatchIndexing();
        }

        public void InsertEntityToBatch(T entity)
        {
            Searcher.AddUpdateLuceneIndex(entity,batchwriter);
        }

        public void Dispose()
        {
            try
            {
                Searcher.CleanUpBatchIndexing(batchwriter);
            }
            catch { }
        }
    }
}
