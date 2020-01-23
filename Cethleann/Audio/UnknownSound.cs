using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Unknown Blob shim
    /// </summary>
    [PublicAPI]
    public class UnknownSound : ISoundResourceSection
    {
        /// <summary>
        ///     Initialize with a blob
        /// </summary>
        /// <param name="blob"></param>
        public UnknownSound(Span<byte> blob)
        {
            FullBuffer = new Memory<byte>(blob.ToArray());
            Base = MemoryMarshal.Read<SoundResourceEntry>(blob);
            Logger.Warn("KTSR", $"SectionType {Base.SectionType:X} not processed");
            Data = new Memory<byte>(blob.Slice(SizeHelper.SizeOf<SoundResourceEntry>()).ToArray());
        }

        /// <summary>
        ///     Data blob
        /// </summary>
        public Memory<byte> Data { get; set; }

        /// <inheritdoc />
        public SoundResourceEntry Base { get; set; }

        /// <inheritdoc />
        public Memory<byte> FullBuffer { get; set; }
    }
}
