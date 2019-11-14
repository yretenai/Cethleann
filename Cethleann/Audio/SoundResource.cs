using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
using DragonLib.IO;

namespace Cethleann.Audio
{
    public class SoundResource
    {
        public SoundResourceHeader Header { get; set; }
        
        public List<ISoundResourceSection> Entries { get; set; } = new List<ISoundResourceSection>();

        public SoundResource(Stream stream) : this(stream.ToSpan()) {}
        
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
                    SoundResourceSectionType.KTSS => (ISoundResourceSection) new SoundResourceSample(sectionData),
                    _ => new SoundUnknown(sectionData)
                };
                Entries.Add(section);
                offset += header.Size;
            }
        }
    }
}
