using System;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;

namespace Cethleann
{
    /// <summary>
    ///     Series of helper for FETH.
    /// </summary>
    public static class Extensions
    {
        static Extensions() => DataTypeHelper.Preload<DataType>();

        /// <summary>
        ///     Converts an integer to a FourCC
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onlyAlphaNum"></param>
        /// <returns></returns>
        public static string ToFourCC(this int value, bool onlyAlphaNum) => string.Join("", BitConverter.GetBytes(value).Select(x => x >= 48 && x <= 122 ? ((char) x).ToString() : onlyAlphaNum ? "" : $@"\x{x:X2}"));

        /// <summary>
        ///     Converts a DataType to a FourCC
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onlyAlphaNum"></param>
        /// <returns></returns>
        public static string ToFourCC(this DataType value, bool onlyAlphaNum) => ((int) value).ToFourCC(onlyAlphaNum);

        /// <summary>
        ///     Converts a version tag to a number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToVersion(this int value) => int.Parse(value.ToFourCC(false).Reverse().ToArray());

        /// <summary>
        ///     Converts a version tag to a number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToVersionI(this int value)
        {
            value -= 0x30303030;
            return (value & 0xFF) + (((value >> 8) & 0xFF) * 10) + (((value >> 16) & 0xFF) * 100) + (((value >> 24) & 0xFF) * 1000);
        }

        /// <summary>
        ///     Converts a version tag to a number.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ToVersionA(this int value)
        {
            var v = 0x30303030;
            v += ((int) Math.Floor((value % 10000) / 1000d)) << 24;
            v += ((int) Math.Floor((value % 1000) / 100d)) << 16;
            v += ((int) Math.Floor((value % 100) / 10d)) << 8;
            v += value % 10;
            return v;
        }

        /// <summary>
        ///     Returns determined string extension for this magic.
        /// </summary>
        /// <param name="magic"></param>
        /// <returns></returns>
        public static string GetExtension(this DataType magic) => DataTypeHelper.GetExtension(magic);

        /// <summary>
        ///     True if the magic values are known
        /// </summary>
        /// <param name="magic"></param>
        /// <returns></returns>
        public static bool IsKnown(this DataType magic) => magic != DataType.None && DataTypeHelper.IsKnown(magic);

        /// <summary>
        ///     True if the magic values are known
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static bool IsKnown(this Span<byte> span) => span.GetDataType().IsKnown();

        /// <summary>
        ///     True if the magic value matches the first 4 bytes of the span.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="magic"></param>
        /// <returns></returns>
        public static bool Matches(this Span<byte> span, DataType magic) => DataTypeHelper.Matches(span, (int) magic);

        /// <summary>
        ///     True if the magic value matches the first 4 bytes of the span.
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public static bool Matches(this DataType magic, Span<byte> span) => DataTypeHelper.Matches(span, (int) magic);

        /// <summary>
        ///     Gets <seealso cref="DataType" /> from a Span.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static DataType GetDataType(this Span<byte> span) => DataTypeHelper.GetMagicValue<DataType>(span);

        /// <summary>
        ///     Guesses if the stream is a DataTable
        /// </summary>
        /// H:\Datamining\FireEmblemTH\Cethleann\DragonLib\DataTypeHelper.cs
        /// <param name="buffer">data to test</param>
        /// <returns>true if the header is predictable</returns>
        public static bool IsDataTable(this Span<byte> buffer)
        {
            if (buffer.Length < 8) return false;

            var count = MemoryMarshal.Read<uint>(buffer);
            var firstOffset = MemoryMarshal.Read<uint>(buffer.Slice(4));
            var estimatedOffset = 4 + count * 8;
            return firstOffset == estimatedOffset || firstOffset == estimatedOffset.Align(0x10);
        }

        /// <summary>
        ///     Guesses if the stream is a valid bundle.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool IsBundle(this Span<byte> buffer)
        {
            return Bundle.Validate(buffer) != null;
        }

        /// <summary>
        ///     Guesses if the stream is a valid bundle.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static bool IsPointerBundle(this Span<byte> buffer)
        {
            return PointerBundle.Validate(buffer) != null;
        }

        /// <summary>
        ///     Guesses if the stream is a valid dds bundle.
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        // TODO: Actually parse this shit. 
        public static bool IsDDSBundle(this Span<byte> buffer)
        {
            if (buffer.Length < 0x100) return false;
            return buffer.Slice(0xC0).GetDataType() == DataType.DDS || buffer.Slice(0x40).GetDataType() == DataType.DDS;
        }
    }
}
