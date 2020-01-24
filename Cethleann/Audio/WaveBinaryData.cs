using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;
using Cethleann.Structure.WHD;
using DragonLib;
using DragonLib.Audio;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Binary WAVE Data
    /// </summary>
    public class WaveBinaryData
    {
        /// <summary>
        ///     Initialize with buffer
        /// </summary>
        /// <param name="buffer"></param>
        public WaveBinaryData(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<ResourceSectionHeader>(buffer);
            Data = new Memory<byte>(buffer.Slice(SizeHelper.SizeOf<ResourceSectionHeader>()).ToArray());
        }

        /// <summary>
        ///     Very Large, Very Chonky WAVE stream
        /// </summary>
        public Memory<byte> Data { get; set; }

        /// <summary>
        ///     Underlying header
        /// </summary>
        public ResourceSectionHeader Header { get; set; }

        /// <summary>
        ///     Reconstruct a stream from WBH data.
        /// </summary>
        /// <param name="entry"></param>
        /// <returns></returns>
        public Memory<byte> ReconstructWave(WBHEntry entry)
        {
            return PCM.ConstructWAVE((short) entry.Codec, 1, entry.Frequency, entry.BlockAlign, entry.Codec switch
            {
                WBHCodec.PCM => 16,
                WBHCodec.MSADPCM => 4,
                _ => 8
            }, Data.Span.Slice(entry.Offset, entry.Size));
        }
    }
}
