using System;
using System.Collections.Generic;
using System.Text;

namespace ParrotLucene.Base
{
    /// <summary>
    /// Fuzzy and wildcard transformation Interface
    /// </summary>
    public interface ITransform
    {
        string Transform(string source);
    }
}
