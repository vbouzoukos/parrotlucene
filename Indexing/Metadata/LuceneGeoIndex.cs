using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace ParrotLucene.Indexing.Metadata
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LuceneGeoIndex : Attribute
    {
        public string Name { get; set; }

        public LuceneGeoIndex([CallerMemberName] string name = "")
        {
            Name = name;
        }
    }
}
