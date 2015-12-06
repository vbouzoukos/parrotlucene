using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneSearchEngine
{
    public class SearchTerm
    {
        public Lucene.Net.Search.Occur TermOccur {get; set;}
        public string Term { get; set; }

        public SearchTerm(string term="", Lucene.Net.Search.Occur toccur= Lucene.Net.Search.Occur.MUST)
        {
            TermOccur = toccur;
            Term = term;
        }
    }
}
