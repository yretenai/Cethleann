namespace Cethleann.Structure
{
    /// <summary>
    ///     Info regarding compression for a file in DATA1
    /// </summary>
    public struct KTGLCompressionInfo
    {
        /// <summary>
        ///     usually 0x0001_0000, sometimes 0xFFFF_FFFF for dynamically sized.
        /// </summary>
        public int ChunkSize { get; set; }

        /// <summary>
        ///     Number of chunks
        /// </summary>
        public int ChunkCount { get; set; }

        /// <summary>
        ///     Compressed (read: bytes to read in this file) size of this file.
        /// </summary>
        public uint Size { get; set; }
    }
}
