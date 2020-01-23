using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Parser for General ADPCM Streams
    /// </summary>
    [PublicAPI]
    public class ADPCMSound : ISoundResourceSection
    {
        /// <summary>
        ///     Initialize from a blob
        /// </summary>
        /// <param name="blob"></param>
        public ADPCMSound(Span<byte> blob)
        {
            Header = MemoryMarshal.Read<ADPCMSoundHeader>(blob);
            var pointers = MemoryMarshal.Cast<byte, int>(blob.Slice(Header.PointersTablePointer, 4 * Header.Count));
            foreach (var pointer in pointers) Sections.Add(SoundResource.DecodeSection(blob.Slice(pointer)));
        }

        /// <summary>
        ///     ADPCM Header
        /// </summary>
        public ADPCMSoundHeader Header { get; set; }

        /// <summary>
        ///     Usually 1.
        /// </summary>
        public List<ISoundResourceSection> Sections { get; set; } = new List<ISoundResourceSection>();

        /// <inheritdoc />
        public SoundResourceEntry Base => Header.Base;
    }
}
