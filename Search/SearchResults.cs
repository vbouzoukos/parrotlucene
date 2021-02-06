using System.Collections.Generic;
using ParrotLucene.IndexedDocument;

namespace ParrotLucene.Search
{
    /// <summary>
    /// Query results structure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchResults<T> where T : ParrotDocument
    {
        /// <summary>
        /// Documents returned
        /// </summary>
        public IEnumerable<T> Documents { get; set; }

        /// <summary>
        /// Total documents found
        /// </summary>
        public int Count { get; set; }
    }
}
