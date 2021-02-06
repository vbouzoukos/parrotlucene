using System;
using System.Collections.Generic;
using System.Text;

namespace ParrotLucene.Utils
{
    /// <summary>
    /// Array utils
    /// </summary>
    internal class ArrayUtils
    {
        //Removed function remove on lucene 4.8 version
        public static int GetNextSize(int targetSize)
        {
            /* This over-allocates proportional to the list size, making room
            * for additional growth.  The over-allocation is mild, but is
            * enough to give linear-time amortized behavior over a long
           * sequence of appends() in the presence of a poorly-performing
           * system realloc().
           * The growth pattern is:  0, 4, 8, 16, 25, 35, 46, 58, 72, 88, ...
           */
            return (targetSize >> 3) + (targetSize < 9 ? 3 : 6) + targetSize;
        }
    }
}
