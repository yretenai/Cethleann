using Cethleann.KTID;
using Cethleann.Structure.KTID;
using DragonLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Cethleann.Archive
{
    // why
    public class RDX
    {
        public RDXEntry[] PortableEntries { get; }
        public List<RDBEntry> Entries { get; } = new List<RDBEntry>();
        public Dictionary<KTIDReference, int> KTIDToEntryId { get; set; } = new Dictionary<KTIDReference, int>();

        public RDX(Span<byte> buffer, string name, string directory)
        {;
            RDXDirectory = directory;
            var rdbBuffer = File.ReadAllBytes(Path.Combine(directory, name + ".rdb")).AsSpan();
            Header = MemoryMarshal.Read<RDBHeader>(rdbBuffer);
            DataDirectory = rdbBuffer.Slice(SizeHelper.SizeOf<RDBHeader>(), Header.HeaderSize - SizeHelper.SizeOf<RDBHeader>()).ReadString() ?? string.Empty;
            Directory = Path.Combine(directory, DataDirectory);
            PortableEntries = MemoryMarshal.Cast<byte, RDXEntry>(buffer).ToArray();

            if (!KTIDToEntryId.TryGetValue(Header.NameDatabaseKTID, out var nameDatabaseId)) return;
            var nameBuffer = ReadEntry(nameDatabaseId);
            if (nameBuffer.Length == 0) return;
            NameDatabase = new NDB(nameBuffer.Span);
        }

        public Memory<byte> ReadEntry(int index)
        {
            throw new NotImplementedException();
        }

        public string RDXDirectory { get; set; }
        public string Directory { get; set; }
        public string DataDirectory { get; set; }
        public string Name { get; set; }
        public RDBHeader Header { get; set; }
        public RDBINFO NameDatabase { get; set; } = new NDB();
    }
}
