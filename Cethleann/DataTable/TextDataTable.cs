using Cethleann.Structure;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Cethleann.DataTable
{
    public class TextDataTable : IDataTable
    {
        public List<byte[]> Data { get; set; } = new List<byte[]>();
        public TextDataTable(Span<byte> buffer)
        {
            var count = MemoryMarshal.Read<int>(buffer);
            var tableData = MemoryMarshal.Cast<byte, TextDataTableHeader>(buffer.Slice(4, count * 8));
            foreach (var tableRecord in tableData) Data.Add(buffer.Slice(tableRecord.Offset, tableRecord.Size).ToArray());
        }
    }
}