namespace Cethleann.Structure.Archive
{
    /// <summary>
    ///     An entry in DATA0, but tiny
    /// </summary>
    public struct TinyDATA0Entry
    {
        /// <summary>
        ///     Uncompressed (read: bytes to expect from decompression) size of this file.
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        ///     Offset in DATA1
        /// </summary>
        public int Offset { get; set; }
    }
}
