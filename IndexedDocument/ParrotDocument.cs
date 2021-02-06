using Newtonsoft.Json;
using ParrotLucene.Base;
using ParrotLucene.Indexing.Metadata;
using ParrotLucene.Utils;
using System;
using System.Globalization;

namespace ParrotLucene.IndexedDocument
{
    /// <summary>
    /// Abstract class of an indexed document
    /// </summary>
    public abstract class ParrotDocument
    {
        ParrotId _entityId;// = ParrotId.New;

        /// <summary>
        /// The document unique Id
        /// </summary>
        [LuceneField("entityId")]
        public ParrotId EntityId
        {
            get
            {
                if (_entityId.IsEmpty)
                {
                    _entityId = ParrotId.New;
                }
                return _entityId;
            }
            set { _entityId = value; }
        }

        /// <summary>
        /// When we have an InArea search clause the distance is stored into this attribute
        /// </summary>
        [LuceneNoIndex]
        public double? Distance { get; set; }

        /// <summary>
        /// Converts the object value into a text or spatial data suitable for storing into the lucene index
        /// Override in order to include custom attributes
        /// </summary>
        /// <param name="value">The object value</param>
        /// <returns>The converted data of the object value</returns>
        public virtual ParrotConvertor LuceneConvert(object value)
        {
            string converted = "";
            double? phi = null;
            double? lamda = null;
            if (value != null)
            {
                if (value.GetType() == typeof(DateTime))
                {
                    converted = ParrotBeak.DateSerialize((DateTime)value);
                }
                else if (value.GetType() == typeof(ParrotId))
                {
                    converted = ((ParrotId)value).ToString(CultureInfo.InvariantCulture);
                }
                else if (value.GetType() == typeof(string))
                {
                    converted = Convert.ToString(value);
                }
                //else if (value.GetType() == typeof(decimal))
                //{
                //    converted = ParrotBeak.DecimalSerialize(((decimal)value));
                //}
                else if (value.GetType() == typeof(float)
                    || value.GetType() == typeof(double)
                    || value.GetType() == typeof(short)
                    || value.GetType() == typeof(int)
                    || value.GetType() == typeof(byte)
                    || value.GetType() == typeof(long)
                    || value.GetType() == typeof(ushort)
                    || value.GetType() == typeof(uint)
                    || value.GetType() == typeof(ulong)
                    || value.GetType() == typeof(decimal))
                {
                    converted = null;// ((ulong)value).ToString(CultureInfo.InvariantCulture;
                }
                else if (value.GetType() == typeof(GeoCoordinate))
                {
                    var point = (GeoCoordinate)value;
                    phi = point.Latitude;
                    lamda = point.Longitude;
                }
                else
                {
                    //unknown data in order to be analysed user should override
                    converted = JsonConvert.SerializeObject(value);
                }
            }
            return new ParrotConvertor()
            {
                Conversion = converted,
                Lamda = lamda,
                Phi = phi
            };
        }
    }
}
