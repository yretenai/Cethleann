namespace Cethleann.Structure
{
    public struct RDBEntry
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public long EntrySize { get; set; }
        public long ContentSize { get; set; }
        public long Size { get; set; }
        public int Type { get; set; }
        public uint FileId { get; set; }
        public uint TypeId { get; set; }
        public RDBFlags Flags { get; set; }
    }

    public enum RDBFlags
    {
        External = 0x00010000,
        Internal = 0x00020000,
        ZlibCompressed = 0x00100000,
        LZ77Compressed = 0x00200000
    }
}
