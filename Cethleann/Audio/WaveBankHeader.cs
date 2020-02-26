using System;
using System.Runtime.InteropServices;
using Cethleann.Audio.WBH;
using Cethleann.Structure.Resource.Audio.WHD;
using DragonLib;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     Wave Bank Headers
    /// </summary>
    [PublicAPI]
    public class WaveBankHeader
    {
        /// <summary>
        ///     Initialize with buffer data
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="alternateNames"></param>
        public WaveBankHeader(Span<byte> buffer, bool alternateNames)
        {
            Header = MemoryMarshal.Read<WBHHeader>(buffer);
            var data = buffer.Slice(SizeHelper.SizeOf<WBHHeader>() - 4);
            Soundbank = Header.SoundbankType switch
            {
                WBHSoundbankType.KWB2 => new KWB2(data, alternateNames),
                _ => null
            };
        }

        /// <summary>
        ///     Underlying header
        /// </summary>
        public WBHHeader Header { get; set; }

        /// <summary>
        ///     Specific soundbank
        /// </summary>
        public IWBHSoundbank? Soundbank { get; set; }
    }
}
