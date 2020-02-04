using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.G1
{
    /// <summary>
    ///     Parser for G1L Files
    /// </summary>
    [PublicAPI]
    public class G1Lazy
    {
        /// <summary>
        ///     Initialize with Span buffer
        /// </summary>
        /// <param name="buffer"></param>
        public G1Lazy(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<LazyContainer>(buffer);
            Logger.Assert(Header.Unknown1 == 0, "Header.Unknown1 == 0");
            Logger.Assert(Header.Unknown2 == 1, "Header.Unknown2 == 1");
            Audio = new Memory<byte>(buffer.Slice(Header.Pointer).ToArray());
        }

        /// <summary>
        ///     G1L Header
        /// </summary>
        public LazyContainer Header { get; set; }

        /// <summary>
        ///     G1L Audio Data
        /// </summary>
        public Memory<byte> Audio { get; set; }
    }
}
