using Lucene.Net.Search;
using ParrotLucene.IndexedDocument;
using ParrotLucene.Indexing.Metadata;
using ParrotLucene.Utils;
using System;
using System.Globalization;

namespace ParrotLucene.Search
{
    /// <summary>
    /// Search term structure
    /// </summary>
    public class SearchTerm
    {
        /// <summary>
        /// The term Field
        /// </summary>
        public string Field { get; set; }
        /// <summary>
        /// Term Occurance in search
        /// </summary>
        public Occur TermOccur { get; set; }
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

        public double? Boost { get; set; }
        public SearchArea InArea { get; set; }
        public int? FromAsInt
        {
            get
            {
                if (int.TryParse(From, out int ret))
                {
                    return new int?(ret);
                }
                return null;
            }
        }
        public int? ToAsInt
        {
            get
            {
                if (int.TryParse(To, out int ret))
                {
                    return new int?(ret);
                }
                return null;
            }
        }
        public double? FromAsDouble
        {
            get
            {
                if (double.TryParse(From, out double ret))
                {
                    return new double?(ret);
                }
                return null;
            }
        }
        public double? ToAsDouble
        {
            get
            {
                if (double.TryParse(To, out double ret))
                {
                    return new double?(ret);
                }
                return null;
            }
        }
        //public string FromAsDecimal
        //{
        //    get
        //    {
        //        if (decimal.TryParse(From, out decimal ret))
        //        {
        //            return ParrotBeak.DecimalSerialize(ret);
        //        }
        //        return null;
        //    }
        //}
        //public string ToAsDecimal
        //{
        //    get
        //    {
        //        if (decimal.TryParse(To, out decimal ret))
        //        {
        //            return ParrotBeak.DecimalSerialize(ret);
        //        }
        //        return null;
        //    }
        //}
        internal SearchTerm(string field, string term = "", SearchFieldOption option = SearchFieldOption.TERM, Occur toccur = Occur.MUST)
        {
            Field = field;
            TermOccur = toccur;
            Term = term;
            SearchingOption = option;
        }
        internal SearchTerm(string field, DateTime dFrom, DateTime dTo, Occur toccur = Occur.MUST)
        {
            Field = field;
            From = ParrotBeak.DateSerialize(dFrom);
            To = ParrotBeak.DateSerialize(dTo);
            TermOccur = toccur;
            SearchingOption = SearchFieldOption.INTRANGE;
        }
        internal SearchTerm(string field, int from, int to, Occur toccur = Occur.MUST)
        {
            Field = field;
            From = from.ToString(new CultureInfo("en"));
            To = to.ToString(new CultureInfo("en"));
            TermOccur = toccur;
            SearchingOption = SearchFieldOption.INTRANGE;
        }
        internal SearchTerm(string field, double from, double to, Occur toccur = Occur.MUST)
        {
            Field = field;
            From = from.ToString(new CultureInfo("en"));
            To = to.ToString(new CultureInfo("en"));
            TermOccur = toccur;
            SearchingOption = SearchFieldOption.DOUBLERANGE;
        }
        internal SearchTerm(string field, decimal from, decimal to, Occur toccur = Occur.MUST)
        {
            Field = field;
            From = from.ToString(new CultureInfo("en"));
            To = to.ToString(new CultureInfo("en"));
            TermOccur = toccur;
            SearchingOption = SearchFieldOption.DECIMALRANGE;
        }
        internal SearchTerm(SearchArea inArea)
        {
            InArea = inArea;
        }
        internal static Occur TermOccurance(Occurance o)
        {
            switch (o)
            {
                case Occurance.MUST:
                    return Occur.MUST;
                case Occurance.SHOULD:
                    return Occur.SHOULD;
                default:
                    return Occur.MUST_NOT;
            }
        }

        internal static SearchTerm QueryFieldBetween<T>(string propertyName,
            object from,
            object to,
            Occurance occure = Occurance.MUST) where T : ParrotDocument
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
                else if (from.GetType() == typeof(int) && to.GetType() == typeof(int)||
                    from.GetType() == typeof(short) && to.GetType() == typeof(short))
                {
                    return new SearchTerm(field, (int)from, (int)to, TermOccurance(occure));
                }
                else if ((from.GetType() == typeof(long) && to.GetType() == typeof(long)))
                {
                    return new SearchTerm(field, (decimal)from, (decimal)to, TermOccurance(occure));
                }
                else if (from.GetType() == typeof(double) || to.GetType() == typeof(double)
                    || from.GetType() == typeof(float) || to.GetType() == typeof(float)
                    || from.GetType() == typeof(decimal) && to.GetType() == typeof(decimal))
                {
                    return new SearchTerm(field, Convert.ToDouble(from), Convert.ToDouble(to), TermOccurance(occure));
                }
            }
            return null;
        }
    }
}
