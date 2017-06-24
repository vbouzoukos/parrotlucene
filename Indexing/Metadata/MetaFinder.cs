using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using LuceneSearchEngine.Search;
using LuceneSearchEngine.Search.Core;

namespace LuceneSearchEngine.Indexing.Metadata
{
    /// <summary>
    /// Used to read annotations of entity to help with storing data
    /// </summary>
    internal class MetaFinder
    {
        internal static T TypeLuceneInfo<T>(Type tp) where T:Attribute
        {
            var attranalysis = (T[])(tp.GetCustomAttributes(typeof(T), true));
            if (attranalysis.Length > 0)
            {
                return attranalysis[0];
            }
            return null;
        }

        internal static T PropertyLuceneInfo<T>(PropertyInfo pi) where T : Attribute
        {
            var attranalysis = (T[])(pi.GetCustomAttributes(typeof(T), true));
            if (attranalysis.Length > 0)
            {
                return attranalysis[0];
            }
            return null;
        }

        internal static T PropertyLuceneInfo<T>(Type t,string pname) where T : Attribute
        {            
            var attranalysis = (T[])((t.GetProperty(pname)).GetCustomAttributes(typeof(T), true));
            if (attranalysis.Length > 0)
            {
                return attranalysis[0];
            }
            return null;
        }
    }
}
