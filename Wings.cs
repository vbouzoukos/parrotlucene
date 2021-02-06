using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using ParrotLucene.Base;
using ParrotLucene.TermTransformation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ParrotLucene
{
    public class Wings : IDisposable
    {
        public string Id { get; private set; }
        public string IndexPath { get; }
        public Analyzer Analyzer { get; private set; }
        public ITransform Transformation { get; private set; }
        public LuceneVersion IndexVersion { get { return LuceneVersion.LUCENE_48; } }
        public Wings(string path, string name, Analyzer analyser = null, ITransform transformation = null)
        {
            IndexPath = path;
            Analyzer = new StandardAnalyzer(IndexVersion);
            Id = name;
            if (analyser != null)
            {
                Analyzer = analyser;
            }
            else
            {
                Analyzer = new StandardAnalyzer(IndexVersion);
            }
            if (transformation != null)
            {
                Transformation = transformation;
            }
            else
            {
                Transformation = new StandardTransformation();
            }
        }

        public void Dispose()
        {
            if (this.Analyzer != null)
            { 
                this.Analyzer.Dispose();
            }
        }
    }
}
