using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Parser for NGCDSP ADPCM Streams
    /// </summary>
    public class GCADPCMSound : ISoundResourceSection
    {
        /// <summary>
        ///     Initialize from buffer
        /// </summary>
        /// <param name="blob"></param>
        public GCADPCMSound(Span<byte> blob)
        {
            FullBuffer = new Memory<byte>(blob.ToArray());
            Header = MemoryMarshal.Read<GCADPCMSoundHeader>(blob);
            Table = MemoryMarshal.Cast<byte, GCADPCMSoundInfo>(blob.Slice(Header.ADPCMPointer, Header.ADPCMSize)).ToArray();
            for (var i = 0; i < Header.Streams; ++i)
            {
                var pair = MemoryMarshal.Cast<byte, int>(blob.Slice(Header.AudioTablePointer + i * 8, 8));
                var size = pair[1] - pair[0];
                AudioBuffers.Add(size == 0 ? Memory<byte>.Empty : new Memory<byte>(blob.Slice(pair[0], size).ToArray()));
            }
        }

        /// <summary>
        ///     GCADPCM Header
        /// </summary>
        public GCADPCMSoundHeader Header { get; set; }

        /// <summary>
        ///     GCADPCM Info Tables
        /// </summary>
        public GCADPCMSoundInfo[] Table { get; set; }

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
            if (Header.Streams == 0) return new List<Memory<byte>>();

            if (Header.Streams == 1)
                return new List<Memory<byte>>
                {
                    FullBuffer
                };

            var header = Header;
            header.Streams = 1;
            header.Unknown2 = -1;
            header.Unknown3 = 0;
            header.ADPCMSize = SizeHelper.SizeOf<GCADPCMSoundInfo>();
            header.ADPCMPointer = 0x38;
            header.AudioTablePointer = 0x98;
            header.UnknownPointer = 0x9C;

            var buffers = new List<Memory<byte>>();
            for (var index = 0; index < AudioBuffers.Count; index++)
            {
                var buffer = AudioBuffers[index];
                var data = new Memory<byte>(new byte[(0xA0 + buffer.Length).Align(0x10)]);
                var @base = header.Base;
                @base.Size = data.Length;
                header.Base = @base;
                MemoryMarshal.Write(data.Span, ref header);
                var info = Table[index];
                MemoryMarshal.Write(data.Span.Slice(header.ADPCMPointer), ref info);
                BinaryPrimitives.WriteUInt32LittleEndian(data.Span.Slice(header.AudioTablePointer), 0xA0);
                BinaryPrimitives.WriteUInt32LittleEndian(data.Span.Slice(header.AudioTablePointer + 4), (uint) buffer.Length);
                buffer.CopyTo(data.Slice(0xA0));
                buffers.Add(data);
            }

            return buffers;
        }
    }
}
