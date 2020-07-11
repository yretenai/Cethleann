using System;
using DragonLib;

namespace Cethleann.Structure.Archive
{
    public struct IDTableEntry
    {
        public int PathOffset { get; set; }
        public int OriginalPathOffset { get; set; }
        public IDTableFlags Flags { get; set; }
        public uint UniqueId { get; set; } // Or path CRC32 checksum?
        public uint CompressedSize { get; set; }
        public uint DecompressedSize { get; set; }
        public uint Padding1 { get; set; }
        public uint Padding2 { get; set; }

        public string Path(Memory<byte> table, int offset) => table.Span.Slice(offset + PathOffset).ReadStringNonNull();

        public string OriginalPath(Memory<byte> table, int offset) => table.Span.Slice(offset + OriginalPathOffset).ReadStringNonNull();
    }
}
