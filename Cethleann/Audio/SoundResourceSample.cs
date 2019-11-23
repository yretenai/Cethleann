using System;
using System.Runtime.InteropServices;
using Cethleann.Koei.Structure.Resource.Audio;
using JetBrains.Annotations;

namespace Cethleann.Koei.Audio
{
    /// <summary>
    /// KTSS/KOVS Container found in KTSS containers
    /// </summary>
    [PublicAPI]
    public class SoundResourceSample : ISoundResourceSection
    {
        /// <summary>
        /// Initialize from a blob
        /// </summary>
        /// <param name="blob"></param>
        public SoundResourceSample(Span<byte> blob)
        {
            Header = MemoryMarshal.Read<SoundResourceEntryKTSS>(blob);
            Data = new SoundSource(blob.Slice(Header.HeaderSize, Header.KTSSSize));
        }

        /// <summary>
        /// KTSS/KOVS container header
        /// </summary>
        public SoundResourceEntryKTSS Header { get; set; }

        /// <summary>
        /// KTSS/KOVS Sound blob
        /// </summary>
        public SoundSource Data { get; set; }

        /// <inheritdoc />
        public SoundResourceEntry Base => Header.Base;
    }
}
