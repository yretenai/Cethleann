using System.Collections.Generic;
using Cethleann.Structure.WHD;

namespace Cethleann.Audio.WBH
{
    /// <summary>
    /// Abstract interface for WBH soundbanks
    /// </summary>
    public interface IWBHSoundbank
    {
        /// <summary>
        /// Get audio stream info entries
        /// </summary>
        public List<WBHEntry[]> Entries { get; }
        
        /// <summary>
        /// Get audio stream names
        /// </summary>
        public List<string> Names { get; }
    }
}
