using System;
using System.Linq;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource;
using Cethleann.Structure.WAV;
using Cethleann.Structure.WHD;
using DragonLib;

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
        /// <param name="entries"></param>
        /// <returns></returns>
        public Memory<byte> ReconstructWave(WBHEntry[] entries)
        {
            var first = entries.First();
            var data = new Memory<byte>(new byte[44 + entries.Sum(x => x.Size)]);
            var header = new WAVEHeader
            {
                Magic = DataType.RIFF,
                Size = data.Length,
                Format = 0x45564157
            };

            var fmt = new WAVEFormat
            {
                Magic = 0x20746D66,
                Size = 16,
                Format = (short) first.Codec,
                Channels = (short) entries.Length,
                SampleRate = first.Frequency,
                BlockAlign = first.BlockAlign, // * entry.Length,
                BitsPerSample = first.Codec switch
                {
                    WBHCodec.MSADPCM => 4,
                    _ => 8
                }
            };
            fmt.ByteRate = fmt.SampleRate * fmt.Channels * fmt.BitsPerSample / 8;
            var dat = new WAVEData
            {
                Magic = 0x61746164,
                Size = data.Length - 44
            };

            MemoryMarshal.Write(data.Span, ref header);
            MemoryMarshal.Write(data.Span.Slice(12), ref fmt);
            MemoryMarshal.Write(data.Span.Slice(36), ref dat);

            var offset = 44;
            foreach (var entry in entries)
            {
                var buffer = Data.Span.Slice(entry.Offset, entry.Size);
                buffer.CopyTo(data.Span.Slice(offset));
                offset += buffer.Length;
            }

            return data;
        }
    }
}
