namespace Cethleann.Structure
{
    /// <summary>
    ///     Info regarding compression for a file in DATA1
    /// </summary>
    public struct CompressionInfo
    {
        /// <summary>
        /// </summary>
        public DataType Magic { get; set; }

        /// <summary>
        ///     Number of chunks
        /// </summary>
        public int ChunkCount { get; set; }

        /// <summary>
        ///     Compressed (read: bytes to read in this file) size of this file.
        /// </summary>
        public int Size { get; set; }
    }
}
