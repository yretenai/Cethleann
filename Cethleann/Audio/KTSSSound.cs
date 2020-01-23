using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     KTSS Sound Sample, or KOVS Encrypted Sound Sample
    /// </summary>
    [PublicAPI]
    public class KTSSSound
    {
        /// <summary>
        ///     Initialize from a buffer.
        /// </summary>
        /// <param name="buffer"></param>
        public KTSSSound(Span<byte> buffer)
        {
            FullBuffer = new Memory<byte>(buffer.ToArray());
            Header = MemoryMarshal.Read<KTSSHeader>(buffer);
            var offset = SizeHelper.SizeOf<KTSSHeader>().Align(0x20);
            var codec = MemoryMarshal.Read<KTSSCodec>(buffer.Slice(offset));

            Logger.Assert(codec == KTSSCodec.Opus, "codec == KTSSCodec.Opus", ":ktgodbrain:", $"codec byte is {codec:X}");
            // if (codec != KTSSCodec.Opus) return;
            CodecHeader = MemoryMarshal.Read<KTSSCodecHeader>(buffer.Slice(offset));
            offset += SizeHelper.SizeOf<KTSSCodecHeader>();
            ChannelFamily = buffer.Slice(offset, CodecHeader.ChannelCount).ToArray();
            offset = CodecHeader.AudioOffset;
            for (var i = 0; i < CodecHeader.FrameCount; ++i)
            {
                var blob = buffer.Slice(offset, CodecHeader.FrameSize);
                var frameHeader = blob.Slice(0, SizeHelper.SizeOf<KTSSSoundFrame>());
                frameHeader.Reverse();
                var frame = MemoryMarshal.Read<KTSSSoundFrame>(frameHeader);
                Logger.Assert(frame.Size == CodecHeader.FrameSize - 0x8, "frame.Size == CodecHeader.FrameSize - 0x8", $"{frame.Size} == {CodecHeader.FrameSize} - 8");
                Packets.Add((frame, new Memory<byte>(blob.Slice(SizeHelper.SizeOf<KTSSSoundFrame>()).ToArray())));
                offset += CodecHeader.FrameSize;
            }
        }

        /// <summary>
        ///     KTSS
        /// </summary>
        public KTSSHeader Header { get; set; }

        /// <summary>
        ///     KTSS Codec Header
        /// </summary>
        public KTSSCodecHeader CodecHeader { get; set; }

        /// <summary>
        ///     Channel info
        /// </summary>
        public byte[] ChannelFamily { get; set; }

        /// <summary>
        ///     Codec packets
        /// </summary>
        public List<(KTSSSoundFrame frame, Memory<byte> data)> Packets { get; set; } = new List<(KTSSSoundFrame Unknown, Memory<byte> data)>();

        /// <summary>
        ///     Full sound buffer
        /// </summary>
        public Memory<byte> FullBuffer { get; set; }
    }
}
