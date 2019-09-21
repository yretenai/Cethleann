using Cethleann.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Cethleann.DataTables
{
    /// <summary>
    /// Main blob bundle of the game.
    /// Can contain many things, from structures to art assets.
    /// </summary>
    public class DataTable
    {
        /// <summary>
        /// Lsof Entries found in the table
        /// </summary>
        public IEnumerable<Memory<byte>> Entries;

        /// <summary>
        /// Initialize with a binary stream.
        /// </summary>
        /// <param name="stream"></param>
        public DataTable(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            var count = MemoryMarshal.Read<int>(buffer);
            buffer = stackalloc byte[8 * count];
            stream.Read(buffer);
            var tableInfo = MemoryMarshal.Cast<byte, DataTableRecord>(buffer);
            var entries = new List<Memory<byte>>();
            foreach (var info in tableInfo)
            {
                buffer = new Span<byte>(new byte[info.Size]);
                stream.Position = info.Offset;
                stream.Read(buffer);
                entries.Add(buffer.ToArray());
            }
            Entries = entries;
        }
        
        /// <summary>
        /// Initialize with a span.
        /// </summary>
        /// <param name="buffer"></param>
        public DataTable(Span<byte> buffer)
        {
            var count = MemoryMarshal.Read<int>(buffer);
            var tableInfo = MemoryMarshal.Cast<byte, DataTableRecord>(buffer.Slice(4, 8 * count));
            var entries = new List<Memory<byte>>();
            foreach (var info in tableInfo)
            {
                entries.Add(buffer.Slice(info.Offset, info.Size).ToArray());
            }
            Entries = entries;
        }
    }
}
