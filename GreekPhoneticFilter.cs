using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Tokenattributes;
using ArrayUtil = Lucene.Net.Util.ArrayUtil;

namespace LuceneSearchEngine
{
    public class GreekPhoneticFilter : TokenFilter
    {
        public GreekPhoneticFilter(TokenStream input) : base(input)
        {
            termAtt = AddAttribute<ITermAttribute>();
        }
        public char[] output = new char[512];
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
                    if (c >= 'Ά')
                    {
                        Transform(buffer, length);
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

        bool IsLoud(char a,char b)
        {
            switch (a)
            {
                case 'α':
                case 'ά':
                case 'Α':
                case 'Ά':

                case 'ε':
                case 'έ':
                case 'Ε':
                case 'Έ':

                case 'η':
                case 'ή':
                case 'Η':
                case 'Ή':

                case 'ι':
                case 'ί':
                case 'ΐ':
                case 'ϊ':
                case 'Ι':
                case 'Ϊ':
                case 'Ί':

                case 'ο':
                case 'ό':
                case 'Ο':
                case 'Ό':

                case 'υ':
                case 'ύ':
                case 'ΰ':
                case 'ϋ':
                case 'Υ':
                case 'Ύ':
                case 'Ϋ':

                case 'ω':
                case 'ώ':
                case 'Ω':
                case 'Ώ':

                case 'γ':
                case 'Γ':
                case 'β':
                case 'Β':
                case 'δ':
                case 'Δ':
                case 'ζ':
                case 'Ζ':
                case 'λ':
                case 'Λ':
                case 'μ':
                case 'Μ':
                case 'ν':
                case 'Ν':
                case 'ρ':
                case 'Ρ':
                    return true;
                case 'τ':
                case 'Τ':
                    if (b == 'ζ' || b == 'Ζ')
                        return true;
                    else
                        return false;
            }
            return false;
        }
        /// <summary> Converts characters above ASCII to their ASCII equivalents.  For example,
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
                char next = (char)0;
                char fnext = (char)0;
                char fnext2 = (char)0;
                if (pos + 1 < length)
                {
                    next = input[pos + 1];
                }
                if (pos + 2 < length)
                {
                    fnext = input[pos + 2];
                }
                if (pos + 3 < length)
                {
                    fnext2 = input[pos + 3];
                }
                // Quick test: if it's not in range then just keep current character
                if (c < 'Ά')
                {
                    output[outputPos++] = c;
                }
                else
                {
                    switch (c)
                    {
                        /*
                        phoenetic similar αι -> α
                        */
                        case 'α':
                        case 'Α':
                            if (next == 'υ'  || next == 'ύ' )
                            {
                                output[outputPos++] = c;
                                pos++;
                                if (IsLoud(fnext, fnext2))
                                    output[outputPos++] = 'β';
                                else
                                    output[outputPos++] = 'φ';
                            }
                            else if (next == 'Y' || next == 'Ύ')
                            {
                                output[outputPos++] = c;
                                pos++;
                                if (IsLoud(fnext, fnext2))
                                    output[outputPos++] = 'Β';
                                else
                                    output[outputPos++] = 'Φ';
                            }
                            else if (next == 'ι')
                            {
                                output[outputPos++] = 'ε';
                                pos++;
                            }
                            else if (next == 'ί')
                            {
                                output[outputPos++] = 'έ';
                                pos++;
                            }
                            else if (next == 'Ι')
                            {
                                output[outputPos++] = 'Ε';
                                pos++;
                            }
                            else if (next == 'Ί')
                            {
                                output[outputPos++] = 'Έ';
                                pos++;
                            }
                            else
                                output[outputPos++] = c;
                            break;
                        case 'ε':
                        case 'Ε':
                            if (next == 'υ' || next == 'ύ')
                            {
                                output[outputPos++] = c;
                                pos++;
                                if (IsLoud(fnext, fnext2))
                                    output[outputPos++] = 'β';
                                else
                                    output[outputPos++] = 'φ';
                            }
                            else if (next == 'Y' || next == 'Ύ')
                            {
                                output[outputPos++] = c;
                                pos++;
                                if (IsLoud(fnext, fnext2))
                                    output[outputPos++] = 'Β';
                                else
                                    output[outputPos++] = 'Φ';
                            }
                            else if (next == 'ι')
                            {
                                output[outputPos++] = 'ι';
                                pos++;
                            }
                            else if (next == 'ί')
                            {
                                output[outputPos++] = 'ί';
                                pos++;
                            }
                            else if (next == 'Ι')
                            {
                                output[outputPos++] = 'Ι';
                                pos++;
                            }
                            else if (next == 'Ί')
                            {
                                output[outputPos++] = 'Ί';
                                pos++;
                            }
                            else
                                output[outputPos++] = c;
                            break;
                        case 'η':
                            output[outputPos++] = 'ι';
                            break;
                        case 'ή':
                            output[outputPos++] = 'ί';
                            break;
                        case 'Η':
                            output[outputPos++] = 'Ι';
                            break;
                        case 'Ή':
                            output[outputPos++] = 'Ί';
                            break;
                        case 'ο':
                        case 'Ο':
                            if (next == 'ι')
                            {
                                output[outputPos++] = 'ι';
                                pos++;
                            }
                            else if (next == 'ί')
                            {
                                output[outputPos++] = 'ί';
                                pos++;
                            }
                            else if (next == 'Ι')
                            {
                                output[outputPos++] = 'Ι';
                                pos++;
                            }
                            else if (next == 'Ί')
                            {
                                output[outputPos++] = 'Ί';
                                pos++;
                            }
                            else
                                output[outputPos++] = c;
                            break;
                        case 'υ':
                            if (next == 'ι')
                            {
                                output[outputPos++] = 'ι';
                                pos++;
                            }
                            else if (next == 'ί')
                            {
                                output[outputPos++] = 'ί';
                                pos++;
                            }
                            else if (next == 'Ι')
                            {
                                output[outputPos++] = 'Ι';
                                pos++;
                            }
                            else if (next == 'Ί')
                            {
                                output[outputPos++] = 'Ί';
                                pos++;
                            }
                            else
                                output[outputPos++] = 'ι';
                            break;
                        case 'ύ':
                            output[outputPos++] = 'ί';
                            break;
                        case 'Υ':
                            if (next == 'ι')
                            {
                                output[outputPos++] = 'ι';
                                pos++;
                            }
                            else if (next == 'ί')
                            {
                                output[outputPos++] = 'ί';
                                pos++;
                            }
                            else if (next == 'Ι')
                            {
                                output[outputPos++] = 'Ι';
                                pos++;
                            }
                            else if (next == 'Ί')
                            {
                                output[outputPos++] = 'Ί';
                                pos++;
                            }
                            else
                                output[outputPos++] = 'Ι';
                            break;
                        case 'Ύ':
                            output[outputPos++] = 'Ί';
                            break;
                        case 'ω':
                            output[outputPos++] = 'ο';
                            break;
                        case 'ώ':
                            output[outputPos++] = 'ό';
                            break;
                        case 'Ω':
                            output[outputPos++] = 'Ο';
                            break;
                        case 'Ώ':
                            output[outputPos++] = 'Ό';
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
