using System;
using System.Globalization;

namespace Cethleann.Structure.KTID
{
    public struct KTIDReference : IComparable, IConvertible, IFormattable, IComparable<uint>, IEquatable<uint>, IComparable<KTIDReference>, IEquatable<KTIDReference>
    {
        public uint KTID { get; set; }

        public static implicit operator uint(KTIDReference reference) => reference.KTID;

        public static implicit operator KTIDReference(uint reference) =>
            new KTIDReference
            {
                KTID = reference
            };

        public KTIDReference(uint ktid) => KTID = ktid;

        public KTIDReference(object? obj)
        {
            KTID = 0;
            if (obj != null && obj is uint ktid) KTID = ktid;
        }

        public string ToString(string? format, IFormatProvider? formatProvider) => KTID.ToString(format, formatProvider);

        public string ToString(string? format) => KTID.ToString(format);

        public int CompareTo(uint other) => KTID.CompareTo(other);

        public bool Equals(uint other) => KTID.Equals(other);

        public int CompareTo(KTIDReference other) => KTID.CompareTo(other.KTID);

        public bool Equals(KTIDReference other) => KTID.Equals(other.KTID);

        public override string ToString() => KTID.ToString("x8");
        public int CompareTo(object? obj) => KTID.CompareTo(obj);
        public TypeCode GetTypeCode() => KTID.GetTypeCode();

        public bool ToBoolean(IFormatProvider? provider) => ((IConvertible) KTID).ToBoolean(provider);

        public byte ToByte(IFormatProvider? provider) => ((IConvertible) KTID).ToByte(provider);

        public char ToChar(IFormatProvider? provider) => ((IConvertible) KTID).ToChar(provider);

        public DateTime ToDateTime(IFormatProvider? provider) => ((IConvertible) KTID).ToDateTime(provider);

        public decimal ToDecimal(IFormatProvider? provider) => ((IConvertible) KTID).ToDecimal(provider);

        public double ToDouble(IFormatProvider? provider) => ((IConvertible) KTID).ToDouble(provider);

        public short ToInt16(IFormatProvider? provider) => ((IConvertible) KTID).ToInt16(provider);

        public int ToInt32(IFormatProvider? provider) => ((IConvertible) KTID).ToInt32(provider);

        public long ToInt64(IFormatProvider? provider) => ((IConvertible) KTID).ToInt64(provider);

        public sbyte ToSByte(IFormatProvider? provider) => ((IConvertible) KTID).ToSByte(provider);

        public float ToSingle(IFormatProvider? provider) => ((IConvertible) KTID).ToSingle(provider);

        public string ToString(IFormatProvider? provider) => KTID.ToString(provider);

        public object ToType(Type conversionType, IFormatProvider? provider) => ((IConvertible) KTID).ToType(conversionType, provider);

        public ushort ToUInt16(IFormatProvider? provider) => ((IConvertible) KTID).ToUInt16(provider);

        public uint ToUInt32(IFormatProvider? provider) => ((IConvertible) KTID).ToUInt32(provider);

        public ulong ToUInt64(IFormatProvider? provider) => ((IConvertible) KTID).ToUInt64(provider);

        public static bool TryParse(string? value, NumberStyles style, IFormatProvider? provider, out KTIDReference ktid)
        {
            if (uint.TryParse(value, style, provider, out var result))
            {
                ktid = result;
                return true;
            }

            ktid = 0;
            return false;
        }

        public static bool TryParse(string? value, out KTIDReference ktid)
        {
            if (uint.TryParse(value, NumberStyles.HexNumber, null, out var result))
            {
                ktid = result;
                return true;
            }

            ktid = 0;
            return false;
        }

        public static KTIDReference Parse(string value, NumberStyles style, IFormatProvider? provider = null) => uint.Parse(value, style, provider);

        public static KTIDReference Parse(string value) => uint.Parse(value);
    }
}
