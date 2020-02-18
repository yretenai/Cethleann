using System;
using DragonLib;

namespace Cethleann.Structure
{
    public struct IDTableEntry
    {
        public int PathOffset { get; set; }
        public int OriginalPathOffset { get; set; }
        public IDTableFlags Flags { get; set; }
        public uint Checksum { get; set; }
        public uint CompressedSize { get; set; }
        public uint DecompressedSize { get; set; }
        public uint Padding1 { get; set; }
        public uint Padding2 { get; set; }

        public string Path(Memory<byte> table, int offset)
        {
            return table.Span.Slice(offset + PathOffset).ReadString();
        }

        public string OriginalPath(Memory<byte> table, int offset)
        {
            return table.Span.Slice(offset + OriginalPathOffset).ReadString();
        }
    }
}
