using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cethleann.Structure;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
using DragonLib.IO;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Implements KTSR
    /// </summary>
    [PublicAPI]
    public class SoundResource
    {
        /// <summary>
        ///     Initialize a KTSR from a Stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="platform"></param>
        public SoundResource(Stream stream, DataPlatform platform) : this(stream.ToSpan(), platform) { }

        /// <summary>
        ///     Initialize a KTSR from a Span buffer
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="platform"></param>
        public SoundResource(Span<byte> buffer, DataPlatform platform)
        {
            Platform = platform;
            Header = MemoryMarshal.Read<SoundResourceHeader>(buffer);
            var offset = SizeHelper.SizeOf<SoundResourceHeader>().Align(0x40);
            Logger.Assert(Header.Size == Header.CompressedSize, "Header.Size == Header.CompressedSize");
            while (offset < Header.Size)
            {
                var section = DecodeSection(buffer.Slice(offset), platform);
                Entries.Add(section);
                offset += section.Base.Size;
            }
        }

        /// <summary>
        ///     Used for platform-specific codecs
        /// </summary>
        public DataPlatform Platform { get; set; }

        /// <summary>
        ///     KTSR Header
        /// </summary>
        public SoundResourceHeader Header { get; set; }

        /// <summary>
        ///     Sound resources found in the KTSR container.
        /// </summary>
        public List<ISoundResourceSection> Entries { get; set; } = new List<ISoundResourceSection>();

        /// <summary>
        ///     Decodes KTSR Section
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="platform"></param>
        /// <returns></returns>
        public static ISoundResourceSection DecodeSection(Span<byte> buffer, DataPlatform platform)
        {
            var header = MemoryMarshal.Read<SoundResourceEntry>(buffer);
            var sectionData = buffer.Slice(0, header.Size);
            return header.SectionType switch
            {
                SoundResourceSectionType.SoundSample => (ISoundResourceSection) new OGGSound(sectionData),
                SoundResourceSectionType.ADPCMSound => (ISoundResourceSection) new NamedSounds(sectionData, platform),
                SoundResourceSectionType.GCADPCMSound when platform == DataPlatform.Switch => (ISoundResourceSection) new GCADPCMSound(sectionData),
                SoundResourceSectionType.GCADPCMSound when platform == DataPlatform.Windows => (ISoundResourceSection) new MSADPCMSound(sectionData),
                _ => new UnknownSound(sectionData)
            };
        }
    }
}
