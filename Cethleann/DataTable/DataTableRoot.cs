using Cethleann.Structure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Cethleann.DataTable
{
    public class DataTableRoot
    {
        public IEnumerable<IDataTable> Tables;

        private const int BINARY_TABLE_MAGIC = 0x16121900;

        public DataTableRoot(Stream stream)
        {
            Span<byte> buffer = stackalloc byte[4];
            stream.Read(buffer);
            var count = MemoryMarshal.Read<int>(buffer);
            buffer = stackalloc byte[8 * count];
            stream.Read(buffer);
            var tableInfo = MemoryMarshal.Cast<byte, DataTableInfo>(buffer);
            var tables = new List<IDataTable>();
            foreach (var info in tableInfo)
            {
                buffer = new Span<byte>(new byte[info.Size]);
                stream.Position = info.Offset;
                stream.Read(buffer);
                if (MemoryMarshal.Read<int>(buffer) == BINARY_TABLE_MAGIC) tables.Add(new BinaryDataTable(buffer));
                else tables.Add(new TextDataTable(buffer));
            }
            Tables = tables;
        }
    }
}
