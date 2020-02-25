namespace Cethleann.Structure.Archive
{
    public struct INFO0Entry
    {
        public long Index { get; set; }
        public long UncompressedSize { get; set; }
        public long CompressedSize { get; set; }
        public long IsCompressed { get; set; }
    }
}
