using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
using DragonLib.IO;

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
            Header = MemoryMarshal.Read<GCADPCMSoundHeader>(blob);
            var table = new ushort[Header.Channels][];
            Logger.Assert(Header.Channels == 1, "Header.Channels == 1");
            for (var i = 0; i < Header.Channels; ++i) table[i] = MemoryMarshal.Cast<byte, ushort>(blob.Slice(SizeHelper.SizeOf<GCADPCMSoundHeader>() + 0x20 * i, 0x20)).ToArray();
            Table = table;
            Unknown = MemoryMarshal.Read<int>(blob.Slice(Header.UnknownPointer));
            FullBuffer = new Memory<byte>(blob.ToArray());
            var size = MemoryMarshal.Read<int>(blob.Slice(Header.AudioPointer));
            AudioBuffer = new Memory<byte>(blob.Slice(Header.AudioPointer + 4, size).ToArray());
        }

        /// <summary>
        ///     GCADPCM Header
        /// </summary>
        public GCADPCMSoundHeader Header { get; set; }

        /// <summary>
        ///     GCADPCM Coefficient Table
        /// </summary>
        public ushort[][] Table { get; set; }

        /// <summary>
        ///     Unknown Value, usually 0xA0.
        /// </summary>
        public int Unknown { get; set; }

        /// <summary>
        ///     Full sound buffer
        /// </summary>
        public Memory<byte> FullBuffer { get; set; }

        /// <summary>
        ///     Full sound buffer
        /// </summary>
        public Memory<byte> AudioBuffer { get; set; }

        /// <inheritdoc />
        public SoundResourceEntry Base => Header.Base;
    }
}
