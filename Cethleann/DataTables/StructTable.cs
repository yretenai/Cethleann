using Cethleann.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cethleann.DataTables
{
    /// <summary>
    /// Parses a memory buffer by assuming it consists of fixed width structs.
    /// </summary>
    public class StructTable
    {
        /// <summary>
        /// Lsof Entries found in the struct table
        /// </summary>
        public List<Memory<byte>> Entries { get; set; } = new List<Memory<byte>>();

        /// <summary>
        /// Initialize with given buffer
        /// </summary>
        /// <param name="buffer"></param>
        public StructTable(Span<byte> buffer)
        {
            var header = MemoryMarshal.Read<StructTableHeader>(buffer);
            for (int i = 0; i < header.Count; ++i) Entries.Add(buffer.Slice(0x40 + i * header.Size, header.Size).ToArray());
        }

        /// <summary>
        /// Cast all entries to the specified struct type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> Cast<T>() where T : struct
        {
            return Entries.Select(x => MemoryMarshal.Read<T>(x.Span)).ToList();
        }
    }
}