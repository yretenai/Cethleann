namespace Cethleann.Structure.Archive
{
    /// <summary>
    ///     All assumed
    /// </summary>
    public struct INFO1Entry
    {
        public long UncompressedSize { get; set; }
        public long CompressedSize { get; set; }
        public long IsCompressed { get; set; }
    }
}
