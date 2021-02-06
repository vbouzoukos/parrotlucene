#region Analysis
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Ar;
using Lucene.Net.Analysis.Bg;
using Lucene.Net.Analysis.Br;
using Lucene.Net.Analysis.Ca;
using Lucene.Net.Analysis.Cjk;
using Lucene.Net.Analysis.Cz;
using Lucene.Net.Analysis.Da;
using Lucene.Net.Analysis.De;
using Lucene.Net.Analysis.Es;
using Lucene.Net.Analysis.El;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Eu;
using Lucene.Net.Analysis.Fa;
using Lucene.Net.Analysis.Fi;
using Lucene.Net.Analysis.Fr;
using Lucene.Net.Analysis.Ga;
using Lucene.Net.Analysis.Gl;
using Lucene.Net.Analysis.Hi;
using Lucene.Net.Analysis.Hu;
using Lucene.Net.Analysis.Hy;
using Lucene.Net.Analysis.Id;
using Lucene.Net.Analysis.It;
using Lucene.Net.Analysis.Lv;
using Lucene.Net.Analysis.Nl;
using Lucene.Net.Analysis.No;
using Lucene.Net.Analysis.Pt;
using Lucene.Net.Analysis.Ro;
using Lucene.Net.Analysis.Ru;
using Lucene.Net.Analysis.Sv;
using Lucene.Net.Analysis.Tr;
#endregion

using Lucene.Net.Util;

using ParrotLucene.Error;
using ParrotLucene.Indexing;
using ParrotLucene.IndexedDocument;

using System;
using System.Collections.Generic;
using System.Globalization;
using ParrotLucene.Search;
using ParrotLucene.Analyzers;

