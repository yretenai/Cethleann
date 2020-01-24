using System;
using System.Runtime.InteropServices;
using Cethleann.Audio.WBH;
using Cethleann.Structure.WHD;
using DragonLib;

namespace Cethleann.Audio
{
    /// <summary>
    /// Binary Wave Headers
    /// </summary>
    public class WaveBinaryHeader
    {
        /// <summary>
        /// Initialize with buffer data
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="alternateNames"></param>
        public WaveBinaryHeader(Span<byte> buffer, bool alternateNames)
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
        /// Underlying header
        /// </summary>
        public WBHHeader Header { get; set; }
        
        /// <summary>
        /// Specific soundbank
        /// </summary>
        public IWBHSoundbank Soundbank { get; set; }
    }
}
