using Lucene.Net.Analysis;
using Lucene.Net.Analysis.TokenAttributes;
using ArrayUtil = ParrotLucene.Utils.ArrayUtils;

namespace ParrotLucene.Analyzers.Filter
{
    public sealed class GreekAccentFilter : TokenFilter
    {
        public GreekAccentFilter(TokenStream input) : base(input)
        {
            termAtt = AddAttribute<ICharTermAttribute>();
        }
        public char[] output = new char[512];
        private int outputPos;
        private readonly ICharTermAttribute termAtt;

        public override bool IncrementToken()
        {
            if (m_input.IncrementToken())
            {
                char[] buffer = termAtt.Buffer;
                int length = termAtt.Length;

                // If no characters actually require rewriting then we
                // just return token as-is:
                for (int i = 0; i < length; ++i)
                {
                    char c = buffer[i];
                    if (c >= 'Ά')
                    {
                        Transform(buffer, length);
                        termAtt.Buffer.SetValue(output, 0, outputPos);
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

        /// <summary> Converts characters with tones.  For example,
        /// accents are removed from accented characters.
        /// </summary>
        /// <param name="input">The string to fold
        /// </param>
        /// <param name="length">The number of characters in the input string
        /// </param>
        public void Transform(char[] input, int length)
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
                if (c < 'Ά')
                {
                    output[outputPos++] = c;
                }
                else
                {
                    switch (c)
                    {
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
                        case 'ΐ':
                        case 'ϊ':
                            output[outputPos++] = 'ι';
                            break;
                        case 'Ί':
                        case 'Ϊ':
                            output[outputPos++] = 'Ι';
                            break;
                        case 'ό':
                            output[outputPos++] = 'ο';
                            break;
                        case 'Ό':
                            output[outputPos++] = 'Ο';
                            break;
                        case 'Ύ':
                        case 'Ϋ':
                            output[outputPos++] = 'Υ';
                            break;
                        case 'ύ':
                        case 'ΰ':
                        case 'ϋ':
                            output[outputPos++] = 'υ';
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
