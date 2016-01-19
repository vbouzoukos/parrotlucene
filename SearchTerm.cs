using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuceneSearchEngine
{
    public class SearchTerm
    {
        /// <summary>
        /// Term Occurance in search
        /// </summary>
        public Lucene.Net.Search.Occur TermOccur {get; set;}
        /// <summary>
        /// The Query Term
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// The Term is a range
        /// </summary>
        public bool RangeTerm { get { return !string.IsNullOrEmpty(From) && !string.IsNullOrEmpty(To); } }
        /// <summary>
        /// Starting Range of the term
        /// </summary>
        public string From { get; set; }
        /// <summary>
        /// Ending Range of the Term
        /// </summary>
        public string To { get; set; }
        public int? iFrom
        {
            get
            {
                int ret = 0;
                int.TryParse(From, out ret);
                return ret > 0 ? new int?(ret) : null;
            }
        }
        public int? iTo
        {
            get
            {
                int ret = 0;
                int.TryParse(To, out ret);
                return ret > 0 ? new int?(ret) : null;
            }
        }
        public SearchTerm(string term="", Lucene.Net.Search.Occur toccur= Lucene.Net.Search.Occur.MUST)
        {
            TermOccur = toccur;
            Term = term;
        }
        public SearchTerm(DateTime dFrom, DateTime dTo, Lucene.Net.Search.Occur toccur = Lucene.Net.Search.Occur.MUST)
        {
            From = Utility.DateSerialize(dFrom);
            To = Utility.DateSerialize(dTo);
            TermOccur = toccur;
        }
    }
}
