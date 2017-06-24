using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LuceneSearchEngine.Indexing.Entity;
using LuceneSearchEngine.Indexing.Metadata;

namespace LuceneSearchEngine.Search
{
    public class SearchTerm
    {
        /// <summary>
        /// The term Field
        /// </summary>
        public string Field { get; set; }
        /// <summary>
        /// Term Occurance in search
        /// </summary>
        public Lucene.Net.Search.Occur TermOccur { get; set; }
        /// <summary>
        /// The Query Term
        /// </summary>
        public string Term { get; set; }
        /// <summary>
        /// The Term is a range
        /// </summary>
        public SearchFieldOption SearchingOption { get; set; }
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
        public double? dFrom
        {
            get
            {
                int ret = 0;
                int.TryParse(From, out ret);
                return ret > 0 ? new int?(ret) : null;
            }
        }
        public double? dTo
        {
            get
            {
                int ret = 0;
                int.TryParse(To, out ret);
                return ret > 0 ? new int?(ret) : null;
            }
        }

        internal SearchTerm(string field, string term = "", SearchFieldOption option = SearchFieldOption.TERM, Lucene.Net.Search.Occur toccur = Lucene.Net.Search.Occur.MUST)
        {
            Field = field;
            TermOccur = toccur;
            Term = term;
            SearchingOption = option;
        }
        internal SearchTerm(string field, DateTime dFrom, DateTime dTo, Lucene.Net.Search.Occur toccur = Lucene.Net.Search.Occur.MUST)
        {
            Field = field;
            From = Utility.DateSerialize(dFrom);
            To = Utility.DateSerialize(dTo);
            TermOccur = toccur;
            SearchingOption = SearchFieldOption.INTRANGE;
        }

        internal SearchTerm(string field, int from, int to, Lucene.Net.Search.Occur toccur = Lucene.Net.Search.Occur.MUST)
        {
            Field = field;
            From = from.ToString();
            To = to.ToString();
            TermOccur = toccur;
            SearchingOption = SearchFieldOption.INTRANGE;
        }
        internal SearchTerm(string field, double from, double to, Lucene.Net.Search.Occur toccur = Lucene.Net.Search.Occur.MUST)
        {
            Field = field;
            From = from.ToString();
            To = to.ToString();
            TermOccur = toccur;
            SearchingOption = SearchFieldOption.DOUBLERANGE;
        }

        private static Lucene.Net.Search.Occur TermOccurance(Occurance o)
        {
            switch (o)
            {
                case Occurance.MUST:
                    return Lucene.Net.Search.Occur.MUST;
                case Occurance.SHOULD:
                    return Lucene.Net.Search.Occur.SHOULD;
                default:
                    return Lucene.Net.Search.Occur.MUST_NOT;
            }
        }
        /// <summary>
        /// Return a search term to use in order to search lucene index for a given field
        /// </summary>
        /// <typeparam name="T">Lucene Entity used for query</typeparam>
        /// <param name="propertyName">pass nameof(Field to Query)</param>
        /// <param name="q">The query term you want to search</param>
        /// <param name="o">The query Operator</param>
        /// <returns>The search term to be used for the Lucene Search</returns>
        public static SearchTerm QueryField<T>(string propertyName, string q, SearchFieldOption comparing, Occurance occure = Occurance.MUST) where T : IBaseLuceneEntity
        {
            LuceneAnalysis analysis = MetaFinder.PropertyLuceneInfo<LuceneAnalysis>(typeof(T), propertyName);
            if (analysis != null)
                return new SearchTerm(analysis.AnalysisName, q, comparing, TermOccurance(occure));
            return null;
        }

        public static SearchTerm QueryFieldBetween<T>(string propertyName,
            DateTime from,
            DateTime to,
            Occurance occure = Occurance.MUST) where T : IBaseLuceneEntity
        {
            return QueryFieldBetween<T>(propertyName, from, to, occure);
        }
        public static SearchTerm QueryFieldBetween<T>(string propertyName,int from,int to,Occurance occure = Occurance.MUST) where T : IBaseLuceneEntity
        {
            return QueryFieldBetween<T>(propertyName, from, to, occure);
        }
        public static SearchTerm QueryFieldBetween<T>(string propertyName,double from,double to,Occurance occure = Occurance.MUST) where T : IBaseLuceneEntity
        {
            return QueryFieldBetween<T>(propertyName, from, to, occure);
        }

        internal static SearchTerm QueryFieldBetween<T>(string propertyName, 
            object from,
            object to, 
            Occurance occure=Occurance.MUST) where T : IBaseLuceneEntity
        {
            string field = "";
            LuceneAnalysis analysis = MetaFinder.PropertyLuceneInfo<LuceneAnalysis>(typeof(T), propertyName);
            if (analysis != null)
            {
                field = analysis.Name;
            }
            else
            {
                LuceneField fld = MetaFinder.PropertyLuceneInfo<LuceneField>(typeof(T), propertyName);
                if (fld != null)
                {
                    field = fld.Name;
                }
            }
            if (!string.IsNullOrEmpty(field))
            {
                if (from.GetType() == typeof(DateTime) && to.GetType() == typeof(DateTime))
                {
                    return new SearchTerm(field, (DateTime)from, (DateTime)to, TermOccurance(occure));
                }
                else if (from.GetType() == typeof(int) & to.GetType() == typeof(int))
                {
                    return new SearchTerm(field, (DateTime)from, (DateTime)to, TermOccurance(occure));
                }
                else if (from.GetType() == typeof(double) || to.GetType() == typeof(double)
                    ||from.GetType() == typeof(float) || to.GetType() == typeof(float))
                {
                    return new SearchTerm(field, (double)from, (double)to, TermOccurance(occure));
                }
            }
            return null;
        }
    }
}
