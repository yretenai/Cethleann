using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Graphics
{
    /// <summary>
    ///     Parser for G1L Files
    /// </summary>
    [PublicAPI]
    public class G1L
    {
        /// <summary>
        ///     Initialize with Span buffer
        /// </summary>
        /// <param name="buffer"></param>
        public G1L(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<LazyContainer>(buffer);
            Logger.Assert(Header.Unknown1 == 0, "Header.Unknown1 == 0");
            Logger.Assert(Header.Unknown2 == 1, "Header.Unknown2 == 1");
            Buffer = new Memory<byte>(buffer.Slice(Header.Pointer).ToArray());
        }

        /// <summary>
        ///     Header
        /// </summary>
        public LazyContainer Header { get; set; }

        /// <summary>
        ///     Data
        /// </summary>
        public Memory<byte> Buffer { get; set; }
    }
}
