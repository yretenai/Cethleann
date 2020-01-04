using Cethleann.Ninja;
using DragonLib;

namespace Cethleann.Structure
{
#pragma warning disable 1591
    public struct IDTableEntry
    {
        public int PathOffset { get; set; }
        public int OriginalPathOffset { get; set; }
        public IDTableFlags Flags { get; set; }
        public uint Checksum { get; set; }
        public uint CompressedSize { get; set; }
        public uint DecompressedSize { get; set; }
        public ulong Padding { get; set; }

        public string Path(IDTable table)
        {
            return table.Table.Span.Slice(table.Header.Offset + PathOffset).ReadString();
        }

        public string OriginalPath(IDTable table)
        {
            return table.Table.Span.Slice(table.Header.Offset + OriginalPathOffset).ReadString();
        }
    }
#pragma warning restore 1591
}
