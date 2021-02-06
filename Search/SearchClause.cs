using ParrotLucene.IndexedDocument;
using ParrotLucene.Indexing.Metadata;
using System;
using System.Linq.Expressions;

namespace ParrotLucene.Search
{
    /// <summary>
    /// Search clause to be used in order to build a lucene query
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchClause<T> where T : ParrotDocument, new()
    {
        internal SearchTerm Field;
        internal static string FieldName(Expression<Func<T, object>> field)
        {
            return FieldName(Reflection.GetExpressionPath(field));
        }
        internal static string FieldName(string propertyName)
        {
            LuceneAnalysis analysis = MetaFinder.PropertyLuceneInfo<LuceneAnalysis>(typeof(T), propertyName);
            if (analysis != null)
                return analysis.AnalysisName;
            else
            {
                LuceneField fieldanalysis = MetaFinder.PropertyLuceneInfo<LuceneField>(typeof(T), propertyName);
                if (fieldanalysis != null)
                {
                    return fieldanalysis.Name;
                }
                else
                {
                    return propertyName;
                }
            }
        }
        /// <summary>
        /// Search for the field that is like the term
        /// </summary>
        /// <param name="field">The field where we search for matching terms</param>
        /// <param name="query">The search term</param>
        /// <param name="boost">Boost used on results</param>
        /// <returns></returns>
        public static SearchClause<T> Term(Expression<Func<T, object>> field, object query, double? boost = null)
        {
            return new SearchClause<T>()
            {
                Field = new SearchTerm(FieldName(field), Convert.ToString(query), SearchFieldOption.TERM) { Boost = boost }
            };
        }

        /// <summary>
        /// Search for documents where the requested field is like the term
        /// </summary>
        /// <param name="field">The field where we search for matching terms</param>
        /// <param name="query">The search term with the wildcard character *</param>
        /// <param name="boost">Boost used on results</param>
        /// <returns></returns>
        public static SearchClause<T> Like(Expression<Func<T, object>> field, object query, double? boost = null)
        {
            return new SearchClause<T>()
            {
                Field = new SearchTerm(FieldName(field), Convert.ToString(query), SearchFieldOption.LIKE) { Boost = boost }
            };
        }
        /// <summary>
        /// Search documents where the requested field is exact as the search term
        /// </summary>
        /// <param name="field"></param>
        /// <param name="query"></param>
        /// <param name="boost"></param>
        /// <returns></returns>
        public static SearchClause<T> Exact(Expression<Func<T, object>> field, object query, double? boost = null)
        {
            return new SearchClause<T>()
            {
                Field = new SearchTerm(FieldName(field), Convert.ToString(query), SearchFieldOption.EXACT) { Boost = boost }
            };
        }
        /// <summary>
        /// Search documents where the requested field is fuzzy related to term
        /// </summary>
        /// <param name="field"></param>
        /// <param name="query"></param>
        /// <param name="boost"></param>
        /// <returns></returns>
        public static SearchClause<T> Fuzzy(Expression<Func<T, object>> field, object query, double? boost = null)
        {
            return new SearchClause<T>()
            {
                Field = new SearchTerm(FieldName(field), Convert.ToString(query), SearchFieldOption.FUZZY) { Boost = boost }
            };
        }
        /// <summary>
        /// Search for documents between a range
        /// </summary>
        /// <param name="field">The field where we search for matching terms</param>
        /// <param name="from">From this number/date</param>
        /// <param name="to">To this number/date</param>
        /// <param name="boost">Boost used on results</param>
        /// <returns></returns>
        public static SearchClause<T> Range(Expression<Func<T, object>> field, object from, object to, double? boost = null)
        {
            var searchTerm = SearchTerm.QueryFieldBetween<T>(FieldName(field), from, to);
            searchTerm.Boost = boost;
            return new SearchClause<T>()
            {
                Field = searchTerm
            };
        }
    }

}
