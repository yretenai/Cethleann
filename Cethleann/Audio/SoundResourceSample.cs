using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;

namespace Cethleann.Audio
{
    public class SoundResourceSample : ISoundResourceSection
    {
        public SoundResourceSample(Span<byte> blob)
        {
            Header = MemoryMarshal.Read<SoundResourceEntryKTSS>(blob);
            Data = new SoundSource(blob.Slice(Header.HeaderSize, Header.KTSSSize));
        }

        public SoundResourceEntryKTSS Header { get; set; }

        public SoundSource Data { get; set; }
        public SoundResourceEntry Base => Header.Base;
    }
}
