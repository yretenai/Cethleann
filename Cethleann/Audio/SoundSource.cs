using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
using DragonLib.IO;

namespace Cethleann.Audio
{
    public class SoundSource
    {
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

        public SoundSampleHeader Header { get; set; }
        public SoundSampleCodecHeader CodecHeader { get; set; }
        public byte[] ChannelFamily { get; set; }
        public List<(SoundSampleFrame frame, Memory<byte> data)> Packets { get; set; } = new List<(SoundSampleFrame Unknown, Memory<byte> data)>();
        public Memory<byte> FullBuffer { get; set; }
    }
}
