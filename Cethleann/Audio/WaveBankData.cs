using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;
using Cethleann.Structure.Resource.Audio.WHD;
using DragonLib;
using DragonLib.Audio;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Wave Bank Data
    /// </summary>
    [PublicAPI]
    public class WaveBankData
    {
        /// <summary>
        ///     Initialize with buffer
        /// </summary>
        /// <param name="buffer"></param>
        public WaveBankData(Span<byte> buffer)
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
        /// <param name="convertToPcm"></param>
        /// <returns></returns>
        public Memory<byte> ReconstructWave(WBHEntry entry, bool convertToPcm)
        {
            var data = Data.Span.Slice(entry.Offset, entry.Size);
            var entryMutate = entry;
            // ReSharper disable once InvertIf
            if (convertToPcm && entry.Codec != WAVECodec.PCM && entry.Channels == 1)
            {
                data = entry.Codec switch
                {
                    WAVECodec.MSADPCM => MSADPCM.Decode(data, entryMutate.BlockAlign),
                    WAVECodec.GCADPCM when entry.Setup is short[] coefficients => GCADPCM.Decode(data, coefficients, entry.Samples),
                    _ => data
                };

                entryMutate.BlockAlign = 2;
                entryMutate.Codec = WAVECodec.PCM;
            }

            return PCM.ConstructWAVE((short) entryMutate.Codec, entry.Channels, entryMutate.Frequency, entryMutate.BlockAlign, entryMutate.Codec switch
            {
                WAVECodec.PCM => 16,
                WAVECodec.MSADPCM => 4,
                WAVECodec.GCADPCM => 4,
                _ => 4
            }, data);
        }
    }
}
