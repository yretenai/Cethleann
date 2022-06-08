using DragonLib.CLI;

namespace Cethleann.ManagedFS.Options
{
    /// <summary>
    ///     Reisalin specific options
    /// </summary>
    public interface IReisalinOptions : IManagedFSOptionsBase
    {
        /// <summary>
        ///     Parse older, 32-bit Atelier PAKs
        /// </summary>
        [CLIFlag("pak-32bit", Help = "Parse older, 32-bit Atelier PAKs", Category = "Reisalin Options")]
        public bool PAKA17 { get; set; }
        /// <summary>
        ///     Parse older, 32-bit Atelier PAKs
        /// </summary>
        [CLIFlag("pak-keyfix", Help = "Parse Atelier PAKs with fixed key sizes", Category = "Reisalin Options")]
        public bool PAKKeyFix { get; set; }
    }
}
