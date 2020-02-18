namespace Cethleann.Structure
{
    public struct BTIFEntry
    {
        public uint Checksum { get; set; }
        public uint UnknownIndex { get; set; }
        public uint Id { get; set; }
        public int NameOffset { get; set; }
        public IDTableFlags Flags { get; set; }
        public uint DecompressedSize { get; set; }
        public uint CompressedSize { get; set; }
        public uint TypeHash { get; set; }
        public uint Version { get; set; }
    }
}
