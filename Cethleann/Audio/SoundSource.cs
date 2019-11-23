using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Koei.Structure.Resource.Audio;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Koei.Audio
{
    /// <summary>
    /// KTSS Sound Sample, or KOVS Encrypted Sound Sample
    /// </summary>
    [PublicAPI]
    public class SoundSource
    {
        /// <summary>
        /// Initialize from a buffer.
        /// </summary>
        /// <param name="buffer"></param>
        public SoundSource(Span<byte> buffer)
        {
            FullBuffer = new Memory<byte>(buffer.ToArray());
            Header = MemoryMarshal.Read<SoundSampleHeader>(buffer);
            var offset = SizeHelper.SizeOf<SoundSampleHeader>().Align(0x20);
            var codec = MemoryMarshal.Read<SoundSampleCodec>(buffer.Slice(offset));

            Logger.Assert(codec == SoundSampleCodec.Opus, "codec == KTSSCodec.Opus", ":ktgodbrain:", $"codec byte is {codec:X}");
            // if (codec != KTSSCodec.Opus) return;
            CodecHeader = MemoryMarshal.Read<SoundSampleCodecHeader>(buffer.Slice(offset));
            offset += SizeHelper.SizeOf<SoundSampleCodecHeader>();
            ChannelFamily = buffer.Slice(offset, CodecHeader.ChannelCount).ToArray();
            offset = CodecHeader.AudioOffset;
            for (var i = 0; i < CodecHeader.FrameCount; ++i)
            {
                var blob = buffer.Slice(offset, CodecHeader.FrameSize);
                var frameHeader = blob.Slice(0, SizeHelper.SizeOf<SoundSampleFrame>());
                frameHeader.Reverse();
                var frame = MemoryMarshal.Read<SoundSampleFrame>(frameHeader);
                Logger.Assert(frame.Size == CodecHeader.FrameSize - 0x8, "frame.Size == CodecHeader.FrameSize - 0x8", $"{frame.Size} == {CodecHeader.FrameSize} - 8");
                Packets.Add((frame, new Memory<byte>(blob.Slice(SizeHelper.SizeOf<SoundSampleFrame>()).ToArray())));
                offset += CodecHeader.FrameSize;
            }
        }

        /// <summary>
        /// KTSS
        /// </summary>
        public SoundSampleHeader Header { get; set; }

        /// <summary>
        /// KTSS Codec Header
        /// </summary>
        public SoundSampleCodecHeader CodecHeader { get; set; }

        /// <summary>
        /// Channel info
        /// </summary>
        public byte[] ChannelFamily { get; set; }

        /// <summary>
        /// Codec packets
        /// </summary>
        public List<(SoundSampleFrame frame, Memory<byte> data)> Packets { get; set; } = new List<(SoundSampleFrame Unknown, Memory<byte> data)>();

        /// <summary>
        /// Full sound buffer
        /// </summary>
        public Memory<byte> FullBuffer { get; set; }
    }
}
