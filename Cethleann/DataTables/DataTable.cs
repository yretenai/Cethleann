using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using DragonLib;

namespace Cethleann.DataTables
{
    /// <summary>
    ///     Main blob bundle of the game.
    ///     Can contain many things, from structures to art assets.
    /// </summary>
    public class DataTable
    {
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
                var block = buffer.Slice(info.Offset, info.Size);
                entries.Add(block.GetDataType() == DataType.Compressed ? DATA0.Decompress(block) : new Memory<byte>(block.ToArray()));
            }

            Entries = entries;
        }

        /// <summary>
        ///     Lsof Entries found in the table
        /// </summary>
        public List<Memory<byte>> Entries { get; }
    }
}
