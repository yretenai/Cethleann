using System;
using JetBrains.Annotations;

namespace Cethleann.Audio
{
    /// <summary>
    ///     WHD File parser
    /// </summary>
    [PublicAPI]
    public class KoeiWaveBank
    {
        /// <summary>
        ///     Parse with buffer data
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="alternateNames"></param>
        public KoeiWaveBank(Span<byte> buffer, bool alternateNames)
        {
            Resource = new RESPACK(buffer);
            WBH = new WaveBankHeader(Resource.Entries[0].Span, alternateNames);
            WBD = new WaveBankData(Resource.Entries[1].Span);
        }

        /// <summary>
        ///     Underlying RTRPK PAK
        /// </summary>
        public RESPACK Resource { get; set; }

        /// <summary>
        ///     WAVE Header data
        /// </summary>
        public WaveBankHeader WBH { get; set; }

        /// <summary>
        ///     WAVE Stream data
        /// </summary>
        public WaveBankData WBD { get; set; }
    }
}
