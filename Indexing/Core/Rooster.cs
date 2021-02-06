using System;
using System.Collections.Generic;

using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;

using ParrotLucene.Indexing.Metadata;
using ParrotLucene.Analyzers;
using ParrotLucene.Base;
using ParrotLucene.IndexedDocument;

namespace ParrotLucene.Indexing.Core
{
    /// <summary>
    /// Base class for indexing documents
    /// </summary>
    /// <typeparam name="T">A parrot document instance</typeparam>
    abstract public class Rooster<T> : Feather, IDisposable where T : ParrotDocument
    {
        #region Privates
        private readonly IndexWriter writer;
        #endregion
        #region Constructor
        public Rooster(Wings wings, string indexName) : base(wings, indexName)
        {
            var analyzer = new GreekACIAnalyzer(wings.IndexVersion);//ASCIIFoldingFilterFactory
            var config = new IndexWriterConfig(wings.IndexVersion, analyzer);
            writer = new IndexWriter(LuceneDirectory, config);
        }
        #endregion

        #region Indexing
        /// <summary>
        /// Base Function for indexing document
        /// </summary>
        /// <param name="doc">Document where we store data</param>
        /// <param name="lce">Entity which holds data</param>
        internal abstract void Indexing(Document doc, T lce);

        /// <summary>
        /// Adds Single document to Index
        /// </summary>
        /// <param name="entitydata">Entity with data</param>
        /// <param name="writer">Writer to store data</param>
        private void AddToLuceneIndex(T entitydata, IndexWriter writer)
        {
            // remove older index entry
            LuceneField attranalysis = MetaFinder.PropertyLuceneInfo<LuceneField>(entitydata.GetType(), nameof(entitydata.EntityId));

            //var searchQuery = new TermQuery(new Term(attranalysis.Name, entitydata.EntityId));
            writer.DeleteDocuments(new Term(attranalysis.Name, entitydata.EntityId));
            // add new index entry
            var doc = new Document();
            // add lucene fields
            Indexing(doc, entitydata);
            // add entry to index
            writer.AddDocument(doc);
        }
        /// <summary>
        /// Adds Single document to Index
        /// </summary>
        /// <param name="luceneData">Entity to store into the index</param> 
        /// <param name="commit">When true commits data to the index</param>
        public void Save(T luceneData, bool commit = true)
        {
            // add data to lucene search index (replaces older entry if any)
            AddToLuceneIndex(luceneData, writer);
            // close handles
            if (commit)
            {
                writer.Commit();
            }
        }
        public void SaveCollection(IEnumerable<T> collection, bool commit = true)
        {
            foreach (var item in collection)
            {
                AddToLuceneIndex(item, writer);
            }
            if (commit)
            {
                writer.Commit();
            }
        }
        /// <summary>
        /// Deletes Document from index
        /// </summary>
        /// <param name="entitydata">Entity data we want to delete</param>
        public void ClearLuceneIndexRecord(T entitydata)
        {
            // remove older index entry
            var searchQuery = new TermQuery(new Term("EntityId", entitydata.EntityId));
            writer.DeleteDocuments(searchQuery);
            writer.Commit();
        }
        /// <summary>
        /// Deletes all
        /// </summary>
        /// <returns>True if operation was successful</returns>
        public bool ClearLuceneIndex()
        {

            if (DirectoryReader.IndexExists(LuceneDirectory))
            {
                try
                {
                    // remove older index entries
                    writer.DeleteAll();
                    writer.Commit();

                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Used to optimise the index
        /// </summary>
        public void ForceMerge(int maxSegments = 1)
        {
            writer.ForceMerge(maxSegments);
        }
        //Stores batch changes
        public void Commit()
        {
            writer.Commit();
        }
        public bool IndexExists
        {
            get { return DirectoryReader.IndexExists(LuceneDirectory); }
        }
        #endregion
        public void Dispose()
        {
            writer.Dispose();
        }
    }
}