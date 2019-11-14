using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
using DragonLib.IO;

namespace Cethleann.Audio
{
    public class SoundUnknown : ISoundResourceSection
    {
        public SoundUnknown(Span<byte> blob)
        {
            Base = MemoryMarshal.Read<SoundResourceEntry>(blob);
            Logger.Warn("KTSR", $"SectionType {Base.SectionType:X} not processed");
            Data = new Memory<byte>(blob.Slice(SizeHelper.SizeOf<SoundResourceEntry>()).ToArray());
        }

        public Memory<byte> Data { get; set; }
        public SoundResourceEntry Base { get; set; }
    }
}
