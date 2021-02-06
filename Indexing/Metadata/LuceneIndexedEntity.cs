using System;

namespace ParrotLucene.Indexing.Metadata
{
    /// <summary>
    /// Marks the class Object as an Lucene Index Entity
    /// we can set the name of the index as parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LuceneIndexedEntity:Attribute
    {
        public string Name { get; set; }

        public LuceneIndexedEntity(string name = "")
        {
            Name = name;
        }
    }
}
