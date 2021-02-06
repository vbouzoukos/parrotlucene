using ParrotLucene.Base;
using System;
using System.Collections.Generic;
using System.Text;

namespace ParrotLucene.TermTransformation
{
    /// <summary>
    /// No transformation
    /// </summary>
    public class StandardTransformation : ITransform
    {
        public string Transform(string source)
        {
            return source;
        }
    }
}
