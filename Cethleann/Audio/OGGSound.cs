using System;
using System.Runtime.InteropServices;
using Cethleann.Structure.Resource.Audio;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     KTSS/KOVS Container found in KTSS containers
    /// </summary>
    [PublicAPI]
    public class OGGSound : ISoundResourceSection
    {
        /// <summary>
        ///     Initialize from a blob
        /// </summary>
        /// <param name="blob"></param>
        public OGGSound(Span<byte> blob)
        {
            FullBuffer = new Memory<byte>(blob.ToArray());
            Header = MemoryMarshal.Read<OGGSoundHeader>(blob);
            Data = new KTSSSound(blob.Slice(Header.HeaderSize, Header.KTSSSize));
        }

        /// <summary>
        ///     KTSS/KOVS container header
        /// </summary>
        public OGGSoundHeader Header { get; set; }

        /// <summary>
        ///     KTSS/KOVS Sound blob
        /// </summary>
        public KTSSSound Data { get; set; }

        /// <inheritdoc />
        public SoundResourceEntry Base => Header.Base;

        public Memory<byte> FullBuffer { get; set; }
    }
}
