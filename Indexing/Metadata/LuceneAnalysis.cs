using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace LuceneSearchEngine.Indexing.Metadata
{
    /// <summary>
    /// Lucene Analysed Field (we use this to set the field as analysed for searching)
    /// It stores the actual data(needed for sorting) and the analysed data for searching
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class LuceneAnalysis:Attribute
    {
        public string Name { get; set; }
        internal string AnalysisName { get; set; }

        public LuceneAnalysis([CallerMemberName] string name="")
        {
            Name = name;
            AnalysisName = string.Format("{0}-analysis", Name);
        }
    }
}
