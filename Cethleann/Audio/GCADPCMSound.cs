using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
using DragonLib.Audio;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Parser for NGCDSP ADPCM Streams
    /// </summary>
    [PublicAPI]
    public class GCADPCMSound : ISoundResourceSection
    {
        /// <summary>
        ///     Initialize from buffer
        /// </summary>
        /// <param name="blob"></param>
        public GCADPCMSound(Span<byte> blob)
        {
            FullBuffer = new Memory<byte>(blob.ToArray());
            Header = MemoryMarshal.Read<ADPCMSoundHeader>(blob);
            Table = MemoryMarshal.Cast<byte, GCADPCMSoundInfo>(blob.Slice(Header.ADPCMPointer, Header.ADPCMSize)).ToArray();
            var pointers = MemoryMarshal.Cast<byte, int>(blob.Slice(Header.PointerTablePointer, 4 * Header.Streams));
            var sizes = MemoryMarshal.Cast<byte, int>(blob.Slice(Header.SizeTablePointer, 4 * Header.Streams));
            for (var i = 0; i < Header.Streams; ++i) AudioBuffers.Add(new Memory<byte>(blob.Slice(pointers[i], sizes[i]).ToArray()));
        }

        /// <summary>
        ///     GCADPCM Header
        /// </summary>
        public ADPCMSoundHeader Header { get; set; }

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
            header.ADPCMSize = SizeHelper.SizeOf<GCADPCMSoundInfo>();
            header.ADPCMPointer = 0x38;
            header.PointerTablePointer = 0x98;
            header.SizeTablePointer = 0x9C;

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
                BinaryPrimitives.WriteUInt32LittleEndian(data.Span.Slice(header.PointerTablePointer), 0xA0);
                BinaryPrimitives.WriteUInt32LittleEndian(data.Span.Slice(header.SizeTablePointer), (uint) buffer.Length);
                buffer.CopyTo(data.Slice(0xA0));
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
            // TODO: BFWAV?
            if (!convertPcm) return ReconstructAsIndividual();

            var streams = new List<Memory<byte>>();
            for (var index = 0; index < AudioBuffers.Count; index++)
            {
                var buffer = AudioBuffers[index];
                var coeffs = new Span<GCADPCMCoefficient>(new[] { Table[index].Coefficient1, Table[index].Coefficient2 });
                var data = buffer.Span;
                data = GCADPCM.Decode(data, MemoryMarshal.Cast<GCADPCMCoefficient, short>(coeffs).ToArray(), Table[index].SampleCount);

                streams.Add(new Memory<byte>(PCM.ConstructWAVE(0x0001, 1, Header.SampleRate, 0x2, 16, data).ToArray()));
            }

            return streams;
        }
    }
}
