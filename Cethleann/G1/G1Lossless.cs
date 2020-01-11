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
    public class G1Lossless
    {
        /// <summary>
        ///     Initialize with Span buffer
        /// </summary>
        /// <param name="buffer"></param>
        public G1Lossless(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<LosslessAudioContainer>(buffer);
            Logger.Assert(Header.Unknown1 == 0, "Header.Unknown1 == 0");
            Logger.Assert(Header.Unknown2 == 1, "Header.Unknown2 == 1");
            Audio = new Memory<byte>(buffer.Slice(Header.SoundPointer).ToArray());
        }

        /// <summary>
        ///     G1L Header
        /// </summary>
        public LosslessAudioContainer Header { get; set; }

        /// <summary>
        ///     G1L Audio Data
        /// </summary>
        public Memory<byte> Audio { get; set; }
    }
}
