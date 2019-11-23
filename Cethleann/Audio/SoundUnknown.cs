using System;
using System.Runtime.InteropServices;
using Cethleann.Koei.Structure.Resource.Audio;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Koei.Audio
{
    /// <summary>
    /// Unknown Blob shim
    /// </summary>
    [PublicAPI]
    public class SoundUnknown : ISoundResourceSection
    {
        /// <summary>
        /// Initialize with a blob
        /// </summary>
        /// <param name="blob"></param>
        public SoundUnknown(Span<byte> blob)
        {
            Base = MemoryMarshal.Read<SoundResourceEntry>(blob);
            Logger.Warn("KTSR", $"SectionType {Base.SectionType:X} not processed");
            Data = new Memory<byte>(blob.Slice(SizeHelper.SizeOf<SoundResourceEntry>()).ToArray());
        }

        /// <summary>
        /// Data blob
        /// </summary>
        public Memory<byte> Data { get; set; }

        /// <inheritdoc />
        public SoundResourceEntry Base { get; set; }
    }
}
