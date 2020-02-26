using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.Table;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Tables
{
    /// <summary>
    ///     Main blob bundle of the game.
    ///     Can contain many things, from structures to art assets.
    /// </summary>
    [PublicAPI]
    public class DataTable
    {
        /// <summary>
        ///     Initialize with no data.
        /// </summary>
        public DataTable()
        {
        }

        /// <summary>
        ///     Initialize with a span.
        /// </summary>
        /// <param name="buffer"></param>
        public DataTable(Span<byte> buffer)
        {
            var count = MemoryMarshal.Read<int>(buffer);
            var tableInfo = MemoryMarshal.Cast<byte, DataTableRecord>(buffer.Slice(sizeof(int), SizeHelper.SizeOf<DataTableRecord>() * count));
            var entries = new List<Memory<byte>>();
            foreach (var info in tableInfo)
            {
                if (info.Offset >= buffer.Length) continue;
                var maxSize = Math.Min(info.Size, buffer.Length - info.Offset);
                var block = buffer.Slice(info.Offset, maxSize);
                entries.Add(new Memory<byte>(block.ToArray()));
            }

            Entries = entries;
        }

        /// <summary>
        ///     Lsof Entries found in the table
        /// </summary>
        public List<Memory<byte>> Entries { get; set; } = new List<Memory<byte>>();

        /// <summary>
        ///     Writes a data buffer for this structure.
        /// </summary>
        /// <returns></returns>
        public Span<byte> Write()
        {
            var baseLength = (4 + SizeHelper.SizeOf<DataTableRecord>() * Entries.Count).Align(0x10);
            var totalLength = baseLength + Entries.Sum(x => x.Length.Align(4));
            var table = new Span<byte>(new byte[totalLength]);

            var count = Entries.Count;
            MemoryMarshal.Write(table, ref count);

            var record = new DataTableRecord();
            var dataOffset = baseLength;
            for (int i = 0; i < Entries.Count; ++i)
            {
                record.Offset = dataOffset;
                record.Size = Entries[i].Length;
                MemoryMarshal.Write(table.Slice(4 + SizeHelper.SizeOf<DataTableRecord>() * i), ref record);
                Entries[i].Span.CopyTo(table.Slice(dataOffset));
                dataOffset += Entries[i].Length.Align(4);
            }

            return table;
        }
    }
}
