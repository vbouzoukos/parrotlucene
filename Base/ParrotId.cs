using ParrotLucene.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ParrotLucene.Base
{
    /// <summary>
    /// Unique identifier of lucene documents of the parrot lucene engine
    /// </summary>
    [Serializable]
    public struct ParrotId : IComparable<ParrotId>, IEquatable<ParrotId>, IConvertible
    {
        private readonly string _id;
        public ParrotId(Guid guid)
        {
            _id = guid.ToString("N");
        }
        public ParrotId(string value)
        {
            _id = value ?? throw new ArgumentNullException("value");
        }
        public ParrotId(byte[] bytes)
        {
            _id = System.Text.UTF8Encoding.Default.GetString(bytes);
        }
        public ParrotId(int value)
        {
            _id = value.ToString(CultureInfo.InvariantCulture);
        }
        public ParrotId(long value)
        {
            _id = value.ToString(CultureInfo.InvariantCulture);
        }

        public static ParrotId New
        {
            get { return new ParrotId(ParrotBeak.GenerateUniqueID); }
        }
        public static ParrotId NewFromGuid
        {
            get { return new ParrotId(Guid.NewGuid()); }
        }
        public static implicit operator string(ParrotId d) => d.ToString(CultureInfo.InvariantCulture);
        public static implicit operator ParrotId(Guid guid) => new ParrotId(guid);
        public static implicit operator ParrotId(string s) => new ParrotId(s);
        public static implicit operator ParrotId(int n) => new ParrotId(n);
        public static implicit operator ParrotId(long n) => new ParrotId(n);

        public override string ToString() => $"{_id}";
        public bool IsEmpty
        {
            get { return string.IsNullOrEmpty(_id); }
        }
        public int CompareTo(ParrotId other)
        {
            return this._id.CompareTo(other._id);
        }

        public bool Equals(ParrotId other)
        {
            return this._id.Equals(other._id);
        }

        public TypeCode GetTypeCode()
        {
            return TypeCode.Object;
        }

        public bool ToBoolean(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public byte ToByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public char ToChar(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public DateTime ToDateTime(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public decimal ToDecimal(IFormatProvider provider)
        {
            if (decimal.TryParse(_id, NumberStyles.Any, provider, out decimal result))
            {
                return result;
            }
            throw new InvalidCastException();
        }

        public double ToDouble(IFormatProvider provider)
        {
            if (double.TryParse(_id, NumberStyles.Any, provider, out double result))
            {
                return result;
            }
            throw new InvalidCastException();
        }

        public short ToInt16(IFormatProvider provider)
        {
            if (short.TryParse(_id, NumberStyles.Any, provider, out short result))
            {
                return result;
            }
            throw new InvalidCastException();
        }

        public int ToInt32(IFormatProvider provider)
        {
            if (int.TryParse(_id, NumberStyles.Any, provider, out int result))
            {
                return result;
            }
            throw new InvalidCastException();
        }

        public long ToInt64(IFormatProvider provider)
        {
            if (Int64.TryParse(_id, NumberStyles.Any, provider, out long result))
            {
                return result;
            }
            throw new InvalidCastException();
        }

        public sbyte ToSByte(IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public float ToSingle(IFormatProvider provider)
        {
            if (Single.TryParse(_id, NumberStyles.Any, provider, out float result))
            {
                return result;
            }
            throw new InvalidCastException();
        }

        public string ToString(IFormatProvider provider)
        {
            return _id?.ToString(provider);
        }

        public object ToType(Type conversionType, IFormatProvider provider)
        {
            throw new InvalidCastException();
        }

        public ushort ToUInt16(IFormatProvider provider)
        {
            if (ushort.TryParse(_id, NumberStyles.Any, provider, out ushort result))
            {
                return result;
            }
            throw new InvalidCastException();
        }

        public uint ToUInt32(IFormatProvider provider)
        {
            if (uint.TryParse(_id, NumberStyles.Any, provider, out uint result))
            {
                return result;
            }
            throw new InvalidCastException();
        }

        public ulong ToUInt64(IFormatProvider provider)
        {
            if (ulong.TryParse(_id, NumberStyles.Any, provider, out ulong result))
            {
                return result;
            }
            throw new InvalidCastException();
        }
    }
}
