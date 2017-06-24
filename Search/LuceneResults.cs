using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuceneSearchEngine.Indexing.Entity;
using LuceneSearchEngine.Indexing.Metadata;

namespace LuceneSearchEngine
{
    public class LuceneResults<T> where T : IBaseLuceneEntity
    {
        public IEnumerable<T> Results { get; set; }
        public int ResultsCount { get; set; }
    }
}
