using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ParrotLucene.Utils
{
    /// <summary>
    /// Utility class useful like the beak is to the parrot
    /// </summary>
    public class ParrotBeak
    {
        /// <summary>
        /// Unique Id generator used into ParrotId
        /// </summary>
        public static string GenerateUniqueID
        {
            get
            {
                int maxSize = 8;
                string a;
                a = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                char[] chars = a.ToCharArray();
                byte[] data = new byte[1];
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                crypto.GetNonZeroBytes(data);
                int size = maxSize;
                data = new byte[size];
                crypto.GetNonZeroBytes(data);
                StringBuilder result = new StringBuilder(size);
                foreach (byte b in data)
                {
                    result.Append(chars[b % (chars.Length - 1)]);
                }
                return result.ToString().ToLower();
            }
        }
        /// <summary>
        /// Converts a string to byte
        /// </summary>
        /// <param name="source">string containing the byte data </param>
        /// <returns>Converted byte value</returns>
        public static byte StringToByte(string source)
        {
            byte.TryParse(source, out byte ret);
            return ret;
        }
        /// <summary>
        /// Converts a string to a short integer
        /// </summary>
        /// <param name="source">string containing short integer</param>
        /// <returns>Converted short integer value</returns>
        public static short StringToShort(string source)
        {
            short.TryParse(source, out short ret);
            return ret;
        }

        /// <summary>
        /// Converts a string to an integer
        /// </summary>
        /// <param name="source">string containing integer</param>
        /// <returns>Converted integer value</returns>
        public static int StringToInt(string source)
        {
            int.TryParse(source, out int ret);
            return ret;
        }

        /// <summary>
        /// Converts a string to a long integer
        /// </summary>
        /// <param name="source">string containing long integer</param>
        /// <returns>Converted long integer value</returns>
        public static long StringToLong(string source)
        {
            long.TryParse(source, out long ret);
            return ret;
        }

        /// <summary>
        /// Converts a string to a unsigned short integer
        /// </summary>
        /// <param name="source">string containing unsigned short integer</param>
        /// <returns>Converted unsigned short integer value</returns>
        public static ushort StringToUShort(string source)
        {
            ushort.TryParse(source, out ushort ret);
            return ret;
        }

        /// <summary>
        /// Converts a string to a unsigned integer
        /// </summary>
        /// <param name="source">string containing unsigned integer</param>
        /// <returns>Converted unsigned integer value</returns>
        public static uint StringToUInt(string source)
        {
            uint.TryParse(source, out uint ret);
            return ret;
        }

        /// <summary>
        /// Converts a string to a unsigned long integer
        /// </summary>
        /// <param name="source">string containing unsigned long integer</param>
        /// <returns>Converted unsigned long integer value</returns>
        public static ulong StringToULong(string source)
        {
            ulong.TryParse(source, out ulong ret);
            return ret;
        }

        /// <summary>
        /// Converts a string to a float
        /// </summary>
        /// <param name="source">string containing float</param>
        /// <returns>Converted float value</returns>
        public static float StringToFloat(string source)
        {
            float.TryParse(source, out float ret);
            return ret;
        }

        /// <summary>
        /// Converts a string to a double
        /// </summary>
        /// <param name="source">string containing double</param>
        /// <returns>Converted double value</returns>
        public static double StringToDouble(string source)
        {
            double.TryParse(source, out double ret);
            return ret;
        }

        /// <summary>
        /// Converts a string to a decimal
        /// </summary>
        /// <param name="source">string containing decimal</param>
        /// <returns>Converted decimal value</returns>
        public static decimal StringToDecimal(string source)
        {
            decimal.TryParse(source, out decimal ret);
            return ret;
        }
        /// <summary>
        /// Date serialized into a lucene format
        /// </summary>
        /// <param name="dateTime">DateTime to be converted</param>
        /// <returns>string with the converted date</returns>
        public static string DateSerialize(DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }
        /// <summary>
        /// Converts a serialized datetime string to a DateTime
        /// </summary>
        /// <param name="str">String containing date value</param>
        /// <returns>DateTime</returns>
        public static DateTime DateDeserialize(string str)
        {
            return DateTime.ParseExact(str, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        //public static string DecimalSerialize(decimal src)
        //{
        //    if (src > 0)
        //    { return $"1{src.ToString(new CultureInfo("en")).PadLeft(30, '0').PadRight(30, '0')}"; }
        //    else
        //    {
        //        return $"0{(src*(-1)).ToString(new CultureInfo("en")).PadLeft(30, '0').PadRight(30, '0')}";
        //    }
        //}
        //public static decimal DecimalDeserialize(string src)
        //{
        //    if (!string.IsNullOrEmpty(src) && src.Length > 1)
        //    {
        //        string parse=src;
        //        if (src[0] == '1')//positive
        //        {
        //            parse = src.Substring(1);
        //        }
        //        else if (src[0] == '1')//minus
        //        {
        //            parse = $"-{src.Substring(1)}";
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //        return StringToDecimal(parse);
        //    }
        //    return 0;
        //}

        /// <summary>
        /// Generates a random number between from-to range
        /// </summary>
        /// <param name="from">Starting value of the range we want to get the random number</param>
        /// <param name="to">Ending value of the range we want to get the random number</param>
        /// <returns>Random number in range [from,to]</returns>
        public static int Random(int from, int to)
        {
            Random r = new Random(DateTime.Now.Millisecond);
            return r.Next(from, to);
        }
        /// <summary>
        /// Calculates total pages for a totalCount number when a page has maximum of itemsperpage
        /// </summary>
        /// <param name="itemsperpage">Items per page</param>
        /// <param name="totalCount">Total number of items</param>
        /// <returns>Pages count</returns>
        public static int PagesCount(int itemsperpage, int totalCount)
        {
            int Pages = (int)Math.Truncate((double)totalCount / (double)itemsperpage);
            if (totalCount % itemsperpage > 0)
                Pages++;
            return Pages;
        }
    }
}
