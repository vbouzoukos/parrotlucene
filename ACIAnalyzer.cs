using System;
using System.Collections.Generic;

using Lucene.Net.Analysis;
using Lucene.Net.Util;
using Version = Lucene.Net.Util.Version;
using Lucene.Net.Analysis.Standard;

namespace LuceneSearchEngine
{
    public class ACIAnalyzer : Analyzer
    {
        private ISet<string> stopSet;

        /// <summary> Specifies whether deprecated acronyms should be replaced with HOST type.
        /// See <a href="https://issues.apache.org/jira/browse/LUCENE-1068">https://issues.apache.org/jira/browse/LUCENE-1068</a>
        /// </summary>
        private bool replaceInvalidAcronym, enableStopPositionIncrements;

        /// <summary>An unmodifiable set containing some common English words that are usually not
        /// useful for searching. 
        /// </summary>
        public static readonly ISet<string> STOP_WORDS_SET;
        private Version matchVersion;

        /// <summary>Builds an analyzer with the default stop words (<see cref="STOP_WORDS_SET" />).
        /// </summary>
        /// <param name="matchVersion">Lucene version to match see <see cref="Version">above</see></param>
        public ACIAnalyzer(Version matchVersion)
                : this(matchVersion, STOP_WORDS_SET)
            { }

        /// <summary>Builds an analyzer with the given stop words.</summary>
        /// <param name="matchVersion">Lucene version to match See <see cref="Version">above</see> />
        ///
        /// </param>
        /// <param name="stopWords">stop words 
        /// </param>
        public ACIAnalyzer(Version matchVersion, ISet<string> stopWords)
        {
            stopSet = stopWords;
            SetOverridesTokenStreamMethod<ACIAnalyzer>();
            enableStopPositionIncrements = StopFilter.GetEnablePositionIncrementsVersionDefault(matchVersion);
            replaceInvalidAcronym = matchVersion.OnOrAfter(Version.LUCENE_24);
            this.matchVersion = matchVersion;
        }

        /// <summary>Builds an analyzer with the stop words from the given file.</summary>
        /// <seealso cref="WordlistLoader.GetWordSet(System.IO.FileInfo)">
        /// </seealso>
        /// <param name="matchVersion">Lucene version to match See <see cref="Version">above</see> />
        ///
        /// </param>
        /// <param name="stopwords">File to read stop words from 
        /// </param>
        public ACIAnalyzer(Version matchVersion, System.IO.FileInfo stopwords)
                : this (matchVersion, WordlistLoader.GetWordSet(stopwords))
            {
        }

        /// <summary>Builds an analyzer with the stop words from the given reader.</summary>
        /// <seealso cref="WordlistLoader.GetWordSet(System.IO.TextReader)">
        /// </seealso>
        /// <param name="matchVersion">Lucene version to match See <see cref="Version">above</see> />
        ///
        /// </param>
        /// <param name="stopwords">Reader to read stop words from 
        /// </param>
        public ACIAnalyzer(Version matchVersion, System.IO.TextReader stopwords)
               : this(matchVersion, WordlistLoader.GetWordSet(stopwords))
           { }

        /// <summary>Constructs a <see cref="StandardTokenizer" /> filtered by a <see cref="StandardFilter" />
        ///, a <see cref="LowerCaseFilter" /> and a <see cref="StopFilter" />. 
        /// </summary>
        public override TokenStream TokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            StandardTokenizer tokenStream = new StandardTokenizer(matchVersion, reader);
            tokenStream.MaxTokenLength = maxTokenLength;
            TokenStream result = new StandardFilter(tokenStream);
            result = new LowerCaseFilter(result);
            result = new ASCIIFoldingFilter(result);
            result = new GreekPhoneticFilter(result);
            result = new GreekAccentFilter(result);
            result = new StopFilter(enableStopPositionIncrements, result, stopSet);
            return result;
        }

        private sealed class SavedStreams
        {
            internal StandardTokenizer tokenStream;
            internal TokenStream filteredTokenStream;
        }

        /// <summary>Default maximum allowed token length </summary>
        public const int DEFAULT_MAX_TOKEN_LENGTH = 255;

        private int maxTokenLength = DEFAULT_MAX_TOKEN_LENGTH;

        /// <summary> Set maximum allowed token length.  If a token is seen
        /// that exceeds this length then it is discarded.  This
        /// setting only takes effect the next time tokenStream or
        /// reusableTokenStream is called.
        /// </summary>
        public virtual int MaxTokenLength
        {
            get { return maxTokenLength; }
            set { maxTokenLength = value; }
        }

        public override TokenStream ReusableTokenStream(System.String fieldName, System.IO.TextReader reader)
        {
            if (overridesTokenStreamMethod)
            {
                // LUCENE-1678: force fallback to tokenStream() if we
                // have been subclassed and that subclass overrides
                // tokenStream but not reusableTokenStream
                return TokenStream(fieldName, reader);
            }
            SavedStreams streams = (SavedStreams)PreviousTokenStream;
            if (streams == null)
            {
                streams = new SavedStreams();
                PreviousTokenStream = streams;
                streams.tokenStream = new StandardTokenizer(matchVersion, reader);
                streams.filteredTokenStream = new StandardFilter(streams.tokenStream);
                streams.filteredTokenStream = new LowerCaseFilter(streams.filteredTokenStream);
                streams.filteredTokenStream = new ASCIIFoldingFilter(streams.filteredTokenStream);
                streams.filteredTokenStream = new GreekPhoneticFilter(streams.filteredTokenStream);
                streams.filteredTokenStream = new GreekAccentFilter(streams.filteredTokenStream);
                streams.filteredTokenStream = new StopFilter(enableStopPositionIncrements,
                                                             streams.filteredTokenStream, stopSet);
            }
            else
            {
                streams.tokenStream.Reset(reader);
            }
            streams.tokenStream.MaxTokenLength = maxTokenLength;

            streams.tokenStream.SetReplaceInvalidAcronym(replaceInvalidAcronym);

            return streams.filteredTokenStream;
        }
        static ACIAnalyzer()
        {
            STOP_WORDS_SET = StopAnalyzer.ENGLISH_STOP_WORDS_SET;
        }
    }
}
