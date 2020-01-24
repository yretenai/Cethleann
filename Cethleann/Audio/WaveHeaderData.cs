using System;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    /// WHD File parser
    /// </summary>
    [PublicAPI]
    public class WaveHeaderData
    {
        /// <summary>
        /// Parse with buffer data
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="alternateNames"></param>
        public WaveHeaderData(Span<byte> buffer, bool alternateNames)
        {
            RTRPK = new RTRPK(buffer);
            WBH = new WaveBinaryHeader(RTRPK.Entries[0].Span, alternateNames);
            WBD = new WaveBinaryData(RTRPK.Entries[1].Span);
        }

        /// <summary>
        /// Underlying RTRPK PAK
        /// </summary>
        public RTRPK RTRPK { get; set; }
        
        /// <summary>
        /// WAVE Header data
        /// </summary>
        public WaveBinaryHeader WBH { get; set; }
        
        /// <summary>
        /// WAVE Stream data
        /// </summary>
        public WaveBinaryData WBD { get; set; }
    }
}
