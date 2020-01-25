using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using Cethleann.Structure.WHD;
using DragonLib;
using DragonLib.Audio;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Parser for MS ADPCM Streams
    /// </summary>
    public class MSADPCMSound : ISoundResourceSection
    {
        /// <summary>
        ///     Initialize from buffer
        /// </summary>
        /// <param name="blob"></param>
        public MSADPCMSound(Span<byte> blob)
        {
            FullBuffer = new Memory<byte>(blob.ToArray());
            Header = MemoryMarshal.Read<ADPCMSoundHeader>(blob);
            Info = MemoryMarshal.Read<MSADPCMSoundInfo>(blob.Slice(Header.ADPCMPointer, Header.ADPCMSize));
            var pointers = MemoryMarshal.Cast<byte, int>(blob.Slice(Header.PointerTablePointer, 4 * Header.Streams));
            var sizes = MemoryMarshal.Cast<byte, int>(blob.Slice(Header.SizeTablePointer, 4 * Header.Streams));
            for (var i = 0; i < Header.Streams; ++i) AudioBuffers.Add(new Memory<byte>(blob.Slice(pointers[i], sizes[i]).ToArray()));
        }

        /// <summary>
        ///     ADPCM Header
        /// </summary>
        public ADPCMSoundHeader Header { get; set; }

        /// <summary>
        ///     ADPCM Setup Info
        /// </summary>
        public MSADPCMSoundInfo Info { get; set; }

        /// <summary>
        ///     Full sound buffers
        /// </summary>
        public List<Memory<byte>> AudioBuffers { get; set; } = new List<Memory<byte>>();

        /// <summary>
        ///     Full sound buffer
        /// </summary>
        public Memory<byte> FullBuffer { get; set; }

        /// <inheritdoc />
        public SoundResourceEntry Base => Header.Base;

        /// <summary>
        ///     Rebuild multi streams as individual streams
        /// </summary>
        /// <returns></returns>
        public List<Memory<byte>> ReconstructAsIndividual()
        {
            switch (Header.Streams)
            {
                case 0:
                    return new List<Memory<byte>>();
                case 1:
                    return new List<Memory<byte>>
                    {
                        FullBuffer
                    };
            }

            var header = Header;
            header.Streams = 1;
            header.Unknown2 = -1;
            header.Unknown3 = 0;
            header.ADPCMSize = 4;
            header.ADPCMPointer = 0x38;
            header.PointerTablePointer = 0x3C;
            header.SizeTablePointer = 0x40;

            var buffers = new List<Memory<byte>>();
            foreach (var buffer in AudioBuffers)
            {
                var data = new Memory<byte>(new byte[(0x50 + buffer.Length).Align(0x10)]);
                var @base = header.Base;
                @base.Size = data.Length;
                header.Base = @base;
                MemoryMarshal.Write(data.Span, ref header);
                BinaryPrimitives.WriteInt32LittleEndian(data.Span.Slice(header.ADPCMPointer), Header.SampleRate);
                BinaryPrimitives.WriteUInt32LittleEndian(data.Span.Slice(header.PointerTablePointer), 0x50);
                BinaryPrimitives.WriteUInt32LittleEndian(data.Span.Slice(header.SizeTablePointer), (uint) buffer.Length);
                buffer.CopyTo(data.Slice(0x50));
                buffers.Add(data);
            }

            return buffers;
        }

        /// <summary>
        ///     Reconstructs stream to a WAV
        /// </summary>
        /// <returns></returns>
        public List<Memory<byte>> ReconstructWave(bool convertPcm)
        {
            var streams = new List<Memory<byte>>();
            foreach (var buffer in AudioBuffers)
            {
                var data = buffer.Span;
                var codec = 0x0002;
                var blockAlign = Info.FrameSize;
                if (convertPcm)
                {
                    data = MSADPCM.Decode(data, blockAlign);
                    codec = 0x0001;
                    blockAlign = 0x2;
                }


                streams.Add(new Memory<byte>(PCM.ConstructWAVE((short) codec, 1, Header.SampleRate, blockAlign, (WAVECodec) codec switch
                {
                    WAVECodec.PCM => 16,
                    WAVECodec.MSADPCM => 4,
                    WAVECodec.GCADPCM => 4,
                    _ => 4
                }, data).ToArray()));
            }

            return streams;
        }
    }
}
