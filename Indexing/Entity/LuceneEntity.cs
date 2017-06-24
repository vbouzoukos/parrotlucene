using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuceneSearchEngine.Indexing.Metadata;

namespace LuceneSearchEngine.Indexing.Entity
{
    /// <summary>
    /// A generic Lucene Search entity all indexed lucene entities can inherit this class
    /// </summary>
    public class LuceneEntity:IBaseLuceneEntity
    {
        [LuceneField]
        public string EntityId { get; set; }
        [LuceneField]
        public double Phi { get; set; }
        [LuceneField]
        public double Lamda { get; set; }
        public double Distance { get; set; }
        public ILuceneCustomMapper Mapper { get; set; }
    }
}
