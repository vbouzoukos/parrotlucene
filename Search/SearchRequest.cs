using ParrotLucene.IndexedDocument;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using Lucene.Net.Search;
using Lucene.Net.Index;
using ParrotLucene.Base;

namespace ParrotLucene.Search
{
    /// <summary>
    /// Used to create a query into an IndexSearcher
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchRequest<T> where T : ParrotDocument, new()
    {
        int MaxResults { get; set; }
        int Page { get; set; }
        int ResultPerPage { get; set; }
        internal IList<SearchTerm> Fields { get; set; } = new List<SearchTerm>();
        internal IList<SortOption> SortFields { get; set; } = new List<SortOption>();
        private readonly IndexSearch<T> seeker;
        public SearchRequest(IndexSearch<T> seeker, int page = 1, int resultPerPage = 20, int max = 500)
        {
            this.seeker = seeker;
            Page = page;
            ResultPerPage = resultPerPage;
            MaxResults = max;
        }
        public SearchRequest<T> Must(SearchClause<T> clause)
        {
            clause.Field.TermOccur = SearchTerm.TermOccurance(Occurance.MUST);
            Fields.Add(clause.Field);
            return this;
        }
        public SearchRequest<T> Should(SearchClause<T> clause)
        {
            clause.Field.TermOccur = SearchTerm.TermOccurance(Occurance.SHOULD);
            Fields.Add(clause.Field);
            return this;
        }
        public SearchRequest<T> Not(SearchClause<T> clause)
        {
            clause.Field.TermOccur = SearchTerm.TermOccurance(Occurance.NOT);
            Fields.Add(clause.Field);
            return this;
        }
        public SearchRequest<T> InArea(GeoCoordinate center, double radius = 500)
        {
            var areaTerm = new SearchTerm(new SearchArea(center.Latitude, center.Longitude, radius));
            if (Fields.Where(x => x.InArea != null).Any())
                throw new ArgumentException("An InArea clause is already defined.");
            Fields.Add(areaTerm);
            return this;
        }
        public T SearchById(Expression<Func<T, object>> field, object value)
        {
            var fieldName = SearchClause<T>.FieldName(field);
            var results = seeker.SearchExactOne(fieldName, Convert.ToString(value));
            return results.Documents.FirstOrDefault();
        }
        public SearchResults<T> Run()
        {
            return seeker.Search(Fields, Page, ResultPerPage, MaxResults);
        }
    }
}
