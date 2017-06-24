using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Security.Cryptography;
using System.Globalization;

namespace LuceneSearchEngine
{
    public class Utility
    {
        public static string GenerateUniqueID
        {
            get
            {
                int maxSize = 8;
                char[] chars = new char[62];
                string a;
                a = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
                chars = a.ToCharArray();
                int size = maxSize;
                byte[] data = new byte[1];
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                crypto.GetNonZeroBytes(data);
                size = maxSize;
                data = new byte[size];
                crypto.GetNonZeroBytes(data);
                StringBuilder result = new StringBuilder(size);
                foreach (byte b in data)
                {
                    result.Append(chars[b % (chars.Length - 1)]);
                }
                return result.ToString();
            }

        }
        public static int StringToInt(string source)
        {
            int ret = 0;
            int.TryParse(source, out ret);
            return ret;
        }
        public static float StringToFloat(string source)
        {
            float ret = 0;
            float.TryParse(source, out ret);
            return ret;
        }
        public static double StringToDouble(string source)
        {
            double ret = 0;
            double.TryParse(source, out ret);
            return ret;
        }
        public static string DateSerialize(DateTime dateTime)
        {
            return dateTime.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }

        public static DateTime DateDeserialize(string str)
        {
            return DateTime.ParseExact(str, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }
        public static int Random(int from, int to)
        {
            Random r = new Random();
            return r.Next(from, to); //for ints
            //int range = 100;
            //double rDouble = r.NextDouble() * range; //for doubles
        }
        public static int PagesCount(int itemsperpage, int totalCount)
        {
            int Pages = (int)Math.Truncate((double)totalCount / (double)itemsperpage);
            if (totalCount % itemsperpage > 0)
                Pages++;
            return Pages;
        }
    }
}
