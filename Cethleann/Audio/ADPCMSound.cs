using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using DragonLib;
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
            FullBuffer = new Memory<byte>(blob.ToArray());
            Header = MemoryMarshal.Read<ADPCMSoundHeader>(blob);
            Filenames = new List<string>(Header.Count);
            var offset = SizeHelper.SizeOf<ADPCMSoundHeader>();
            for (var i = 0; i < Header.Count; ++i)
            {
                var name = blob.Slice(offset).ReadString(null, false);
                offset += (name?.Length ?? 0) + 1;
                Filenames.Add(name);
            }
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
        
        /// <summary>
        ///     Filenames (if any)
        /// </summary>
        public List<string> Filenames { get; set; }

        /// <inheritdoc />
        public SoundResourceEntry Base => Header.Base;

        /// <inheritdoc />
        public Memory<byte> FullBuffer { get; set; }
    }
}