namespace ParrotLucene
{
    /// <summary>
    /// I am parrot who is trained to fetch results from a lucene index
    /// </summary>
    public class Parrot : IDisposable
    {
        const LuceneVersion IndexVersion = LuceneVersion.LUCENE_48;
        private readonly Dictionary<string, Wings> WingsIndex;
        public string IndexPath { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="indexPath">The path where my lucene indexes are</param>
        public Parrot(string indexPath)
        {
            IndexPath = indexPath;
            WingsIndex = new Dictionary<string, Wings>();
        }
        /// <summary>
        /// Dispose implementation
        /// </summary>
        public void Dispose()
        {
            foreach (var w in WingsIndex.Values)
            {
                w.Dispose();
            }
        }
        /// <summary>
        /// Creates an index helper wing
        /// </summary>
        /// <param name="name">Name of the used index(id)</param>
        /// <param name="analyzer">How the data of the index is analysed</param>
        /// <returns></returns>
        public Wings BuildIndex(string name, Analyzer analyzer)
        {
            var wings = new Wings(IndexPath, name, analyzer);
            WingsIndex.Add(wings.Id, wings);
            return wings;
        }
        /// <summary>
        /// Creates an index helper wing
        /// </summary>
        /// <param name="name">Name of the used index(id)</param>
        /// <param name="cultureInfo">Native calture of lucene .NET to use as analyzer</param>
        /// <returns>Helper wing for the name index in lucene native culture</returns>
        public Wings BuildCultureIndex(string name, CultureInfo cultureInfo)
        {
            switch (cultureInfo.TwoLetterISOLanguageName.ToUpper())
            {
                case "EN":
                    return BuildIndex(name, new EnglishAnalyzer(IndexVersion));
                case "AR":
                    return BuildIndex(name, new ArabicAnalyzer(IndexVersion));
                case "BG":
                    return BuildIndex(name, new BulgarianAnalyzer(IndexVersion));
                case "BR":
                    return BuildIndex(name, new BrazilianAnalyzer(IndexVersion));
                case "JP":
                case "CN":
                case "KR":
                    switch (cultureInfo.Name)
                    {
                        default:
                            return BuildIndex(name, new CJKAnalyzer(IndexVersion));
                    }
                case "CZ":
                    return BuildIndex(name, new CzechAnalyzer(IndexVersion));
                case "DA":
                    return BuildIndex(name, new DanishAnalyzer(IndexVersion));
                case "DE":
                    return BuildIndex(name, new GermanAnalyzer(IndexVersion));
                case "GR":
                    return BuildIndex(name, new GreekAnalyzer(IndexVersion));
                case "EU":
                    return BuildIndex(name, new BasqueAnalyzer(IndexVersion));
                case "ES":
                    switch (cultureInfo.Name)
                    {
                        case "ca-ES":
                            return BuildIndex(name, new CatalanAnalyzer(IndexVersion));
                        default:
                            return BuildIndex(name, new SpanishAnalyzer(IndexVersion));
                    }
                case "FA":
                    return BuildIndex(name, new PersianAnalyzer(IndexVersion));
                case "FI":
                    return BuildIndex(name, new FinnishAnalyzer(IndexVersion));
                case "FR":
                    return BuildIndex(name, new FrenchAnalyzer(IndexVersion));
                case "GA":
                    return BuildIndex(name, new IrishAnalyzer(IndexVersion));
                case "GL":
                    return BuildIndex(name, new GalicianAnalyzer(IndexVersion));
                case "HI":
                    return BuildIndex(name, new HindiAnalyzer(IndexVersion));
                case "HU":
                    return BuildIndex(name, new HungarianAnalyzer(IndexVersion));
                case "HY":
                    return BuildIndex(name, new ArmenianAnalyzer(IndexVersion));
                case "ID":
                    return BuildIndex(name, new IndonesianAnalyzer(IndexVersion));
                case "IT":
                    return BuildIndex(name, new ItalianAnalyzer(IndexVersion));
                case "LV":
                    return BuildIndex(name, new LatvianAnalyzer(IndexVersion));
                case "NL":
                    return BuildIndex(name, new DutchAnalyzer(IndexVersion));
                case "NO":
                    return BuildIndex(name, new NorwegianAnalyzer(IndexVersion));
                case "PT":
                    return BuildIndex(name, new PortugueseAnalyzer(IndexVersion));
                case "RO":
                    return BuildIndex(name, new RomanianAnalyzer(IndexVersion));
                case "RU":
                    return BuildIndex(name, new RussianAnalyzer(IndexVersion));
                case "SV":
                    return BuildIndex(name, new SwedishAnalyzer(IndexVersion));
                case "TR":
                    return BuildIndex(name, new TurkishAnalyzer(IndexVersion));
                default:
                    throw new UnknownCultureAnalyzer(cultureInfo);
            }
        }
        /// <summary>
        /// Create a helping wing for a Greek language accent insensitive index
        /// </summary>
        /// <param name="name">Name of the index(id)</param>
        /// <returns>Helping wing for a Greek language accent insensitive index</returns>
        public Wings BuildGreekAccentInsensitiveIndex(string name)
        {
            var wings = new Wings(IndexPath, name, new GreekACIAnalyzer(IndexVersion));
            WingsIndex.Add(wings.Id, wings);
            return wings;
        }
        /// <summary>
        /// Creates an indexer
        /// </summary>
        /// <typeparam name="T">ParrotDocument class instance of the indexed documents</typeparam>
        /// <param name="wings">Helping wing</param>
        /// <returns>Indexer instance</returns>
        public Indexer<T> BuildIndexer<T>(Wings wings) where T : ParrotDocument
        {
            return new Indexer<T>(wings);
        }
        /// <summary>
        /// Creates an IndexSearch seeker
        /// </summary>
        /// <typeparam name="T">ParrotDocument class instance of the indexed documents</typeparam>
        /// <param name="wings">Helping wing</param>
        /// <returns>IndexSearch seeker instance(keep in mind to refresh when you have data updated)</returns>
        public IndexSearch<T> BuildSeek<T>(Wings wings) where T : ParrotDocument
        {
            return new IndexSearch<T>(wings);
        }
    }
}
