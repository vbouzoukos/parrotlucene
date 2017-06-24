using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace LuceneSearchEngine.Indexing.Metadata
{
    /// <summary>
    /// We use this to store field data only. Used for query by id only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class LuceneField:Attribute
    {
        public string Name { get; set; }

        public LuceneField([CallerMemberName] string name="")
        {
            Name = name;
        }
    }
}
