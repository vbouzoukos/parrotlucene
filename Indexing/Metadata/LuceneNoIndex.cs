using System;
using System.Collections.Generic;
using System.Text;

namespace ParrotLucene.Indexing.Metadata
{
    /// <summary>
    /// We use this to store field data only. Used for query by id only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class LuceneNoIndex : Attribute
    {
    }
}
