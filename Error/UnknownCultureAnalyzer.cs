using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ParrotLucene.Error
{
    /// <summary>
    /// Unknown culture analyzer exception
    /// </summary>
    public class UnknownCultureAnalyzer : Exception
    {
        public UnknownCultureAnalyzer(CultureInfo culture) : base($"For cultrure {culture} there is no native Analyzer implementation. Use BuildIndex with your custom implementation.")
        {
        }
    }
}
