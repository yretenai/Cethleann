using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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
        public SoundResource(Stream stream) : this(stream.ToSpan()) { }

        /// <summary>
        ///     Initialize a KTSR from a Span buffer
        /// </summary>
        /// <param name="buffer"></param>
        public SoundResource(Span<byte> buffer)
        {
            Header = MemoryMarshal.Read<SoundResourceHeader>(buffer);
            var offset = SizeHelper.SizeOf<SoundResourceHeader>().Align(0x40);
            Logger.Assert(Header.Size == Header.CompressedSize, "Header.Size == Header.CompressedSize");
            while (offset < Header.Size)
            {
                var header = MemoryMarshal.Read<SoundResourceEntry>(buffer.Slice(offset));
                var sectionData = buffer.Slice(offset, header.Size);
                var section = header.SectionType switch
                {
                    SoundResourceSectionType.SoundSample => (ISoundResourceSection) new SoundResourceSample(sectionData),
                    _ => new SoundUnknown(sectionData)
                };
                Entries.Add(section);
                offset += header.Size;
            }
        }

        /// <summary>
        ///     KTSR Header
        /// </summary>
        public SoundResourceHeader Header { get; set; }

        /// <summary>
        ///     Sound resources found in the KTSR container.
        /// </summary>
        public List<ISoundResourceSection> Entries { get; set; } = new List<ISoundResourceSection>();
    }
}
