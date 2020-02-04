namespace Cethleann.Structure
{
    public struct LFMArchiveEntry
    {
        public long Offset { get; set; }
        public long CompressedSize { get; set; }
        public long Size { get; set; }
        public long IsCompressed { get; set; }
    }
}
