using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuceneSearchEngine.Indexing.Entity;
using LuceneSearchEngine.Indexing.Metadata;

namespace LuceneSearchEngine.Search
{
    public class SortOption
    {
        public string Field { get; internal set; }
        public bool Descending { get; internal set; }
        internal SortOption()
        {
            Field = "";
            Descending = false;
        }
        internal SortOption(string filed,bool desc=false)
        {
            Field = filed;
            Descending = desc;
        }
        public static SortOption SortField<T>(string propertyName, bool desc = false) where T : IBaseLuceneEntity
        {
            LuceneAnalysis analysis = MetaFinder.PropertyLuceneInfo<LuceneAnalysis>(typeof(T), propertyName);
            if (analysis != null)
                return new SortOption(analysis.Name, desc);
            else
            {
                LuceneField f = MetaFinder.PropertyLuceneInfo<LuceneField>(typeof(T), propertyName);
                if (f != null)
                    return new SortOption(f.Name, desc);
            }
            return null;
        }
    }
}
