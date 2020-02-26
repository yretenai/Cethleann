using System;

namespace Cethleann.Structure.KTID
{
    public struct KTIDReference : IFormattable
    {
        public uint KTID { get; set; }

        public static implicit operator uint(KTIDReference reference)
        {
            return reference.KTID;
        }

        public static implicit operator KTIDReference(uint reference)
        {
            return new KTIDReference
            {
                KTID = reference
            };
        }

        public string ToString(string? format, IFormatProvider? formatProvider) => KTID.ToString(format, formatProvider);

        public string ToString(string? format) => KTID.ToString(format);

        public override string ToString() => KTID.ToString("x8");
    }
}
