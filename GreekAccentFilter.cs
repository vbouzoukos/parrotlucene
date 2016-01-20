/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Lucene.Net.Analysis.Tokenattributes;
using ArrayUtil = Lucene.Net.Util.ArrayUtil;

namespace Lucene.Net.Analysis
{

    /// <summary> This class converts alphabetic, numeric, and symbolic Unicode characters
    /// which are not in the first 127 ASCII characters (the "Basic Latin" Unicode
    /// block) into their ASCII equivalents, if one exists.
    /// 
    /// Characters from the following Unicode blocks are converted; however, only
    /// those characters with reasonable ASCII alternatives are converted:
    /// 
    /// <list type="bullet">
    /// <item>C1 Controls and Latin-1 Supplement: <a href="http://www.unicode.org/charts/PDF/U0080.pdf">http://www.unicode.org/charts/PDF/U0080.pdf</a></item>
    /// <item>Latin Extended-A: <a href="http://www.unicode.org/charts/PDF/U0100.pdf">http://www.unicode.org/charts/PDF/U0100.pdf</a></item>
    /// <item>Latin Extended-B: <a href="http://www.unicode.org/charts/PDF/U0180.pdf">http://www.unicode.org/charts/PDF/U0180.pdf</a></item>
    /// <item>Latin Extended Additional: <a href="http://www.unicode.org/charts/PDF/U1E00.pdf">http://www.unicode.org/charts/PDF/U1E00.pdf</a></item>
    /// <item>Latin Extended-C: <a href="http://www.unicode.org/charts/PDF/U2C60.pdf">http://www.unicode.org/charts/PDF/U2C60.pdf</a></item>
    /// <item>Latin Extended-D: <a href="http://www.unicode.org/charts/PDF/UA720.pdf">http://www.unicode.org/charts/PDF/UA720.pdf</a></item>
    /// <item>IPA Extensions: <a href="http://www.unicode.org/charts/PDF/U0250.pdf">http://www.unicode.org/charts/PDF/U0250.pdf</a></item>
    /// <item>Phonetic Extensions: <a href="http://www.unicode.org/charts/PDF/U1D00.pdf">http://www.unicode.org/charts/PDF/U1D00.pdf</a></item>
    /// <item>Phonetic Extensions Supplement: <a href="http://www.unicode.org/charts/PDF/U1D80.pdf">http://www.unicode.org/charts/PDF/U1D80.pdf</a></item>
    /// <item>General Punctuation: <a href="http://www.unicode.org/charts/PDF/U2000.pdf">http://www.unicode.org/charts/PDF/U2000.pdf</a></item>
    /// <item>Superscripts and Subscripts: <a href="http://www.unicode.org/charts/PDF/U2070.pdf">http://www.unicode.org/charts/PDF/U2070.pdf</a></item>
    /// <item>Enclosed Alphanumerics: <a href="http://www.unicode.org/charts/PDF/U2460.pdf">http://www.unicode.org/charts/PDF/U2460.pdf</a></item>
    /// <item>Dingbats: <a href="http://www.unicode.org/charts/PDF/U2700.pdf">http://www.unicode.org/charts/PDF/U2700.pdf</a></item>
    /// <item>Supplemental Punctuation: <a href="http://www.unicode.org/charts/PDF/U2E00.pdf">http://www.unicode.org/charts/PDF/U2E00.pdf</a></item>
    /// <item>Alphabetic Presentation Forms: <a href="http://www.unicode.org/charts/PDF/UFB00.pdf">http://www.unicode.org/charts/PDF/UFB00.pdf</a></item>
    /// <item>Halfwidth and Fullwidth Forms: <a href="http://www.unicode.org/charts/PDF/UFF00.pdf">http://www.unicode.org/charts/PDF/UFF00.pdf</a></item>
    /// </list>
    /// 
    /// See: <a href="http://en.wikipedia.org/wiki/Latin_characters_in_Unicode">http://en.wikipedia.org/wiki/Latin_characters_in_Unicode</a>
    /// 
    /// The set of character conversions supported by this class is a superset of
    /// those supported by Lucene's <see cref="ISOLatin1AccentFilter" /> which strips
    /// accents from Latin1 characters.  For example, '&#192;' will be replaced by
    /// 'a'.
    /// </summary>
    public sealed class GreekAccentFilter : TokenFilter
    {
        public GreekAccentFilter(TokenStream input) : base(input)
        {
            termAtt = AddAttribute<ITermAttribute>();
        }

        private char[] output = new char[512];
        private int outputPos;
        private ITermAttribute termAtt;

        public override bool IncrementToken()
        {
            if (input.IncrementToken())
            {
                char[] buffer = termAtt.TermBuffer();
                int length = termAtt.TermLength();

                // If no characters actually require rewriting then we
                // just return token as-is:
                for (int i = 0; i < length; ++i)
                {
                    char c = buffer[i];
                    if (c >= '\u0080')
                    {
                        FoldToASCII(buffer, length);
                        termAtt.SetTermBuffer(output, 0, outputPos);
                        break;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary> Converts characters above ASCII to their ASCII equivalents.  For example,
        /// accents are removed from accented characters.
        /// </summary>
        /// <param name="input">The string to fold
        /// </param>
        /// <param name="length">The number of characters in the input string
        /// </param>
        public void FoldToASCII(char[] input, int length)
        {
            // Worst-case length required:
            int maxSizeNeeded = 4 * length;
            if (output.Length < maxSizeNeeded)
            {
                output = new char[ArrayUtil.GetNextSize(maxSizeNeeded)];
            }

            outputPos = 0;

            for (int pos = 0; pos < length; ++pos)
            {
                char c = input[pos];

                // Quick test: if it's not in range then just keep current character
                if (c < '\u0080')
                {
                    output[outputPos++] = c;
                }
                else
                {
                    switch (c)
                    {

                        //        private static readonly string smallcase = "άέήίόύώςΐϊϋΰ";

                        case 'ά':
                            output[outputPos++] = 'α';
                            break;
                        case 'Ά':
                            output[outputPos++] = 'Α';
                            break;
                        case 'έ':
                            output[outputPos++] = 'ε';
                            break;
                        case 'Έ':
                            output[outputPos++] = 'Ε';
                            break;
                        case 'ή':
                            output[outputPos++] = 'η';
                            break;
                        case 'Ή':
                            output[outputPos++] = 'Η';
                            break;
                        case 'ί':
                            output[outputPos++] = 'ι';
                            break;
                        case 'Ί':
                            output[outputPos++] = 'Ι';
                            break;
                        case 'ΐ':
                            output[outputPos++] = 'ι';
                            break;
                        case 'ϊ':
                            output[outputPos++] = 'ι';
                            break;
                        case 'Ϊ':
                            output[outputPos++] = 'Ι';
                            break;
                        case 'ό':
                            output[outputPos++] = 'ο';
                            break;
                        case 'Ό':
                            output[outputPos++] = 'Ο';
                            break;
                        case 'ύ':
                            output[outputPos++] = 'υ';
                            break;
                        case 'Ύ':
                            output[outputPos++] = 'Υ';
                            break;
                        case 'ΰ':
                            output[outputPos++] = 'υ';
                            break;
                        case 'ϋ':
                            output[outputPos++] = 'υ';
                            break;
                        case 'Ϋ':
                            output[outputPos++] = 'Υ';
                            break;
                        case 'ώ':
                            output[outputPos++] = 'ω';
                            break;
                        case 'Ώ':
                            output[outputPos++] = 'Ω';
                            break;
                        default:
                            output[outputPos++] = c;
                            break;

                    }
                }
            }
        }
    }
}
