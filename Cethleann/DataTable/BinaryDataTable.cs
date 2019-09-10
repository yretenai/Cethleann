using Cethleann.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Cethleann.DataTable
{
    public class BinaryDataTable : IDataTable
    {
        public List<byte[]> Data { get; set; } = new List<byte[]>();

        public BinaryDataTable(Span<byte> buffer)
        {
            var header = MemoryMarshal.Read<BinaryDataTableHeader>(buffer);
            for (int i = 0; i < header.Count; ++i) Data.Add(buffer.Slice(0x40 + i * header.Size, header.Size).ToArray());
        }

        public List<T> Cast<T>() where T : struct
        {
            return Data.Select(x => MemoryMarshal.Read<T>(new ReadOnlySpan<byte>(x))).ToList();
        }
    }
}