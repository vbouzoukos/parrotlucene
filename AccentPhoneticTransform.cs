using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArrayUtil = Lucene.Net.Util.ArrayUtil;

namespace LuceneSearchEngine
{
    public class AccentPhoneticTransform
    {
        public static string Transform(string source)
        {
            int maxSizeNeeded = 4 * source.Length;

            char[] poutput = new char[ArrayUtil.GetNextSize(maxSizeNeeded)];

            int len=PhoneticTransform(poutput, source.ToCharArray(), source.Length);
            char[] aoutput = new char[ArrayUtil.GetNextSize(maxSizeNeeded)];
            len=Accent(aoutput, poutput, len);
            return new string(aoutput, 0,len).ToLower();
        }

        static bool  IsLoud(char a, char b)
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

        public static int PhoneticTransform(char[] output,char[] input, int length)
        {
            // Worst-case length required:
            int outputPos = 0;

            for (int pos = 0; pos < length; ++pos)
            {
                char c = input[pos];

                if (c == 0)
                    break;

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
            return outputPos;
        }
        public static int Accent(char[] output,char[] input, int length)
        {
            // Worst-case length required:
            int outputPos = 0;

            for (int pos = 0; pos < length; ++pos)
            {
                char c = input[pos];
                if (c == 0)
                    break;

                // Quick test: if it's not in range then just keep current character
                if (c < 'Ά')
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
            return outputPos;
        }

    }
}
