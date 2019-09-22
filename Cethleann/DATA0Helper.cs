using Cethleann.DataTables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cethleann
{
    public static class DATA0Helper
    {
        private static readonly uint[] dataTypes = Enum.GetValues(typeof(DataType)).Cast<uint>().ToArray();

        /// <summary>
        /// Guesses the format based on the magic value.
        /// </summary>
        /// <param name="data">data to test</param>
        /// <returns>data type, null if magic isn't known</returns>
        public static unsafe DataType? GuessType(Stream data)
        {
            Span<byte> buffer = stackalloc byte[4];
            data.Read(buffer);
            data.Position -= 4;
            return GuessType(buffer);
        }

        /// <summary>
        /// Guesses the format based on the magic value.
        /// </summary>
        /// <param name="buffer">data to test</param>
        /// <returns>data type, null if magic isn't known</returns>
        public static DataType? GuessType(Span<byte> buffer)
        {
            var magic = MemoryMarshal.Read<uint>(buffer);
            if (dataTypes.Contains(magic)) return (DataType)magic;
            return null;
        }

        /// <summary>
        /// Guesses if the stream is a DataTable
        /// </summary>
        /// <param name="data">data to test</param>
        /// <returns>true if the header is predictable</returns>
        public static unsafe bool GuessDataTable(Stream data)
        {
            Span<byte> buffer = stackalloc byte[8];
            data.Read(buffer);
            data.Position -= 8;
            return GuessDataTable(buffer);
        }

        /// <summary>
        /// Guesses if the stream is a DataTable
        /// </summary>
        /// <param name="buffer">data to test</param>
        /// <returns>true if the header is predictable</returns>
        public static bool GuessDataTable(Span<byte> buffer)
        {
            var count = MemoryMarshal.Read<uint>(buffer);
            var firstOffset = MemoryMarshal.Read<uint>(buffer.Slice(4));
            return firstOffset == 4 + count * 8;
        }

        public static TextLocalization[][] GetTextLocalizationsRoot(DataTable table)
        {
            var locs = new List<TextLocalization[]>();
            foreach(var entry in table.Entries)
            {
                var entryTable = new DataTable(entry.Span);
                locs.Add(GetTextLocalizations(entryTable));
            }
            return locs.ToArray();
        }

        public static TextLocalization[] GetTextLocalizations(DataTable table)
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
