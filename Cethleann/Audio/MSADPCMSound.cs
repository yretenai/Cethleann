using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Parser for MS ADPCM Streams (frame 0x16)
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
            SampleRate = MemoryMarshal.Read<int>(blob.Slice(Header.ADPCMPointer, Header.ADPCMSize));
            var pointers = MemoryMarshal.Cast<byte, int>(blob.Slice(Header.PointerTablePointer, 4 * Header.Channels));
            var sizes = MemoryMarshal.Cast<byte, int>(blob.Slice(Header.SizeTablePointer, 4 * Header.Channels));
            for (var i = 0; i < Header.Channels; ++i) AudioBuffers.Add(new Memory<byte>(blob.Slice(pointers[i], sizes[i]).ToArray()));
        }

        /// <summary>
        ///     ADPCM Header
        /// </summary>
        public ADPCMSoundHeader Header { get; set; }

        /// <summary>
        ///     Audio Sample Rate
        /// </summary>
        public int SampleRate { get; set; }

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
        public List<Memory<byte>> RebuildAsIndividual()
        {
            switch (Header.Channels)
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
            header.Channels = 1;
            header.Unknown2 = -1;
            header.Unknown3 = 0;
            header.ADPCMSize = 4;
            header.ADPCMPointer = 0x38;
            header.PointerTablePointer = 0x3C;
            header.SizeTablePointer = 0x40;

            var buffers = new List<Memory<byte>>();
            for (var index = 0; index < AudioBuffers.Count; index++)
            {
                var buffer = AudioBuffers[index];
                var data = new Memory<byte>(new byte[(0x50 + buffer.Length).Align(0x10)]);
                var @base = header.Base;
                @base.Size = data.Length;
                header.Base = @base;
                MemoryMarshal.Write(data.Span, ref header);
                BinaryPrimitives.WriteInt32LittleEndian(data.Span.Slice(header.ADPCMPointer), SampleRate);
                BinaryPrimitives.WriteUInt32LittleEndian(data.Span.Slice(header.PointerTablePointer), 0x50);
                BinaryPrimitives.WriteUInt32LittleEndian(data.Span.Slice(header.SizeTablePointer), (uint) buffer.Length);
                buffer.CopyTo(data.Slice(0x50));
                buffers.Add(data);
            }

            return buffers;
        }
    }
}
