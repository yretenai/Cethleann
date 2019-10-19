using Cethleann.DataTables;
using DragonLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cethleann
{
    /// <summary>
    /// Series of helper for FETH.
    /// </summary>
    public static class DATA0Helper
    {
        static DATA0Helper()
        {
            DataTypeHelper.Preload<DataType>();
        }

        /// <summary>
        /// Returns determined string extension for this magic.
        /// </summary>
        /// <param name="magic"></param>
        /// <returns></returns>
        public static string GetExtension(this DataType magic)
        {
            return DataTypeHelper.GetExtension(magic);
        }

        /// <summary>
        /// True if the magic values are known
        /// </summary>
        /// <param name="magic"></param>
        /// <returns></returns>
        public static bool IsKnown(this DataType magic)
        {
            return DataTypeHelper.IsKnown<DataType>(magic);
        }

        /// <summary>
        /// True if the magic values are known
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static bool IsKnown(this Span<byte> span)
        {
            return span.GetDataType().IsKnown();
        }

        /// <summary>
        /// True if the magic value matches the first 4 bytes of the span.
        /// </summary>
        /// <param name="span"></param>
        /// <param name="magic"></param>
        /// <returns></returns>
        public static bool Matches(this Span<byte> span, DataType magic)
        {
            return DataTypeHelper.Matches(span, (int)magic);
        }

        /// <summary>
        /// True if the magic value matches the first 4 bytes of the span.
        /// </summary>
        /// <param name="magic"></param>
        /// <param name="span"></param>
        /// <returns></returns>
        public static bool Matches(this DataType magic, Span<byte> span)
        {
            return DataTypeHelper.Matches(span, (int)magic);
        }

        /// <summary>
        /// Gets <seealso cref="DataType"/> from a Span.
        /// </summary>
        /// <param name="span"></param>
        /// <returns></returns>
        public static DataType GetDataType(this Span<byte> span)
        {
            return DataTypeHelper.GetMagicValue<DataType>(span);
        }

        /// <summary>
        /// Guesses if the stream is a DataTable
        /// </summary>H:\Datamining\FireEmblemTH\Cethleann\DragonLib\DataTypeHelper.cs
        /// <param name="buffer">data to test</param>
        /// <returns>true if the header is predictable</returns>
        public static bool IsDataTable(this Span<byte> buffer)
        {
            if (buffer.Length < 8) return false;
            var count = MemoryMarshal.Read<uint>(buffer);
            var firstOffset = MemoryMarshal.Read<uint>(buffer.Slice(4));
            return firstOffset == 4 + count * 8;
        }

        /// <summary>
        /// Gets two dimensional text localizations from a data table.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static TextLocalization[][] GetTextLocalizationsRoot(this DataTable table)
        {
            var locs = new List<TextLocalization[]>();
            foreach(var entry in table.Entries)
            {
                var entryTable = new DataTable(entry.Span);
                locs.Add(GetTextLocalizations(entryTable));
            }
            return locs.ToArray();
        }

        /// <summary>
        /// Gets Text Localizations from table entries.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static TextLocalization[] GetTextLocalizations(this DataTable table)
        {
            var locs = new List<TextLocalization>();
            foreach (var entry in table.Entries)
            {
                locs.Add(new TextLocalization(entry.Span));
            }
            return locs.ToArray();
        }
    }
}
