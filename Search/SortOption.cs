using ParrotLucene.IndexedDocument;
using ParrotLucene.Indexing.Metadata;


namespace ParrotLucene.Search
{
    /// <summary>
    /// Sorting results option
    /// </summary>
    public class SortOption
    {
        public string Field { get; internal set; }
        public bool Descending { get; internal set; }
        internal SortOption()
        {
            Field = "";
            Descending = false;
        }
        internal SortOption(string filed, bool desc = false)
        {
            Field = filed;
            Descending = desc;
        }
        public static SortOption SortField<T>(string propertyName, bool desc = false) where T : ParrotDocument
        {
            LuceneAnalysis analysedField = MetaFinder.PropertyLuceneInfo<LuceneAnalysis>(typeof(T), propertyName);
            LuceneField storedField = MetaFinder.PropertyLuceneInfo<LuceneField>(typeof(T), propertyName);
            string fieldName = (analysedField != null) ? analysedField.Name : (storedField != null) ? storedField.Name : propertyName;
            return new SortOption(fieldName, desc);
        }
    }
}
