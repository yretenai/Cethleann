namespace Cethleann.Structure.Archive
{
    /// <summary>
    ///     An entry in DATA0
    /// </summary>
    public struct DATA0Entry
    {
        /// <summary>
        ///     Offset in DATA1
        /// </summary>
        public long Offset { get; set; }

        /// <summary>
        ///     Uncompressed (read: bytes to expect from decompression) size of this file.
        /// </summary>
        public long UncompressedSize { get; set; }

        /// <summary>
        ///     Compressed (read: bytes to read in this file) size of this file.
        /// </summary>
        public long CompressedSize { get; set; }

        /// <summary>
        ///     True if the file is compressed with ZLIB
        /// </summary>
        public bool IsCompressed { get; set; }
    }
}
