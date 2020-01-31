namespace Cethleann.Structure
{
    public struct LINKDATAEntry
    {
        public uint Offset { get; set; }
        public int Reserved { get; set; }
        public int Size { get; set; }
        public int DecompressedSize { get; set; }
    }
}
