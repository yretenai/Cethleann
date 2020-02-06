namespace Cethleann.Structure
{
    public struct RDBEntry
    {
        public DataType Magic { get; set; }
        public int Version { get; set; }
        public int EntrySize { get; set; }
        public int Reserved1 { get; set; }
        public int ContentSize { get; set; }
        public int Reserved2 { get; set; }
        public int Size { get; set; }
        public int Type { get; set; }
        public int FileId { get; set; }
        public int TypeId { get; set; }
        public RDBFlags Flags { get; set; }
    }

    public enum RDBFlags
    {
        External = 0x00010000,
        Internal = 0x00020000,
        Compressed = 0x00100000
    }
}
