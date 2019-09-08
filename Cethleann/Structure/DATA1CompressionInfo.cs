using System.Runtime.InteropServices;

namespace Cethleann.Structure
{
    /// <summary>
    /// Info regarding compression for a file in DATA1
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 12)]
    public struct DATA1CompressionInfo
    {
        /// <summary>
        /// Always 0x10000, I assumed ChunkSize but it doesn't match up.
        /// </summary>
        public int Unknown { get; set; }
        /// <summary>
        /// Number of chunks
        /// </summary>
        public int ChunkCount { get; set; }
        /// <summary>
        /// Compressed (read: bytes to read in this file) size of this file.
        /// Don't know why this exist, maybe anonymous file pointers?
        /// </summary>
        public int Size { get; set; }
    }
}
